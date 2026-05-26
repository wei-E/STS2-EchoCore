using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Entities.Players;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 声骸库存运行时缓存。
/// 真正的落盘由 EchoPersistenceService 负责，这里仍然只暴露简单的内存接口。
/// </summary>
public static class EchoInventory
{
    public const int MaxEquipSlots = 5;

    private static readonly Dictionary<ulong, List<EchoInstance>> EchoesByPlayerNetId = new();
    private static readonly Dictionary<ulong, string?[]> EquippedInstanceIdsByPlayerNetId = new();

    public static void Add(Player player, EchoInstance instance)
    {
        if (!EchoesByPlayerNetId.TryGetValue(player.NetId, out var inventory))
        {
            inventory = [];
            EchoesByPlayerNetId[player.NetId] = inventory;
        }

        inventory.Add(instance);
        EchoPersistenceService.NotifyStateChanged(player);
    }

    public static IReadOnlyList<EchoInstance> GetAll(Player player)
    {
        EnsurePlayerRunBound(player);
        return EchoesByPlayerNetId.TryGetValue(player.NetId, out var inventory)
            ? inventory
            : [];
    }

    public static IReadOnlyList<string?> GetEquippedInstanceIds(Player player)
    {
        EnsurePlayerRunBound(player);
        return GetOrCreateEquipSlots(player);
    }

    public static IReadOnlyList<EchoInstance> GetEquipped(Player player)
    {
        return GetOrCreateEquipSlots(player)
            .Select(slotInstanceId => FindByInstanceId(player, slotInstanceId))
            .OfType<EchoInstance>()
            .ToList();
    }

    public static bool Equip(Player player, EchoInstance instance, int slotIndex)
    {
        EnsurePlayerRunBound(player);
        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
        {
            return false;
        }

        var inventory = GetAll(player);
        if (!inventory.Any(echo => echo.InstanceId == instance.InstanceId))
        {
            return false;
        }

        var slots = GetOrCreateEquipSlots(player);

        // 同一个声骸只能占用一个装备槽，换槽时先从旧槽移除。
        for (var i = 0; i < slots.Length; i++)
        {
            if (slots[i] == instance.InstanceId)
            {
                slots[i] = null;
            }
        }

        slots[slotIndex] = instance.InstanceId;
        EchoPersistenceService.NotifyStateChanged(player);
        return true;
    }

    public static void Unequip(Player player, int slotIndex)
    {
        EnsurePlayerRunBound(player);
        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
        {
            return;
        }

        GetOrCreateEquipSlots(player)[slotIndex] = null;
        EchoPersistenceService.NotifyStateChanged(player);
    }

    public static EchoInstance? FindByInstanceId(Player player, string? instanceId)
    {
        EnsurePlayerRunBound(player);
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return null;
        }

        return GetAll(player).FirstOrDefault(echo => echo.InstanceId == instanceId);
    }

    public static bool IsEquipped(Player player, EchoInstance instance)
    {
        EnsurePlayerRunBound(player);
        return GetOrCreateEquipSlots(player).Any(id => id == instance.InstanceId);
    }

    public static bool ReplaceInstance(Player player, EchoInstance updatedInstance)
    {
        EnsurePlayerRunBound(player);
        if (!EchoesByPlayerNetId.TryGetValue(player.NetId, out var inventory))
        {
            return false;
        }

        var index = inventory.FindIndex(echo => echo.InstanceId == updatedInstance.InstanceId);
        if (index < 0)
        {
            return false;
        }

        inventory[index] = updatedInstance;
        EchoPersistenceService.NotifyStateChanged(player);
        return true;
    }

    internal static IReadOnlyList<PlayerInventorySnapshot> ExportSnapshots()
    {
        return EchoesByPlayerNetId
            .OrderBy(pair => pair.Key)
            .Select(pair => new PlayerInventorySnapshot(
                pair.Key,
                pair.Value.Select(CloneInstance).ToList(),
                CloneSlots(EquippedInstanceIdsByPlayerNetId.TryGetValue(pair.Key, out var slots) ? slots : null)))
            .ToList();
    }

    internal static void RestoreFromSnapshot(IReadOnlyList<PlayerInventorySnapshot> snapshots)
    {
        ResetRuntime();

        foreach (PlayerInventorySnapshot snapshot in snapshots)
        {
            EchoesByPlayerNetId[snapshot.NetId] = snapshot.Inventory.Select(CloneInstance).ToList();
            EquippedInstanceIdsByPlayerNetId[snapshot.NetId] = CloneSlots(snapshot.EquippedInstanceIds);
        }
    }

    internal static void ResetRuntime()
    {
        EchoesByPlayerNetId.Clear();
        EquippedInstanceIdsByPlayerNetId.Clear();
    }

    private static string?[] GetOrCreateEquipSlots(Player player)
    {
        if (!EquippedInstanceIdsByPlayerNetId.TryGetValue(player.NetId, out var slots))
        {
            slots = new string?[MaxEquipSlots];
            EquippedInstanceIdsByPlayerNetId[player.NetId] = slots;
        }

        return slots;
    }

    private static void EnsurePlayerRunBound(Player player)
    {
        EchoPersistenceService.EnsurePlayerRunBound(player);
    }

    private static EchoInstance CloneInstance(EchoInstance instance)
    {
        return instance with
        {
            Affixes = instance.Affixes.Select(affix => affix with { }).ToList(),
        };
    }

    private static string?[] CloneSlots(IReadOnlyList<string?>? sourceSlots)
    {
        string?[] result = new string?[MaxEquipSlots];
        if (sourceSlots == null)
        {
            return result;
        }

        for (int i = 0; i < Math.Min(result.Length, sourceSlots.Count); i++)
        {
            result[i] = sourceSlots[i];
        }

        return result;
    }

    internal sealed record PlayerInventorySnapshot(
        ulong NetId,
        IReadOnlyList<EchoInstance> Inventory,
        IReadOnlyList<string?> EquippedInstanceIds
    );
}
