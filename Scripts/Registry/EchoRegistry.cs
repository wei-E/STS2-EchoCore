using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.BuffSkills;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Effects.Affixes;
using EchoCore.Scripts.Effects.Echoes;
using EchoCore.Scripts.Effects.Skills;
using EchoCore.Scripts.Effects.Sonatas;
using EchoCore.Scripts.Sonata;

namespace EchoCore.Scripts.Registry;

/// <summary>
/// Echo Core 的全局注册表。MVP 先用内存字典承载定义，后续再叠加 JSON 与第三方软依赖入口。
/// </summary>
public static class EchoRegistry
{
    private static readonly Dictionary<string, EchoDefinition> EchoesById = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, EchoDefinition> EchoesByMonsterId = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, EchoAffixDefinition> AffixesById = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, BuffSkillDefinition> BuffSkillsById = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, SonataDefinition> SonatasById = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, IEchoEffectHandler> EchoEffectHandlersById = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, IAffixEffectHandler> AffixEffectHandlersById = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, ISonataEffectHandler> SonataEffectHandlersById = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<EchoFormType, IActiveSkillHandler> ActiveSkillHandlersByFormType = new();

    public static IReadOnlyCollection<EchoDefinition> Echoes => EchoesById.Values;

    public static IReadOnlyCollection<EchoAffixDefinition> Affixes => AffixesById.Values;

    public static IReadOnlyCollection<BuffSkillDefinition> BuffSkills => BuffSkillsById.Values;

    public static IReadOnlyCollection<SonataDefinition> Sonatas => SonatasById.Values;

    public static void Clear()
    {
        EchoesById.Clear();
        EchoesByMonsterId.Clear();
        AffixesById.Clear();
        BuffSkillsById.Clear();
        SonatasById.Clear();
        EchoEffectHandlersById.Clear();
        AffixEffectHandlersById.Clear();
        SonataEffectHandlersById.Clear();
        ActiveSkillHandlersByFormType.Clear();
    }

    public static void RegisterEcho(EchoDefinition definition)
    {
        // 声骸 ID 是跨 Mod 边界的稳定键，重复注册通常代表内容包冲突，直接失败更容易定位。
        if (!EchoesById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException($"Echo already registered: {definition.Id}");
        }

        if (!string.IsNullOrWhiteSpace(definition.SourceMonsterId))
        {
            EchoesByMonsterId[definition.SourceMonsterId] = definition;
        }
    }

    /// <summary>
    /// 为已注册声骸追加一个可归属的候选合鸣。
    /// 该方法用于外部 Mod 扩展现有声骸的合鸣池；真正掉落实例时，仍然只会从候选池中抽中一个最终合鸣。
    /// </summary>
    public static bool TryAddSonataToEcho(string echoId, string sonataId)
    {
        if (!EchoesById.TryGetValue(echoId, out var definition))
        {
            return false;
        }

        if (!SonatasById.ContainsKey(sonataId))
        {
            return false;
        }

        if (definition.SonataIds.Any(id => string.Equals(id, sonataId, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var updatedDefinition = definition with
        {
            SonataIds = definition.SonataIds.Concat([sonataId]).ToArray(),
        };

        EchoesById[echoId] = updatedDefinition;
        if (!string.IsNullOrWhiteSpace(updatedDefinition.SourceMonsterId))
        {
            EchoesByMonsterId[updatedDefinition.SourceMonsterId] = updatedDefinition;
        }

        return true;
    }

    public static void RegisterAffix(EchoAffixDefinition definition)
    {
        if (!AffixesById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException($"Echo affix already registered: {definition.Id}");
        }
    }

    public static void RegisterBuffSkill(BuffSkillDefinition definition)
    {
        if (!BuffSkillsById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException($"Echo buff skill already registered: {definition.Id}");
        }
    }

    public static void RegisterSonata(SonataDefinition definition)
    {
        if (!SonatasById.TryAdd(definition.Id, definition))
        {
            throw new InvalidOperationException($"Echo sonata already registered: {definition.Id}");
        }
    }

    public static void RegisterEchoEffectHandler(IEchoEffectHandler handler)
    {
        if (!EchoEffectHandlersById.TryAdd(handler.EchoId, handler))
        {
            throw new InvalidOperationException($"Echo effect handler already registered: {handler.EchoId}");
        }
    }

    public static void RegisterAffixEffectHandler(IAffixEffectHandler handler)
    {
        if (!AffixEffectHandlersById.TryAdd(handler.AffixId, handler))
        {
            throw new InvalidOperationException($"Echo affix effect handler already registered: {handler.AffixId}");
        }
    }

    public static void RegisterSonataEffectHandler(ISonataEffectHandler handler)
    {
        if (!SonataEffectHandlersById.TryAdd(handler.SonataId, handler))
        {
            throw new InvalidOperationException($"Echo sonata effect handler already registered: {handler.SonataId}");
        }
    }

    public static void RegisterActiveSkillHandler(IActiveSkillHandler handler)
    {
        if (!ActiveSkillHandlersByFormType.TryAdd(handler.FormType, handler))
        {
            throw new InvalidOperationException($"Echo active skill handler already registered: {handler.FormType}");
        }
    }

    public static bool TryGetEcho(string id, out EchoDefinition definition)
    {
        return EchoesById.TryGetValue(id, out definition!);
    }

    public static bool TryGetEchoByMonsterId(string monsterId, out EchoDefinition definition)
    {
        return EchoesByMonsterId.TryGetValue(monsterId, out definition!);
    }

    public static bool TryGetAffix(string id, out EchoAffixDefinition definition)
    {
        return AffixesById.TryGetValue(id, out definition!);
    }

    public static bool TryGetBuffSkill(string id, out BuffSkillDefinition definition)
    {
        return BuffSkillsById.TryGetValue(id, out definition!);
    }

    public static bool TryGetSonata(string id, out SonataDefinition definition)
    {
        return SonatasById.TryGetValue(id, out definition!);
    }

    public static bool TryGetEchoEffectHandler(string id, out IEchoEffectHandler handler)
    {
        return EchoEffectHandlersById.TryGetValue(id, out handler!);
    }

    public static bool TryGetAffixEffectHandler(string id, out IAffixEffectHandler handler)
    {
        return AffixEffectHandlersById.TryGetValue(id, out handler!);
    }

    public static bool TryGetSonataEffectHandler(string id, out ISonataEffectHandler handler)
    {
        return SonataEffectHandlersById.TryGetValue(id, out handler!);
    }

    public static bool TryGetActiveSkillHandler(EchoFormType formType, out IActiveSkillHandler handler)
    {
        return ActiveSkillHandlersByFormType.TryGetValue(formType, out handler!);
    }
}
