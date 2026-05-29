using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// Chomper 主动技：获得人工制品，并向弃牌堆加入 1 张眩晕。
/// 这样既保留硬壳怪的抗性特色，也保留原怪 Screech 的副作用感。
/// </summary>
public sealed class EchoCoreCardChomper() : EchoCoreCard(1, CardType.Skill, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Artifact", 1m),
        new DynamicVar("Dazed", 1m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ArtifactPower>(Owner.Creature, DynamicVars["Artifact"].BaseValue, Owner.Creature, this);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(
            Owner.Creature,
            PileType.Discard,
            DynamicVars["Dazed"].IntValue,
            addedByPlayer: true);
    }
}
