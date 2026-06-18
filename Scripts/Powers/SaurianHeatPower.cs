using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 绿熔蜥（稚形）的升温层数。
/// 只负责给玩家提供可见层数，具体强化判定仍由怪物自身处理。
/// </summary>
[CustomID("ECHO_CORE_SAURIAN_HEAT_POWER")]
public sealed class SaurianHeatPower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/monsters/wuwa/baby_viridblaze_saurian.webp";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "热势",
            "每次行动都会升温。达到 3 层后，下一次攻击会被强化。",
            "每次行动都会升温。达到 3 层后，下一次攻击会被强化。"),
        _ => new PowerLoc(
            "Heat",
            "This creature heats up whenever it acts. At 3 Heat, its next attack is empowered.",
            "This creature heats up whenever it acts. At 3 Heat, its next attack is empowered.")
    };
}
