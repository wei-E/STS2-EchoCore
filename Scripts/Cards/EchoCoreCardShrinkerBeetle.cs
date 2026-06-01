using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 缩小甲虫主动技：对目标施加缩小，持续指定回合。
/// </summary>
public sealed class EchoCoreCardShrinkerBeetle() : EchoCoreCard(3, CardType.Skill, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Shrink", 2m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await PowerCmd.Apply<ShrinkPower>(cardPlay.Target, DynamicVars["Shrink"].BaseValue, Owner.Creature, this);
    }
}
