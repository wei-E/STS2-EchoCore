using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 树叶史莱姆（小）主动技：0 费单体伤害并附加虚弱，同时制造 1 张黏液。
/// </summary>
public sealed class EchoCoreCardLeafSlimeS() : EchoCoreCard(0, CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("Weak", 1m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        CardModel slimed = combatState.CreateCard<Slimed>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(slimed, PileType.Discard, addedByPlayer: true);
    }
}
