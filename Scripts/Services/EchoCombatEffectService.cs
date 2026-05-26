using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Sonata;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

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
            foreach (var affix in instance.Affixes)
            {
                await ApplyStartOfCombatAffix(player, instance, affix);
            }
        }

        foreach (var sonata in GetActiveSonataSummaries(player))
        {
            await ApplyStartOfCombatSonata(player, sonata);
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

    private static async Task ApplyStartOfCombatAffix(Player player, EchoInstance instance, Affixes.EchoAffixInstance affix)
    {
        switch (affix.AffixId)
        {
            case "echo_core:strength_start":
                await PowerCmd.Apply<StrengthPower>(player.Creature, affix.Value, player.Creature, null);
                LogAppliedAffix(instance, affix, "Strength");
                return;

            case "echo_core:dexterity_start":
                await PowerCmd.Apply<DexterityPower>(player.Creature, affix.Value, player.Creature, null);
                LogAppliedAffix(instance, affix, "Dexterity");
                return;

            case "echo_core:block_start":
                await CreatureCmd.GainBlock(player.Creature, affix.Value, ValueProp.Unpowered, null);
                LogAppliedAffix(instance, affix, "Block");
                return;
        }
    }

    private static async Task ApplyStartOfCombatSonata(Player player, ActiveSonataSummary sonata)
    {
        if (string.Equals(sonata.Definition.Id, VanillaEchoBootstrap.UniversalSonataId, StringComparison.OrdinalIgnoreCase))
        {
            await ApplyUniversalStartOfCombatSonata(player, sonata);
            return;
        }

        if (string.Equals(sonata.Definition.Id, VanillaEchoBootstrap.HiddenLightSonataId, StringComparison.OrdinalIgnoreCase))
        {
            await ApplyHiddenLightStartOfCombatSonata(player, sonata);
        }
    }

    private static async Task ApplyUniversalStartOfCombatSonata(Player player, ActiveSonataSummary sonata)
    {
        // MVP 先只实现基础残响的 2/3/5 件通用增益，确保合鸣件数在战斗内有真实收益。
        foreach (var breakpoint in sonata.ActiveBreakpoints)
        {
            switch (breakpoint)
            {
                case 2:
                    await CreatureCmd.GainBlock(player.Creature, 4m, ValueProp.Unpowered, null);
                    break;

                case 3:
                    await PowerCmd.Apply<StrengthPower>(player.Creature, 1m, player.Creature, null);
                    break;

                case 5:
                    await PowerCmd.Apply<DexterityPower>(player.Creature, 1m, player.Creature, null);
                    break;
            }

            Log.Info($"[EchoCore] Applied sonata effect. sonata={sonata.Definition.Id}, equipped={sonata.EquippedCount}, breakpoint={breakpoint}");
        }
    }

    private static async Task ApplyHiddenLightStartOfCombatSonata(Player player, ActiveSonataSummary sonata)
    {
        foreach (var breakpoint in sonata.ActiveBreakpoints)
        {
            switch (breakpoint)
            {
                case 2:
                    await CreatureCmd.Heal(player.Creature, 1m);
                    break;

                case 3:
                    await CreatureCmd.GainBlock(player.Creature, 3m, ValueProp.Unpowered, null);
                    break;

                case 5:
                    await PowerCmd.Apply<DexterityPower>(player.Creature, 1m, player.Creature, null);
                    break;
            }

            Log.Info($"[EchoCore] Applied sonata effect. sonata={sonata.Definition.Id}, equipped={sonata.EquippedCount}, breakpoint={breakpoint}");
        }
    }

    private static void LogAppliedAffix(EchoInstance instance, Affixes.EchoAffixInstance affix, string effectType)
    {
        // 保留简洁日志，方便验证装备词条是否在开战时实际触发。
        var displayName = EchoRegistry.TryGetEcho(instance.DefinitionId, out var definition)
            ? definition.Id
            : instance.DefinitionId;
        Log.Info($"[EchoCore] Applied echo affix effect. echo={displayName}, affix={affix.AffixId}, tier={affix.Tier}, value={affix.Value}, effect={effectType}");
    }
}
