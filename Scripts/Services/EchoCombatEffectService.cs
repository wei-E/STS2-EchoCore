using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Sonata;
using MegaCrit.Sts2.Core.Entities.Players;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 声骸战斗效果服务。MVP 只实现“战斗开始时”这类被动词条，先把装备 -> 生效闭环跑通。
/// </summary>
public static class EchoCombatEffectService
{
    public sealed record ActiveSonataSummary(
        SonataDefinition Definition,
        int EquippedCount,
        IReadOnlyList<int> ActiveBreakpoints
    );

    public static async Task ApplyEquippedEchoStartOfCombatEffects(Player player)
    {
        foreach (var instance in EchoInventory.GetEquipped(player))
        {
            if (!EchoRegistry.TryGetEcho(instance.DefinitionId, out var definition))
            {
                continue;
            }

            if (EchoRegistry.TryGetEchoEffectHandler(definition.Id, out var echoHandler))
            {
                await echoHandler.OnCombatStart(player, instance, definition);
            }

            foreach (var affix in instance.Affixes)
            {
                if (EchoRegistry.TryGetAffixEffectHandler(affix.AffixId, out var handler))
                {
                    await handler.OnCombatStart(player, instance, affix);
                }
            }
        }

        foreach (var sonata in GetActiveSonataSummaries(player))
        {
            if (EchoRegistry.TryGetSonataEffectHandler(sonata.Definition.Id, out var handler))
            {
                await handler.OnCombatStart(player, sonata);
            }
        }
    }

    public static IReadOnlyList<ActiveSonataSummary> GetActiveSonataSummaries(Player player)
    {
        var equippedInstances = EchoInventory.GetEquipped(player)
            .ToList();

        var sonataCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var countedPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var instance in equippedInstances)
        {
            if (string.IsNullOrWhiteSpace(instance.SelectedSonataId))
            {
                continue;
            }

            // 同一声骸定义在同一套装中最多只计数一次，但不同套装归属仍可分别参与统计。
            var countedKey = $"{instance.SelectedSonataId}|{instance.DefinitionId}";
            if (!countedPairs.Add(countedKey))
            {
                continue;
            }

            sonataCounts[instance.SelectedSonataId] = sonataCounts.TryGetValue(instance.SelectedSonataId, out var count)
                ? count + 1
                : 1;
        }

        var summaries = new List<ActiveSonataSummary>();
        foreach (var pair in sonataCounts)
        {
            if (!EchoRegistry.TryGetSonata(pair.Key, out var sonataDefinition))
            {
                continue;
            }

            var activeBreakpoints = sonataDefinition.Breakpoints
                .Where(breakpoint => pair.Value >= breakpoint.RequiredCount)
                .Select(breakpoint => breakpoint.RequiredCount)
                .OrderBy(requiredCount => requiredCount)
                .ToList();

            if (activeBreakpoints.Count == 0)
            {
                continue;
            }

            summaries.Add(new ActiveSonataSummary(sonataDefinition, pair.Value, activeBreakpoints));
        }

        return summaries;
    }
}
