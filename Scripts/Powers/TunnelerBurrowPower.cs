using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 地道虫主动技专属 Power。
/// 生效期间本回合不能打出攻击牌；到下个我方回合开始时，对随机敌人造成伤害后移除自身。
/// </summary>
[CustomID("ECHO_CORE_TUNNELER_BURROW_POWER")]
public sealed class TunnelerBurrowPower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/echoes/icons/default_echo_icon.png";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "钻地伏击",
            "本回合不能打出攻击牌。下回合开始时，对随机敌人造成 20 点伤害，然后移除。",
            "本回合不能打出攻击牌。下回合开始时，对随机敌人造成 20 点伤害，然后移除。"),
        _ => new PowerLoc(
            "Burrow Ambush",
            "You cannot play Attacks this turn. At the start of your next turn, deal 20 damage to a random enemy, then remove this.",
            "You cannot play Attacks this turn. At the start of your next turn, deal 20 damage to a random enemy, then remove this.")
    };

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card.Owner.Creature != Owner)
        {
            return true;
        }

        return card.Type != CardType.Attack;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        var hittableEnemies = Owner.CombatState?.HittableEnemies;
        if (hittableEnemies == null || hittableEnemies.Count == 0)
        {
            await PowerCmd.Remove(this);
            return;
        }

        Flash();
        Creature? target = player.RunState.Rng.CombatTargets.NextItem(hittableEnemies);
        if (target == null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered, Owner, null);
        await PowerCmd.Remove(this);
    }
}
