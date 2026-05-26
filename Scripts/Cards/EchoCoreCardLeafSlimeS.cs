using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 树叶史莱姆（小）主动技：打出一记轻击，并制造一张 Slimed 体现史莱姆来源。
/// </summary>
public sealed class EchoCoreCardLeafSlimeS() : EchoCoreCard(1, CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        CardModel slimed = combatState.CreateCard<Slimed>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(slimed, PileType.Discard, addedByPlayer: true);
    }
}
