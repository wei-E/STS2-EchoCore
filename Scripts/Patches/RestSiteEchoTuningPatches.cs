using EchoCore.Scripts.Content;
using EchoCore.Scripts.RestSite;
using EchoCore.Scripts.Services;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Runs;

namespace EchoCore.Scripts.Patches;

/// <summary>
/// 把 Echo Core 的调谐入口挂到火堆选项里，并为自定义按钮补上可见图标。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyRestSiteOptions))]
public static class RestSiteEchoTuningOptionPatch
{
    [HarmonyPostfix]
    private static void AddEchoTuneOption(IRunState runState, Player player, ICollection<RestSiteOption> options)
    {
        if (options.Any(option => string.Equals(option.OptionId, EchoTuningService.RestSiteOptionId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        options.Add(new EchoTuneRestSiteOption(player));
    }
}

[HarmonyPatch(typeof(NRestSiteButton), "Reload")]
public static class RestSiteEchoTuningButtonVisualPatch
{
    [HarmonyPostfix]
    private static void OverrideEchoTuneButtonIcon(NRestSiteButton __instance)
    {
        if (!string.Equals(__instance.Option.OptionId, EchoTuningService.RestSiteOptionId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 原版火堆按钮会按固定命名规则找图；调谐选项先强制替换成声骸默认图标，避免出现缺图按钮。
        var icon = __instance.GetNodeOrNull<TextureRect>("%Icon");
        if (icon != null)
        {
            icon.Texture = PreloadManager.Cache.GetCompressedTexture2D(EchoContentConstants.DefaultIconPath);
        }
    }
}
