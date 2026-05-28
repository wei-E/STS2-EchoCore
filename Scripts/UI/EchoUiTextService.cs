using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;

namespace EchoCore.Scripts.UI;

/// <summary>
/// EchoCore UI 的文本拼装服务。
/// 它负责把声骸、词条、主动技、合鸣等运行时数据转成可直接显示的字符串，
/// 避免 Overlay 控件直接承担业务解释逻辑。
/// </summary>
public static class EchoUiTextService
{
    public static string GetEchoTitle(EchoDefinition definition)
    {
        return GetLocalizedTextOrFallback(definition.NameKey);
    }

    public static string GetEchoDisplayName(EchoInstance instance)
    {
        if (!EchoRegistry.TryGetEcho(instance.DefinitionId, out var definition))
        {
            return instance.DefinitionId;
        }

        string shortId = instance.InstanceId.Length > 8 ? instance.InstanceId[^8..] : instance.InstanceId;
        return $"{GetLocalizedTextOrFallback(definition.NameKey)}  Lv.{instance.Level}  #{shortId}";
    }

    public static string GetEchoDescription(EchoDefinition definition)
    {
        return GetLocStringWithFallback("monsters", definition.DescriptionKey, "该声骸暂未提供详细描述。");
    }

    public static string GetSkillSummary(EchoDefinition definition)
    {
        if (EchoRegistry.TryGetActiveSkillHandler(definition.FormType, out var handler)
            && handler.HasUsableSkill(definition))
        {
            return handler.GetSkillSummary(definition);
        }

        return "当前版本未实现该形态的战斗主动技。";
    }

    public static string GetAffixSummary(EchoInstance instance)
    {
        if (instance.Affixes.Count == 0)
        {
            return "词条：无";
        }

        var parts = instance.Affixes.Select(affix =>
        {
            string name = EchoRegistry.TryGetAffix(affix.AffixId, out var definition)
                ? GetLocalizedTextOrFallback(definition.NameKey)
                : affix.AffixId;
            return $"{name} +{affix.Value:0.#}（档位{affix.Tier}）";
        });

        return "词条：" + string.Join(" / ", parts);
    }

    public static string GetAffixDetailSummary(EchoInstance instance)
    {
        if (instance.Affixes.Count == 0)
        {
            return "当前没有词条。";
        }

        var lines = instance.Affixes.Select(affix =>
        {
            string name = EchoRegistry.TryGetAffix(affix.AffixId, out var definition)
                ? GetLocalizedTextOrFallback(definition.NameKey)
                : affix.AffixId;
            return $"{name} +{affix.Value:0.#}";
        });

        return string.Join("\n", lines);
    }

    public static string GetSonataSummary(EchoInstance instance)
    {
        if (string.IsNullOrWhiteSpace(instance.SelectedSonataId))
        {
            return "合鸣：无";
        }

        if (EchoRegistry.TryGetSonata(instance.SelectedSonataId, out var sonata))
        {
            return $"合鸣：{GetLocalizedTextOrFallback(sonata.NameKey)}";
        }

        return $"合鸣：{instance.SelectedSonataId}";
    }

    public static string GetSonataDetailSummary(Player player, EchoInstance instance)
    {
        string currentSonata = GetSonataSummary(instance);
        var summaries = EchoCombatEffectService.GetActiveSonataSummaries(player);
        if (summaries.Count == 0)
        {
            return $"{currentSonata}\n当前已装备声骸还没有激活合鸣效果。";
        }

        var lines = summaries.Select(summary =>
        {
            string sonataName = GetLocalizedTextOrFallback(summary.Definition.NameKey);
            string breakpointsText = string.Join(
                " / ",
                summary.ActiveBreakpoints.Select(value => GetSonataBreakpointDisplayText(summary, value)));
            return $"{sonataName}：{summary.EquippedCount} 件，{breakpointsText}";
        });

        return $"{currentSonata}\n{string.Join("\n", lines)}";
    }

    public static string GetSonataBreakpointDisplayText(EchoCombatEffectService.ActiveSonataSummary summary, int requiredCount)
    {
        var breakpoint = summary.Definition.Breakpoints.FirstOrDefault(item => item.RequiredCount == requiredCount);
        if (breakpoint == null)
        {
            return $"{requiredCount}件";
        }

        return GetLocalizedTextOrFallback(breakpoint.DescriptionKey);
    }

    public static string GetLocalizedTextOrFallback(string key)
    {
        string localized = new LocString("monsters", key).GetFormattedText();
        if (HasResolvedLocalization("monsters", key, localized))
        {
            return localized;
        }

        return key switch
        {
            "ECHO_CORE_UNIVERSAL_RESONANCE.name" => "基础残响",
            "ECHO_CORE_UNIVERSAL_RESONANCE.description" => "Echo Core MVP 使用的通用合鸣占位。",
            "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_2" => "2件：开战获得4点格挡。",
            "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_3" => "3件：额外获得1点力量。",
            "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_5" => "5件：额外获得1点敏捷。",
            "ECHO_CORE_HIDDEN_LIGHT.name" => "隐世回光",
            "ECHO_CORE_HIDDEN_LIGHT.description" => "适合恢复与稳态收益的合鸣占位。",
            "ECHO_CORE_HIDDEN_LIGHT.breakpoint_2" => "2件：开战回复1点生命。",
            "ECHO_CORE_HIDDEN_LIGHT.breakpoint_3" => "3件：额外获得3点格挡。",
            "ECHO_CORE_HIDDEN_LIGHT.breakpoint_5" => "5件：额外获得1点敏捷。",
            _ => key,
        };
    }

    public static string GetLocStringWithFallback(string table, string key, string fallback)
    {
        string localized = GetLocStringOrEmpty(table, key);
        return string.IsNullOrWhiteSpace(localized) ? fallback : localized;
    }

    public static string GetLocStringOrEmpty(string table, string key)
    {
        string localized = new LocString(table, key).GetFormattedText();
        return HasResolvedLocalization(table, key, localized) ? localized : string.Empty;
    }

    /// <summary>
    /// STS2/Mod 本地化在未命中时有时返回 `table.key`，有时直接返回裸 `key`，
    /// 这里统一判定两种形式都视为未解析，避免 UI 把 key 原样显示出来。
    /// </summary>
    public static bool HasResolvedLocalization(string table, string key, string localized)
    {
        if (string.IsNullOrWhiteSpace(localized))
        {
            return false;
        }

        return !string.Equals(localized, $"{table}.{key}", StringComparison.Ordinal)
            && !string.Equals(localized, key, StringComparison.Ordinal);
    }
}
