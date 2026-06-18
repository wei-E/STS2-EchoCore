using EchoCore.Scripts.UI;
using EchoCore.Scripts.Config;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;

namespace EchoCore.Scripts.Patches;

/// <summary>
/// 进入一局游戏后挂载声骸库存入口。只负责 UI 入口，不影响原版 NRun 初始化。
/// </summary>
[HarmonyPatch(typeof(NRun), nameof(NRun._Ready))]
public static class NRunEchoInventoryOverlayPatch
{
    public static void Postfix(NRun __instance)
    {
        try
        {
            EchoInventoryOverlay.AttachTo(__instance);
            Log.Info("[EchoCore] Attached echo inventory overlay.");
        }
        catch (Exception ex)
        {
            Log.Error($"[EchoCore] Failed to attach echo inventory overlay: {ex}");
        }

        if (!EchoDeveloperConfig.EnableEchoDeveloperMenu)
        {
            return;
        }

        try
        {
            EchoDeveloperMenuHost.AttachTo(__instance);
            Log.Info("[EchoCore] Attached echo developer menu host.");
        }
        catch (Exception ex)
        {
            Log.Error($"[EchoCore] Failed to attach echo developer menu host: {ex}");
        }
    }
}
