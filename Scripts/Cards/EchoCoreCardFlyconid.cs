using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// Flyconid 主动技：孢子会根据目标状态在易伤与脆弱之间切换。
/// </summary>
public sealed class EchoCoreCardFlyconid() : EchoCoreCard(1, CardType.Skill, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Vulnerable", 2m),
        new DynamicVar("Frail", 2m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        if (cardPlay.Target.HasPower<VulnerablePower>())
        {
            await PowerCmd.Apply<FrailPower>(cardPlay.Target, DynamicVars["Frail"].BaseValue, Owner.Creature, this);
            return;
        }

        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars["Vulnerable"].BaseValue, Owner.Creature, this);
    }
}
