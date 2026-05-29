using MegaCrit.Sts2.Core.Models;

namespace EchoCore.Scripts.Cards;

/// <summary>
/// 主动技卡目录。
/// 这里只维护“声骸主动技 entry <-> 卡牌类型”的稳定映射，避免业务层手写字符串。
/// </summary>
public static class EchoSkillCardRegistry
{
    private static readonly Dictionary<string, Type> CardTypesByEntry = new(StringComparer.Ordinal)
    {
        [GetCardEntry<EchoCoreCardLeafSlimeS>()] = typeof(EchoCoreCardLeafSlimeS),
        [GetCardEntry<EchoCoreCardShrinkerBeetle>()] = typeof(EchoCoreCardShrinkerBeetle),
        [GetCardEntry<EchoCoreCardNibbit>()] = typeof(EchoCoreCardNibbit),
        [GetCardEntry<EchoCoreCardChomper>()] = typeof(EchoCoreCardChomper),
        [GetCardEntry<EchoCoreCardByrdonis>()] = typeof(EchoCoreCardByrdonis),
        [GetCardEntry<EchoCoreCardCeremonialBeast>()] = typeof(EchoCoreCardCeremonialBeast),
    };

    public static string GetCardEntry<TCard>() where TCard : CardModel
    {
        return ModelDb.GetEntry(typeof(TCard));
    }

    public static bool TryGetCanonicalCard(string? skillCardId, out CardModel? canonicalCard)
    {
        canonicalCard = null;
        if (string.IsNullOrWhiteSpace(skillCardId))
        {
            return false;
        }

        if (CardTypesByEntry.TryGetValue(skillCardId, out Type? cardType))
        {
            canonicalCard = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            return true;
        }

        canonicalCard = ModelDb.GetByIdOrNull<CardModel>(new ModelId("CARD", skillCardId));
        return canonicalCard != null;
    }
}
