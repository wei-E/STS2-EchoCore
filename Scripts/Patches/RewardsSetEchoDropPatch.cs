using EchoCore.Scripts.Services;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rewards;

namespace EchoCore.Scripts.Patches;

/// <summary>
/// 把声骸掉落挂到原版房间奖励生成之后，保证奖励界面直接显示 1 个声骸。
/// </summary>
[HarmonyPatch(typeof(RewardsSet), nameof(RewardsSet.WithRewardsFromRoom))]
public static class RewardsSetEchoDropPatch
{
    public static void Postfix(RewardsSet __instance)
    {
        try
        {
            if (EchoDropService.TryAppendEchoReward(__instance))
            {
                Log.Info("[EchoCore] Appended echo reward to room rewards.");
            }
        }
        catch (Exception ex)
        {
            // 掉落系统不能阻断原版奖励，异常先写日志，方便进游戏定位。
            Log.Error($"[EchoCore] Failed to append echo reward: {ex}");
        }
    }
}
