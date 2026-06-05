using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 碎獠猪主动技：多段冲锋，若目标已被压制则追加一口。
/// </summary>
public sealed class EchoCoreCardSabyrBoar() : EchoCoreCard(1, CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HitDamage", 6m),
        new DynamicVar("Finisher", 4m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        bool shouldFinish = cardPlay.Target.HasPower<VulnerablePower>() || cardPlay.Target.HasPower<WeakPower>() || cardPlay.Target.HasPower<FrailPower>();
        await DamageCmd.Attack(DynamicVars["HitDamage"].BaseValue).WithHitCount(2).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);

        if (shouldFinish && cardPlay.Target.IsAlive)
        {
            await DamageCmd.Attack(DynamicVars["Finisher"].BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        }
    }
}
