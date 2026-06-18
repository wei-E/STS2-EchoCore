using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Afflictions;

/// <summary>
/// 踏光兽施加的轻量卡牌异常。
/// 第一版先用额外卡牌文字、费用提升和保留来保证规则可读。
/// </summary>
[CustomID("ECHO_CORE_STAGGERED_AFFLICTION")]
public sealed class LighttreaderStaggeredAffliction : AfflictionModel, ILocalizationProvider
{
    private bool _appliedRetainKeyword;

    public override bool HasExtraCardText => true;

    public string? LocTable => "afflictions";

    public List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardModifierLoc(
            "震慑",
            "费用增加 1。保留。打出后，所有敌人获得 1 点力量。",
            "震慑"),
        _ => new CardModifierLoc(
            "Staggered",
            "Costs 1 more. Retain. When played, all enemies gain 1 Strength.",
            "Staggered")
    };

    public bool AppliedRetainKeyword
    {
        get => _appliedRetainKeyword;
        set
        {
            AssertMutable();
            _appliedRetainKeyword = value;
        }
    }

    public override void AfterApplied()
    {
        if (!Card.Keywords.Contains(CardKeyword.Retain))
        {
            CardCmd.ApplyKeyword(Card, CardKeyword.Retain);
            AppliedRetainKeyword = true;
        }
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<StrengthPower>(CombatState.HittableEnemies, 1m, null, null);
        CardCmd.ClearAffliction(Card);
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card != Card)
        {
            return Task.CompletedTask;
        }

        if (card.Pile?.Type is PileType.Hand or PileType.Play)
        {
            return Task.CompletedTask;
        }

        CardCmd.ClearAffliction(card);
        return Task.CompletedTask;
    }

    public override void BeforeRemoved()
    {
        if (AppliedRetainKeyword)
        {
            CardCmd.RemoveKeyword(Card, CardKeyword.Retain);
        }
    }
}
