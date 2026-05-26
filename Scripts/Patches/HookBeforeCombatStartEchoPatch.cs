using EchoCore.Scripts.Services;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;

namespace EchoCore.Scripts.Patches;

/// <summary>
/// 在原版战斗开始 Hook 结束后，应用当前玩家已装备声骸的开战词条。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class HookBeforeCombatStartEchoPatch
{
    [HarmonyPostfix]
    private static void ApplyEquippedEchoEffects(IRunState runState, CombatState? combatState, ref Task __result)
    {
        __result = ApplyAfterOriginalHooks(__result, runState, combatState);
    }

    private static async Task ApplyAfterOriginalHooks(Task originalTask, IRunState runState, CombatState? combatState)
    {
        await originalTask;

        if (combatState == null)
        {
            return;
        }

        var player = LocalContext.GetMe(runState.Players);
        if (player == null || player.Creature == null || player.Creature.CombatState != combatState)
        {
            return;
        }

        EchoActiveSkillService.ResetForCombat(combatState);
        await EchoCombatEffectService.ApplyEquippedEchoStartOfCombatEffects(player);
        Log.Info($"[EchoCore] Applied equipped echo start-of-combat effects. equipped={EchoInventory.GetEquippedInstanceIds(player).Count(id => !string.IsNullOrWhiteSpace(id))}");
    }
}
