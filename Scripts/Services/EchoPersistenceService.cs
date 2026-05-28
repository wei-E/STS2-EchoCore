using System.Text.Json;
using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 负责把运行时内存态和 Run 存档中的 modifier 快照做双向同步。
/// 这样 EchoInventory / EchoTuningService 仍可保持简单内存接口，真正落盘逻辑集中在这里。
/// </summary>
public static class EchoPersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static RunState? _boundRunState;
    private static bool _isRestoring;

    public static void NotifyStateChanged(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player.RunState is not RunState runState)
        {
            return;
        }

        EnsureBoundRun(runState);
        Persist(runState);
    }

    public static void EnsurePlayerRunBound(MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player.RunState is RunState runState)
        {
            EnsureBoundRun(runState);
        }
    }

    public static void RestoreFromSavedModifier(RunState runState, EchoRunStateModifier modifier)
    {
        EnsureBoundRun(runState);
        _isRestoring = true;

        try
        {
            EchoInventory.ResetRuntime();
            EchoTuningService.ResetRuntime();

            if (string.IsNullOrWhiteSpace(modifier.EchoCoreSnapshotJson))
            {
                Log.Info("[EchoCore] Loaded empty echo persistence snapshot.");
                return;
            }

            EchoRunSnapshot? snapshot = JsonSerializer.Deserialize<EchoRunSnapshot>(modifier.EchoCoreSnapshotJson, JsonOptions);
            if (snapshot == null)
            {
                Log.Warn("[EchoCore] Echo persistence snapshot deserialized to null, ignoring.");
                return;
            }

            EchoInventory.RestoreFromSnapshot(
                snapshot.Players.Select(playerSnapshot => new EchoInventory.PlayerInventorySnapshot(
                    playerSnapshot.NetId,
                    playerSnapshot.Inventory.Select(ToRuntimeInstance).ToList(),
                    SanitizeEquippedIds(playerSnapshot.EquippedInstanceIds))).ToList());
            EchoTuningService.RestorePendingPlayers(snapshot.PendingTuningPlayerNetIds ?? []);

            Log.Info($"[EchoCore] Restored echo persistence snapshot. players={snapshot.Players.Count}, pendingTuning={snapshot.PendingTuningPlayerNetIds?.Count ?? 0}");
        }
        catch (Exception exception)
        {
            EchoInventory.ResetRuntime();
            EchoTuningService.ResetRuntime();
            Log.Error($"[EchoCore] Failed to restore echo persistence snapshot: {exception}");
        }
        finally
        {
            _isRestoring = false;
        }
    }

    private static void Persist(RunState runState)
    {
        if (_isRestoring)
        {
            return;
        }

        try
        {
            EchoRunStateModifier modifier = EnsureModifier(runState);
            EchoRunSnapshot snapshot = BuildSnapshot();
            string snapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions);
            modifier.SetSnapshot(snapshotJson);
            Log.Info($"[EchoCore] Persisted echo snapshot. chars={snapshotJson.Length}, players={snapshot.Players.Count}, pendingTuning={snapshot.PendingTuningPlayerNetIds.Count}");
        }
        catch (Exception exception)
        {
            Log.Error($"[EchoCore] Failed to persist echo state: {exception}");
        }
    }

    private static EchoRunSnapshot BuildSnapshot()
    {
        return new EchoRunSnapshot
        {
            Version = 1,
            Players = EchoInventory.ExportSnapshots()
                .Select(playerSnapshot => new EchoPlayerSnapshot
                {
                    NetId = playerSnapshot.NetId,
                    Inventory = playerSnapshot.Inventory.Select(ToSerializableInstance).ToList(),
                    EquippedInstanceIds = playerSnapshot.EquippedInstanceIds.ToList(),
                })
                .ToList(),
            PendingTuningPlayerNetIds = EchoTuningService.ExportPendingPlayers().ToList(),
        };
    }

    private static EchoRunStateModifier EnsureModifier(RunState runState)
    {
        EchoRunStateModifier? existingModifier = runState.Modifiers.OfType<EchoRunStateModifier>().FirstOrDefault();
        if (existingModifier != null)
        {
            Log.Info("[EchoCore] Reusing existing EchoRunStateModifier on run.");
            return existingModifier;
        }

        // 运行中首次接触声骸系统时再补挂 modifier，避免对未使用本系统的 run 平白增加存档负担。
        EchoRunStateModifier modifier = ModelDb.Modifier<EchoRunStateModifier>().ToMutable() as EchoRunStateModifier
            ?? throw new InvalidOperationException("Failed to create EchoRunStateModifier.");
        modifier.OnRunCreated(runState);
        runState.AddModifierDebug(modifier);
        Log.Info("[EchoCore] Added EchoRunStateModifier to current run.");
        return modifier;
    }

    private static void EnsureBoundRun(RunState runState)
    {
        if (ReferenceEquals(_boundRunState, runState))
        {
            return;
        }

        _boundRunState = runState;

        // 换 run 时先把旧的静态缓存清空，避免主菜单回到新局后仍带着上一局的内存态。
        EchoInventory.ResetRuntime();
        EchoTuningService.ResetRuntime();
    }

    private static string?[] SanitizeEquippedIds(IReadOnlyList<string?>? equippedInstanceIds)
    {
        string?[] slots = new string?[EchoInventory.MaxEquipSlots];
        if (equippedInstanceIds == null)
        {
            return slots;
        }

        for (int i = 0; i < Math.Min(equippedInstanceIds.Count, slots.Length); i++)
        {
            slots[i] = string.IsNullOrWhiteSpace(equippedInstanceIds[i]) ? null : equippedInstanceIds[i];
        }

        return slots;
    }

    private static EchoInstanceSnapshot ToSerializableInstance(EchoInstance instance)
    {
        return new EchoInstanceSnapshot
        {
            InstanceId = instance.InstanceId,
            DefinitionId = instance.DefinitionId,
            SelectedSonataId = instance.SelectedSonataId,
            Level = instance.Level,
            Locked = instance.Locked,
            Affixes = instance.Affixes.Select(affix => new EchoAffixSnapshot
            {
                AffixId = affix.AffixId,
                Value = affix.Value,
                Tier = affix.Tier,
                TierRarity = affix.TierRarity,
                Weight = affix.Weight,
                IsTuned = affix.IsTuned,
            }).ToList(),
            TuningCount = instance.TuningCount,
            AcquiredAtFloor = instance.AcquiredAtFloor,
            SourceType = instance.SourceType,
            SourceId = instance.SourceId,
        };
    }

    private static EchoInstance ToRuntimeInstance(EchoInstanceSnapshot snapshot)
    {
        return new EchoInstance(
            snapshot.InstanceId ?? Guid.NewGuid().ToString("N"),
            snapshot.DefinitionId ?? string.Empty,
            snapshot.SelectedSonataId,
            snapshot.Level,
            snapshot.Locked,
            (snapshot.Affixes ?? [])
            .Select(affix => new EchoAffixInstance(
                affix.AffixId ?? string.Empty,
                affix.Value,
                affix.Tier,
                affix.TierRarity,
                affix.Weight,
                affix.IsTuned))
            .ToList(),
            snapshot.TuningCount,
            snapshot.AcquiredAtFloor,
            snapshot.SourceType,
            snapshot.SourceId);
    }

    private sealed class EchoRunSnapshot
    {
        public int Version { get; set; }

        public List<EchoPlayerSnapshot> Players { get; set; } = [];

        public List<ulong> PendingTuningPlayerNetIds { get; set; } = [];
    }

    private sealed class EchoPlayerSnapshot
    {
        public ulong NetId { get; set; }

        public List<EchoInstanceSnapshot> Inventory { get; set; } = [];

        public List<string?> EquippedInstanceIds { get; set; } = [];
    }

    private sealed class EchoInstanceSnapshot
    {
        public string? InstanceId { get; set; }

        public string? DefinitionId { get; set; }

        public string? SelectedSonataId { get; set; }

        public int Level { get; set; }

        public bool Locked { get; set; }

        public List<EchoAffixSnapshot> Affixes { get; set; } = [];

        public int TuningCount { get; set; }

        public int AcquiredAtFloor { get; set; }

        public EchoSourceType SourceType { get; set; }

        public string? SourceId { get; set; }
    }

    private sealed class EchoAffixSnapshot
    {
        public string? AffixId { get; set; }

        public decimal Value { get; set; }

        public int Tier { get; set; }

        public EchoAffixTierRarity TierRarity { get; set; }

        public int Weight { get; set; }

        public bool IsTuned { get; set; }
    }
}
