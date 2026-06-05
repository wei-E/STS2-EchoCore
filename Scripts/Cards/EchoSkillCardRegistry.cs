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
        [GetCardEntry<EchoCoreCardVanguardJunrock>()] = typeof(EchoCoreCardVanguardJunrock),
        [GetCardEntry<EchoCoreCardElectroPredator>()] = typeof(EchoCoreCardElectroPredator),
        [GetCardEntry<EchoCoreCardSabyrBoar>()] = typeof(EchoCoreCardSabyrBoar),
        [GetCardEntry<EchoCoreCardGlacioPrism>()] = typeof(EchoCoreCardGlacioPrism),
        [GetCardEntry<EchoCoreCardLeafSlimeS>()] = typeof(EchoCoreCardLeafSlimeS),
        [GetCardEntry<EchoCoreCardShrinkerBeetle>()] = typeof(EchoCoreCardShrinkerBeetle),
        [GetCardEntry<EchoCoreCardNibbit>()] = typeof(EchoCoreCardNibbit),
        [GetCardEntry<EchoCoreCardChomper>()] = typeof(EchoCoreCardChomper),
        [GetCardEntry<EchoCoreCardByrdonis>()] = typeof(EchoCoreCardByrdonis),
        [GetCardEntry<EchoCoreCardCeremonialBeast>()] = typeof(EchoCoreCardCeremonialBeast),
        [GetCardEntry<EchoCoreCardAxebot>()] = typeof(EchoCoreCardAxebot),
        [GetCardEntry<EchoCoreCardBowlbugSilk>()] = typeof(EchoCoreCardBowlbugSilk),
        [GetCardEntry<EchoCoreCardCalcifiedCultist>()] = typeof(EchoCoreCardCalcifiedCultist),
        [GetCardEntry<EchoCoreCardFlailKnight>()] = typeof(EchoCoreCardFlailKnight),
        [GetCardEntry<EchoCoreCardFlyconid>()] = typeof(EchoCoreCardFlyconid),
        [GetCardEntry<EchoCoreCardFossilStalker>()] = typeof(EchoCoreCardFossilStalker),
        [GetCardEntry<EchoCoreCardFrogKnight>()] = typeof(EchoCoreCardFrogKnight),
        [GetCardEntry<EchoCoreCardGlobeHead>()] = typeof(EchoCoreCardGlobeHead),
    };

    private static readonly Dictionary<string, (string TitleKey, string DescriptionKey)> SkillSummaryLocKeysByEntry = new(StringComparer.Ordinal)
    {
        [GetCardEntry<EchoCoreCardVanguardJunrock>()] = ("ECHO_CORE_SKILL_VANGUARD_JUNROCK.title", "ECHO_CORE_SKILL_VANGUARD_JUNROCK.description"),
        [GetCardEntry<EchoCoreCardElectroPredator>()] = ("ECHO_CORE_SKILL_ELECTRO_PREDATOR.title", "ECHO_CORE_SKILL_ELECTRO_PREDATOR.description"),
        [GetCardEntry<EchoCoreCardSabyrBoar>()] = ("ECHO_CORE_SKILL_SABYR_BOAR.title", "ECHO_CORE_SKILL_SABYR_BOAR.description"),
        [GetCardEntry<EchoCoreCardGlacioPrism>()] = ("ECHO_CORE_SKILL_GLACIO_PRISM.title", "ECHO_CORE_SKILL_GLACIO_PRISM.description"),
        [GetCardEntry<EchoCoreCardLeafSlimeS>()] = ("ECHO_CORE_SKILL_LEAF_SLIME_S.title", "ECHO_CORE_SKILL_LEAF_SLIME_S.description"),
        [GetCardEntry<EchoCoreCardShrinkerBeetle>()] = ("ECHO_CORE_SKILL_SHRINKER_BEETLE.title", "ECHO_CORE_SKILL_SHRINKER_BEETLE.description"),
        [GetCardEntry<EchoCoreCardNibbit>()] = ("ECHO_CORE_SKILL_NIBBIT.title", "ECHO_CORE_SKILL_NIBBIT.description"),
        [GetCardEntry<EchoCoreCardChomper>()] = ("ECHO_CORE_SKILL_CHOMPER.title", "ECHO_CORE_SKILL_CHOMPER.description"),
        [GetCardEntry<EchoCoreCardByrdonis>()] = ("ECHO_CORE_SKILL_BYRDONIS.title", "ECHO_CORE_SKILL_BYRDONIS.description"),
        [GetCardEntry<EchoCoreCardCeremonialBeast>()] = ("ECHO_CORE_SKILL_CEREMONIAL_BEAST.title", "ECHO_CORE_SKILL_CEREMONIAL_BEAST.description"),
        [GetCardEntry<EchoCoreCardAxebot>()] = ("ECHO_CORE_SKILL_AXEBOT.title", "ECHO_CORE_SKILL_AXEBOT.description"),
        [GetCardEntry<EchoCoreCardBowlbugSilk>()] = ("ECHO_CORE_SKILL_BOWLBUG_SILK.title", "ECHO_CORE_SKILL_BOWLBUG_SILK.description"),
        [GetCardEntry<EchoCoreCardCalcifiedCultist>()] = ("ECHO_CORE_SKILL_CALCIFIED_CULTIST.title", "ECHO_CORE_SKILL_CALCIFIED_CULTIST.description"),
        [GetCardEntry<EchoCoreCardFlailKnight>()] = ("ECHO_CORE_SKILL_FLAIL_KNIGHT.title", "ECHO_CORE_SKILL_FLAIL_KNIGHT.description"),
        [GetCardEntry<EchoCoreCardFlyconid>()] = ("ECHO_CORE_SKILL_FLYCONID.title", "ECHO_CORE_SKILL_FLYCONID.description"),
        [GetCardEntry<EchoCoreCardFossilStalker>()] = ("ECHO_CORE_SKILL_FOSSIL_STALKER.title", "ECHO_CORE_SKILL_FOSSIL_STALKER.description"),
        [GetCardEntry<EchoCoreCardFrogKnight>()] = ("ECHO_CORE_SKILL_FROG_KNIGHT.title", "ECHO_CORE_SKILL_FROG_KNIGHT.description"),
        [GetCardEntry<EchoCoreCardGlobeHead>()] = ("ECHO_CORE_SKILL_GLOBE_HEAD.title", "ECHO_CORE_SKILL_GLOBE_HEAD.description"),
    };

    private static readonly Dictionary<string, (string TitleKey, string DescriptionKey)> BuffSkillSummaryLocKeysById = new(StringComparer.Ordinal)
    {
        [Content.EchoContentConstants.InkletSlipperyBuffSkillId] = ("ECHO_CORE_SKILL_INKLET.title", "ECHO_CORE_SKILL_INKLET.description"),
        [Content.EchoContentConstants.SoulFyshBuffSkillId] = ("ECHO_CORE_SKILL_SOUL_FYSH.title", "ECHO_CORE_SKILL_SOUL_FYSH.description"),
        [Content.EchoContentConstants.TunnelerBuffSkillId] = ("ECHO_CORE_SKILL_TUNNELER.title", "ECHO_CORE_SKILL_TUNNELER.description"),
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

    public static bool TryGetSkillSummaryLocKeys(string? skillCardId, out string titleKey, out string descriptionKey)
    {
        titleKey = string.Empty;
        descriptionKey = string.Empty;
        if (string.IsNullOrWhiteSpace(skillCardId))
        {
            return false;
        }

        if (!SkillSummaryLocKeysByEntry.TryGetValue(skillCardId, out var value))
        {
            return false;
        }

        titleKey = value.TitleKey;
        descriptionKey = value.DescriptionKey;
        return true;
    }

    public static bool TryGetBuffSkillSummaryLocKeys(string? buffSkillId, out string titleKey, out string descriptionKey)
    {
        titleKey = string.Empty;
        descriptionKey = string.Empty;
        if (string.IsNullOrWhiteSpace(buffSkillId))
        {
            return false;
        }

        if (!BuffSkillSummaryLocKeysById.TryGetValue(buffSkillId, out var value))
        {
            return false;
        }

        titleKey = value.TitleKey;
        descriptionKey = value.DescriptionKey;
        return true;
    }
}
