using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 裂变幼岩分裂资格标记。
/// 仅本体携带，小体不带，用于避免无限分裂。
/// </summary>
[CustomID("ECHO_CORE_FISSION_SPLIT_POWER")]
public sealed class FissionSplitPower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/monsters/wuwa/fission_junrock.webp";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "裂变核",
            "死亡时分裂为两只较小的幼岩。",
            "死亡时分裂为两只较小的幼岩。"),
        _ => new PowerLoc(
            "Fission Core",
            "On death, split into two smaller junrocks.",
            "On death, split into two smaller junrocks.")
    };

    public override bool ShouldStopCombatFromEnding()
    {
        return Owner != null && Owner.IsAlive;
    }
}
