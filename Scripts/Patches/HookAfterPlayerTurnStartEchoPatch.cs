using EchoCore.Scripts.Services;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace EchoCore.Scripts.Patches;

/// <summary>
/// 玩家回合开始时推进声骸主动技冷却。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class HookAfterPlayerTurnStartEchoPatch
{
    [HarmonyPostfix]
    private static void TickEchoSkillCooldown(CombatState combatState, PlayerChoiceContext choiceContext, Player player, ref Task __result)
    {
        __result = TickAfterOriginalHooks(__result, player);
    }

    private static async Task TickAfterOriginalHooks(Task originalTask, Player player)
    {
        await originalTask;
        EchoActiveSkillService.OnPlayerTurnStart(player);
    }
}
