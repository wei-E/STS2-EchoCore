using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// Calcified Cultist 主动技：把原怪开场咏唱压缩为一次可控的仪式成长。
/// </summary>
public sealed class EchoCoreCardCalcifiedCultist() : EchoCoreCard(1, CardType.Skill, TargetType.None, CardRarity.Uncommon)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Ritual", 1m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RitualPower>(Owner.Creature, DynamicVars["Ritual"].BaseValue, Owner.Creature, this);
    }
}
