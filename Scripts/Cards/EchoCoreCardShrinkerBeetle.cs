using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 缩小甲虫主动技：MVP 先用伤害加虚弱表达“缩小/削弱”。
/// </summary>
public sealed class EchoCoreCardShrinkerBeetle() : EchoCoreCard(1, CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new DynamicVar("Weak", 1m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
    }
}
