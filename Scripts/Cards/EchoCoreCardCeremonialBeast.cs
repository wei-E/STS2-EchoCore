using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 仪式兽主动技：Boss 声骸主动技，MVP 先做高伤害并给予易伤。
/// </summary>
public sealed class EchoCoreCardCeremonialBeast() : EchoCoreCard(2, CardType.Attack, TargetType.AnyEnemy, CardRarity.Rare)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14m, ValueProp.Move),
        new DynamicVar("Vulnerable", 1m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars["Vulnerable"].BaseValue, Owner.Creature, this);
    }
}
