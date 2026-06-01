using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 隐世回光 5 件：本场战斗中下一次受到致命伤害时，保留 1 点生命。
/// </summary>
[CustomID("ECHO_CORE_HIDDEN_LIGHT_REVIVE_POWER")]
public sealed class HiddenLightRevivePower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/echoes/icons/default_echo_icon.png";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "回光",
            "当你受到致命伤害时，改为保留 1 点生命，然后移除。",
            "当你受到致命伤害时，改为保留 1 点生命，然后移除。"),
        _ => new PowerLoc(
            "Hidden Light",
            "When you would take fatal damage, survive with 1 HP instead, then remove this.",
            "When you would take fatal damage, survive with 1 HP instead, then remove this.")
    };

    public override bool ShouldDieLate(Creature creature)
    {
        return creature != Owner;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(creature, 1m);
        await PowerCmd.Remove(this);
    }
}
