using System.Linq;
using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Rewards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 声骸掉落服务。当前统一 50% 掉落。
/// </summary>
public static class EchoDropService
{
    private const int DropRatePercent = 50;

    public static bool TryAppendEchoReward(RewardsSet rewardsSet)
    {
        if (rewardsSet.Room is not CombatRoom combatRoom)
        {
            return false;
        }

        // 避免同一份奖励列表被重复处理时追加多个声骸。
        if (rewardsSet.Rewards.OfType<EchoReward>().Any())
        {
            return false;
        }

        if (!TryCreateEchoInstanceForCombat(rewardsSet.Player, combatRoom, out var definition, out var instance))
        {
            return false;
        }

        rewardsSet.Rewards.Add(new EchoReward(definition, instance, rewardsSet.Player));
        return true;
    }

    private static bool TryCreateEchoInstanceForCombat(Player player, CombatRoom combatRoom, out EchoDefinition definition, out EchoInstance instance)
    {
        definition = null!;
        instance = null!;

        if (!ShouldDrop(player.PlayerRng.Rewards))
        {
            return false;
        }

        var candidateDefinitions = combatRoom.Encounter.MonstersWithSlots
            .Select(pair => pair.Item1.Id)
            .SelectMany(GetMonsterLookupKeys)
            .SelectMany(key => EchoRegistry.TryGetEchoByMonsterId(key, out var echo) ? [echo] : Array.Empty<EchoDefinition>())
            .DistinctBy(echo => echo.Id)
            .ToList();

        if (candidateDefinitions.Count == 0)
        {
            return false;
        }

        definition = player.PlayerRng.Rewards.NextItem(candidateDefinitions)!;
        instance = CreateInstance(player, definition);
        return true;
    }

    private static bool ShouldDrop(Rng rng)
    {
        return DropRatePercent >= 100 || rng.NextInt(100) < DropRatePercent;
    }

    private static EchoInstance CreateInstance(Player player, EchoDefinition definition)
    {
        var affixes = RollAffixes(player.PlayerRng.Rewards, count: 1);
        var floor = player.RunState?.ActFloor ?? 0;
        var selectedSonataId = RollSelectedSonataId(player.PlayerRng.Rewards, definition);

        return new EchoInstance(
            InstanceId: $"echo-{Guid.NewGuid():N}",
            DefinitionId: definition.Id,
            SelectedSonataId: selectedSonataId,
            Level: 0,
            Locked: false,
            Affixes: affixes,
            TuningCount: 0,
            AcquiredAtFloor: floor,
            SourceType: EchoSourceType.CombatDrop,
            SourceId: definition.SourceMonsterId);
    }

    private static string? RollSelectedSonataId(Rng rng, EchoDefinition definition)
    {
        if (definition.SonataIds.Count == 0)
        {
            return null;
        }

        if (definition.SonataIds.Count == 1)
        {
            return definition.SonataIds[0];
        }

        // 一个声骸定义可以挂多个候选合鸣，但单个掉落实例只能最终归属其中一个。
        return rng.NextItem(definition.SonataIds)!;
    }

    private static IReadOnlyList<EchoAffixInstance> RollAffixes(Rng rng, int count)
    {
        var affixes = EchoRegistry.Affixes.ToList();
        if (affixes.Count == 0)
        {
            return [];
        }

        var result = new List<EchoAffixInstance>(count);
        for (var i = 0; i < count; i++)
        {
            var affix = rng.NextItem(affixes)!;
            var tier = RollWeightedTier(rng, affix.Tiers);
            result.Add(new EchoAffixInstance(affix.Id, tier.Value, tier.Tier, tier.Rarity, tier.Weight, IsTuned: false));
        }

        return result;
    }

    private static EchoAffixTierDefinition RollWeightedTier(Rng rng, IReadOnlyList<EchoAffixTierDefinition> tiers)
    {
        var totalWeight = tiers.Sum(tier => tier.Weight);
        var roll = rng.NextInt(totalWeight);

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

    private static IEnumerable<string> GetMonsterLookupKeys(ModelId modelId)
    {
        // 兼容注册表可能使用纯 Entry 或完整 ModelId 的两种写法。
        yield return modelId.Entry;
        yield return modelId.ToString();
    }
}
