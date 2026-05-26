using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 多尼斯异鸟主动技：兼具防守和攻击，体现精英声骸的主动技价值。
/// </summary>
public sealed class EchoCoreCardByrdonis() : EchoCoreCard(1, CardType.Attack, TargetType.AnyEnemy, CardRarity.Uncommon)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new BlockVar(6m, ValueProp.Move),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }
}
