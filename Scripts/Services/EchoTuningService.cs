using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 声骸调谐服务。MVP 先做“火堆开启一次调谐机会 -> 选择一个声骸重骰唯一词条”。
/// </summary>
public static class EchoTuningService
{
    public const string RestSiteOptionId = "ECHO_TUNE";

    private static readonly HashSet<ulong> PlayersWithPendingTuning = new();

    public static bool CanOpenTuning(Player player)
    {
        return EchoInventory.GetAll(player).Count > 0;
    }

    public static bool IsTuningModeActive(Player player)
    {
        return PlayersWithPendingTuning.Contains(player.NetId);
    }

    public static void BeginTuningMode(Player player)
    {
        PlayersWithPendingTuning.Add(player.NetId);
        EchoPersistenceService.NotifyStateChanged(player);
    }

    public static void EndTuningMode(Player player)
    {
        PlayersWithPendingTuning.Remove(player.NetId);
        EchoPersistenceService.NotifyStateChanged(player);
    }

    public static int GetTuningCost(EchoInstance instance)
    {
        if (!EchoRegistry.TryGetEcho(instance.DefinitionId, out var definition))
        {
            return 50;
        }

        return definition.Class switch
        {
            EchoClass.Common => 50,
            EchoClass.Elite => 75,
            EchoClass.Overlord => 100,
            EchoClass.Calamity => 125,
            _ => 50,
        };
    }

    public static bool CanTune(Player player, EchoInstance instance)
    {
        if (!IsTuningModeActive(player))
        {
            return false;
        }

        if (instance.Affixes.Count == 0)
        {
            return false;
        }

        return player.Gold >= GetTuningCost(instance);
    }

    public static async Task<bool> TryTuneEcho(Player player, EchoInstance instance)
    {
        if (!CanTune(player, instance))
        {
            return false;
        }

        var cost = GetTuningCost(instance);
        var rerolledAffix = RollReplacementAffix(player, instance);
        var updatedInstance = instance with
        {
            Affixes = [rerolledAffix],
            TuningCount = instance.TuningCount + 1,
        };

        await PlayerCmd.LoseGold(cost, player, GoldLossType.Spent);
        EchoInventory.ReplaceInstance(player, updatedInstance);

        // MVP 中一次火堆调谐只允许完成一次，和传统火堆选项一致。
        EndTuningMode(player);

        Log.Info($"[EchoCore] Tuned echo. instance={instance.InstanceId}, affix={instance.Affixes[0].AffixId}->{rerolledAffix.AffixId}, tier={instance.Affixes[0].Tier}->{rerolledAffix.Tier}, cost={cost}");
        return true;
    }

    internal static IReadOnlyCollection<ulong> ExportPendingPlayers()
    {
        return PlayersWithPendingTuning.ToArray();
    }

    internal static void RestorePendingPlayers(IEnumerable<ulong> playerNetIds)
    {
        PlayersWithPendingTuning.Clear();
        foreach (ulong playerNetId in playerNetIds)
        {
            PlayersWithPendingTuning.Add(playerNetId);
        }
    }

    internal static void ResetRuntime()
    {
        PlayersWithPendingTuning.Clear();
    }

    private static EchoAffixInstance RollReplacementAffix(Player player, EchoInstance instance)
    {
        var previousAffix = instance.Affixes[0];

        // 为了避免“调谐后完全没变化”的糟糕反馈，先尝试多次重骰，只要结果和旧词条不同就接受。
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var candidate = RollSingleAffix(player);
            if (!IsSameAffixRoll(candidate, previousAffix))
            {
                return candidate with { IsTuned = true };
            }
        }

        return RollSingleAffix(player) with { IsTuned = true };
    }

    private static EchoAffixInstance RollSingleAffix(Player player)
    {
        var affixDefinitions = EchoRegistry.Affixes.ToList();
        var affixDefinition = player.PlayerRng.Rewards.NextItem(affixDefinitions)!;
        var tierDefinition = RollWeightedTier(player, affixDefinition.Tiers);
        return new EchoAffixInstance(
            affixDefinition.Id,
            tierDefinition.Value,
            tierDefinition.Tier,
            tierDefinition.Rarity,
            tierDefinition.Weight,
            IsTuned: true);
    }

    private static EchoAffixTierDefinition RollWeightedTier(Player player, IReadOnlyList<EchoAffixTierDefinition> tiers)
    {
        var totalWeight = tiers.Sum(tier => tier.Weight);
        var roll = player.PlayerRng.Rewards.NextInt(totalWeight);

        foreach (var tier in tiers)
        {
            roll -= tier.Weight;
            if (roll < 0)
            {
                return tier;
            }
        }

        return tiers[^1];
    }

    private static bool IsSameAffixRoll(EchoAffixInstance left, EchoAffixInstance right)
    {
        return string.Equals(left.AffixId, right.AffixId, StringComparison.OrdinalIgnoreCase)
            && left.Tier == right.Tier
            && left.Value == right.Value;
    }
}
