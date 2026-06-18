using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 踏光兽的半血阶段标记。
/// 主要用于向玩家提示其已进入更激进的攻击节奏。
/// </summary>
[CustomID("ECHO_CORE_LIGHTTREADER_ENRAGED_POWER")]
public sealed class LighttreaderEnragedPower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/monsters/wuwa/lighttreader_beast.webp";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "暴躁",
            "进入半血暴躁态，会更频繁地发动高压攻击。",
            "进入半血暴躁态，会更频繁地发动高压攻击。"),
        _ => new PowerLoc(
            "Lighttread Frenzy",
            "Has entered its low-health frenzy and will favor more dangerous attacks.",
            "Has entered its low-health frenzy and will favor more dangerous attacks.")
    };
}
