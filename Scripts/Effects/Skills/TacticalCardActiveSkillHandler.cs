using System.Text.RegularExpressions;
using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace EchoCore.Scripts.Effects.Skills;

/// <summary>
/// 卡牌型主动技处理器。
/// 当前实现是把绑定卡牌生成到手牌，尽量复用原版卡牌链路。
/// </summary>
public sealed class TacticalCardActiveSkillHandler : IActiveSkillHandler
{
    public EchoFormType FormType => EchoFormType.TacticalCard;

    public bool HasUsableSkill(EchoDefinition definition)
    {
        return !string.IsNullOrWhiteSpace(definition.SkillCardId);
    }

    public bool RequiresHandSpace(EchoDefinition definition)
    {
        return true;
    }

    public string GetSkillSummary(EchoDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.SkillCardId))
        {
            return "当前版本未实现该形态的战斗主动技。";
        }

        string key = definition.SkillCardId;
        string skillName = GetLocStringOrEmpty("cards", $"{key}.title");
        if (string.IsNullOrWhiteSpace(skillName))
        {
            skillName = GetLocStringOrEmpty("cards", $"ECHOCORE-{key}.title");
        }

        string rawDescription = GetLocStringOrEmpty("cards", $"{key}.description");
        if (string.IsNullOrWhiteSpace(rawDescription))
        {
            rawDescription = GetLocStringOrEmpty("cards", $"ECHOCORE-{key}.description");
        }

        if (string.IsNullOrWhiteSpace(rawDescription))
        {
            rawDescription = "该主动技描述暂未配置。";
        }

        if (string.IsNullOrWhiteSpace(skillName))
        {
            skillName = "未命名主动技";
        }

        return $"{skillName}\n{SanitizeCardDescription(rawDescription)}\n冷却回合：{definition.SkillCooldownTurns}";
    }

    public async Task<bool> TryActivate(Player player, EchoDefinition definition, CombatState combatState)
    {
        if (string.IsNullOrWhiteSpace(definition.SkillCardId))
        {
            return false;
        }

        if (!EchoSkillCardRegistry.TryGetCanonicalCard(definition.SkillCardId, out CardModel? canonicalCard)
            || canonicalCard == null)
        {
            Log.Error($"[EchoCore] Echo skill card model not found. echo={definition.Id}, skillCardId={definition.SkillCardId}");
            return false;
        }

        var card = combatState.CreateCard(canonicalCard, player);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
        return true;
    }

    private static string GetLocStringOrEmpty(string table, string key)
    {
        string localized = new LocString(table, key).GetFormattedText();
        return HasResolvedLocalization(table, key, localized) ? localized : string.Empty;
    }

    private static bool HasResolvedLocalization(string table, string key, string localized)
    {
        if (string.IsNullOrWhiteSpace(localized))
        {
            return false;
        }

        return !string.Equals(localized, $"{table}.{key}", StringComparison.Ordinal)
            && !string.Equals(localized, key, StringComparison.Ordinal);
    }

    private static string SanitizeCardDescription(string description)
    {
        return Regex.Replace(description, @"\{[^}]+\}", "X");
    }
}
