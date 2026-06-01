using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace EchoCore.Scripts.Powers;

/// <summary>
/// 不绝余音 5 件：当本次打出的攻击牌满足“手牌里没有其他攻击牌”时，额外再打出一次。
/// </summary>
[CustomID("ECHO_CORE_ENDLESS_ECHO_POWER")]
public sealed class EndlessEchoPower : CustomPowerModel
{
    private const string IconTexturePath = "res://echo-core/ui/echoes/icons/default_echo_icon.png";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => IconTexturePath;

    public override string CustomBigIconPath => IconTexturePath;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc(
            "不绝余音",
            "当你打出攻击牌时，若手牌里没有其他攻击牌，则将其额外打出一次。",
            "当你打出攻击牌时，若手牌里没有其他攻击牌，则将其额外打出一次。"),
        _ => new PowerLoc(
            "Endless Echo",
            "When you play an Attack, if there are no other Attacks in your hand, play it an additional time.",
            "When you play an Attack, if there are no other Attacks in your hand, play it an additional time.")
    };

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Owner.Creature != Owner || card.Type != CardType.Attack)
        {
            return playCount;
        }

        var handPile = PileType.Hand.GetPile(card.Owner);
        bool hasOtherAttack = handPile.Cards.Any(handCard => handCard.Type == CardType.Attack);
        if (hasOtherAttack)
        {
            return playCount;
        }

        return playCount + 1;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (card.Owner.Creature == Owner && card.Type == CardType.Attack)
        {
            Flash();
        }

        return Task.CompletedTask;
    }
}
