using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 裂变幼岩小体的可见死亡爆炸提示。
/// 伤害逻辑放在 Power 中，保证玩家悬停图标就能读到死亡惩罚。
/// </summary>
[CustomID("ECHO_CORE_FISSION_BURST_ON_DEATH_POWER")]
public sealed class FissionBurstOnDeathPower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/monsters/wuwa/fission_junrock.webp";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "裂爆",
            "死亡时，对所有玩家造成伤害。",
            "死亡时，对所有玩家造成伤害。"),
        _ => new PowerLoc(
            "Fission Burst",
            "On death, deal damage to all players. The number under this icon is the damage.",
            "On death, deal damage to all players. The number under this icon is the damage.")
    };

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        await base.AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
        if (creature != Owner || wasRemovalPrevented || CombatState == null)
        {
            return;
        }

        foreach (Creature playerCreature in CombatState.Players.Select(player => player.Creature).Where(playerCreature => playerCreature.IsAlive))
        {
            await CreatureCmd.Damage(choiceContext, playerCreature, Amount, ValueProp.Unpowered, null, null);
        }
    }
}
