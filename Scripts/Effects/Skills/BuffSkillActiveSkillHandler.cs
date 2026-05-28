using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;

namespace EchoCore.Scripts.Effects.Skills;

/// <summary>
/// Buff 型主动技处理器。
/// 当前复用 BuffSkillDefinition，并把具体 Power 施加交给 EchoBuffSkillService。
/// </summary>
public sealed class BuffSkillActiveSkillHandler : IActiveSkillHandler
{
    public EchoFormType FormType => EchoFormType.Morph;

    public bool HasUsableSkill(EchoDefinition definition)
    {
        return !string.IsNullOrWhiteSpace(definition.BuffSkillId);
    }

    public bool RequiresHandSpace(EchoDefinition definition)
    {
        return false;
    }

    public string GetSkillSummary(EchoDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.BuffSkillId))
        {
            return "当前版本未实现该形态的战斗主动技。";
        }

        if (EchoRegistry.TryGetBuffSkill(definition.BuffSkillId, out var buffSkill))
        {
            string skillName = GetLocStringWithFallback("monsters", buffSkill.NameKey, "未命名主动技");
            string description = GetLocStringWithFallback("monsters", buffSkill.DescriptionKey, "该主动技描述暂未配置。");
            return $"{skillName}\n{description}\n冷却回合：{definition.SkillCooldownTurns}";
        }

        return "当前版本未实现该形态的战斗主动技。";
    }

    public Task<bool> TryActivate(Player player, EchoDefinition definition, CombatState combatState)
    {
        return EchoBuffSkillService.TryActivate(player, definition);
    }

    private static string GetLocStringWithFallback(string table, string key, string fallback)
    {
        string localized = new LocString(table, key).GetFormattedText();
        return HasResolvedLocalization(table, key, localized) ? localized : fallback;
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
