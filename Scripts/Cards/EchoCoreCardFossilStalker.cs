using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// Fossil Stalker 主动技：先压出脆弱，再对已经松动的目标追加鞭击。
/// </summary>
public sealed class EchoCoreCardFossilStalker() : EchoCoreCard(1, CardType.Attack, TargetType.AnyEnemy, CardRarity.Uncommon)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("Frail", 1m),
        new DynamicVar("LashDamage", 3m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        bool wasFrail = cardPlay.Target.HasPower<FrailPower>();
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<FrailPower>(cardPlay.Target, DynamicVars["Frail"].BaseValue, Owner.Creature, this);

        if (wasFrail && cardPlay.Target.IsAlive)
        {
            await DamageCmd.Attack(DynamicVars["LashDamage"].BaseValue).WithHitCount(2).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        }
    }
}
