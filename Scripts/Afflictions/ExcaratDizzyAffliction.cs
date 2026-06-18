using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace EchoCore.Scripts.Afflictions;

/// <summary>
/// 遁地鼠的轻量骚扰牌面异常。
/// 它不会彻底锁牌，只是把下一次摸到或留在手里的牌临时抬费 1。
/// </summary>
[CustomID("ECHO_CORE_EXCARAT_DIZZY_AFFLICTION")]
public sealed class ExcaratDizzyAffliction : AfflictionModel, ILocalizationProvider
{
    public override bool HasExtraCardText => true;

    public override bool CanAfflictUnplayableCards => false;

    public string? LocTable => "afflictions";

    public List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardModifierLoc(
            "眩晕",
            "费用增加 1。离开手牌或抽牌堆后移除。",
            "眩晕"),
        _ => new CardModifierLoc(
            "Dizzy",
            "Costs 1 more. Removed after leaving your hand or draw pile.",
            "Dizzy")
    };

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (card == Card)
        {
            modifiedCost = originalCost + 1m;
            return true;
        }

        modifiedCost = originalCost;
        return false;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card != Card)
        {
            return Task.CompletedTask;
        }

        if (card.Pile?.Type is PileType.Hand or PileType.Draw)
        {
            return Task.CompletedTask;
        }

        CardCmd.ClearAffliction(card);
        return Task.CompletedTask;
    }
}
