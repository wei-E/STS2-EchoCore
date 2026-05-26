using EchoCore.Scripts.Services;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace EchoCore.Scripts.RestSite;

/// <summary>
/// 火堆里的调谐入口。点击后开启一次调谐模式，实际选择与重骰在声骸面板里完成。
/// </summary>
public sealed class EchoTuneRestSiteOption : RestSiteOption
{
    public override string OptionId => EchoTuningService.RestSiteOptionId;

    public EchoTuneRestSiteOption(Player owner)
        : base(owner)
    {
        // 只有背包里至少有一个声骸时，才展示可用的调谐选项。
        IsEnabled = EchoTuningService.CanOpenTuning(owner);
    }

    public override Task<bool> OnSelect()
    {
        if (!EchoTuningService.CanOpenTuning(Owner))
        {
            return Task.FromResult(false);
        }

        EchoTuningService.BeginTuningMode(Owner);
        UI.EchoInventoryOverlay.OpenForTuning();
        return Task.FromResult(true);
    }
}
