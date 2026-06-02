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

        if (EchoSkillCardRegistry.TryGetSkillSummaryLocKeys(definition.SkillCardId, out string titleKey, out string descriptionKey))
        {
            string skillName = GetLocStringOrEmpty("monsters", titleKey);
            string description = GetLocStringOrEmpty("monsters", descriptionKey);
            if (string.IsNullOrWhiteSpace(skillName))
            {
                skillName = "未命名主动技";
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = "该主动技描述暂未配置。";
            }

            return $"{skillName}\n{description}\n冷却回合：{definition.SkillCooldownTurns}";
        }

        return $"未命名主动技\n该主动技描述暂未配置。\n冷却回合：{definition.SkillCooldownTurns}";
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
        try
        {
            string localized = new LocString(table, key).GetFormattedText();
            return HasResolvedLocalization(table, key, localized) ? localized : string.Empty;
        }
        catch (Exception exception)
        {
            // 某些主动技文案沿用了卡牌 diff 占位符；在未提供动态变量时直接格式化会抛异常。
            // 这里降级为空串，让上层摘要回退到占位文案，而不是把整个 UI 挂载链打断。
            Log.Warn($"[EchoCore] Failed to resolve skill localization. table={table}, key={key}, error={exception.Message}");
            return string.Empty;
        }
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

}
