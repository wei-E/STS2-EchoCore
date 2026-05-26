using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Sonata;

namespace EchoCore.Scripts.Registry;

/// <summary>
/// 注册 Echo Core MVP 自带的原版怪物声骸、基础词条和占位合鸣。
/// </summary>
public static class VanillaEchoBootstrap
{
    public const string OwnerModId = "EchoCore";
    public const string DefaultIconPath = "res://echo-core/ui/echoes/icons/default_echo_icon.png";
    public const string UniversalSonataId = "echo_core:universal_resonance";
    public const string HiddenLightSonataId = "echo_core:hidden_light";
    public const string BasicAffixPoolId = "echo_core:basic";

    public static void RegisterAll()
    {
        RegisterAffixes();
        RegisterSonatas();
        RegisterEchoes();
    }

    private static void RegisterAffixes()
    {
        EchoRegistry.RegisterAffix(MakeTieredAffix(
            "echo_core:strength_start",
            "ECHO_CORE_AFFIX_STRENGTH_START.name",
            "ECHO_CORE_AFFIX_STRENGTH_START.description",
            1m,
            2m,
            3m));

        EchoRegistry.RegisterAffix(MakeTieredAffix(
            "echo_core:dexterity_start",
            "ECHO_CORE_AFFIX_DEXTERITY_START.name",
            "ECHO_CORE_AFFIX_DEXTERITY_START.description",
            1m,
            2m,
            3m));

        EchoRegistry.RegisterAffix(MakeTieredAffix(
            "echo_core:block_start",
            "ECHO_CORE_AFFIX_BLOCK_START.name",
            "ECHO_CORE_AFFIX_BLOCK_START.description",
            3m,
            6m,
            9m));
    }

    private static EchoAffixDefinition MakeTieredAffix(string id, string nameKey, string descriptionKey, decimal tier1, decimal tier2, decimal tier3)
    {
        // 低档高权重、高档低权重，先把鸣潮式刷词条的“惊喜但少见”落到数据结构上。
        return new EchoAffixDefinition(
            id,
            nameKey,
            descriptionKey,
            [
                new EchoAffixTierDefinition(1, tier1, EchoAffixTierRarity.Common, 70),
                new EchoAffixTierDefinition(2, tier2, EchoAffixTierRarity.Rare, 25),
                new EchoAffixTierDefinition(3, tier3, EchoAffixTierRarity.Epic, 5),
            ]);
    }

    private static void RegisterSonatas()
    {
        EchoRegistry.RegisterSonata(new SonataDefinition(
            UniversalSonataId,
            "ECHO_CORE_UNIVERSAL_RESONANCE.name",
            "ECHO_CORE_UNIVERSAL_RESONANCE.description",
            DefaultIconPath,
            [
                new SonataBreakpointDefinition(2, "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_2"),
                new SonataBreakpointDefinition(3, "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_3"),
                new SonataBreakpointDefinition(5, "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_5"),
            ]));

        EchoRegistry.RegisterSonata(new SonataDefinition(
            HiddenLightSonataId,
            "ECHO_CORE_HIDDEN_LIGHT.name",
            "ECHO_CORE_HIDDEN_LIGHT.description",
            DefaultIconPath,
            [
                new SonataBreakpointDefinition(2, "ECHO_CORE_HIDDEN_LIGHT.breakpoint_2"),
                new SonataBreakpointDefinition(3, "ECHO_CORE_HIDDEN_LIGHT.breakpoint_3"),
                new SonataBreakpointDefinition(5, "ECHO_CORE_HIDDEN_LIGHT.breakpoint_5"),
            ]));
    }

    private static void RegisterEchoes()
    {
        RegisterVanillaEcho(
            id: "echo_core:monster_leaf_slime_s",
            nameKey: "ECHO_CORE_ECHO_LEAF_SLIME_S.name",
            descriptionKey: "ECHO_CORE_ECHO_LEAF_SLIME_S.description",
            sourceMonsterId: "LEAF_SLIME_S",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardLeafSlimeS>(),
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "slime", "common"]);

        RegisterVanillaEcho(
            id: "echo_core:monster_shrinker_beetle",
            nameKey: "ECHO_CORE_ECHO_SHRINKER_BEETLE.name",
            descriptionKey: "ECHO_CORE_ECHO_SHRINKER_BEETLE.description",
            sourceMonsterId: "SHRINKER_BEETLE",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardShrinkerBeetle>(),
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "beetle", "common"]);

        RegisterVanillaEcho(
            id: "echo_core:monster_nibbit",
            nameKey: "ECHO_CORE_ECHO_NIBBIT.name",
            descriptionKey: "ECHO_CORE_ECHO_NIBBIT.description",
            sourceMonsterId: "NIBBIT",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardNibbit>(),
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "nibbit", "common"]);

        RegisterVanillaEcho(
            id: "echo_core:monster_byrdonis",
            nameKey: "ECHO_CORE_ECHO_BYRDONIS.name",
            descriptionKey: "ECHO_CORE_ECHO_BYRDONIS.description",
            sourceMonsterId: "BYRDONIS",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardByrdonis>(),
            echoClass: EchoClass.Elite,
            cost: 3,
            dropTags: ["act1", "elite", "byrdonis"],
            sonataIds: [UniversalSonataId, HiddenLightSonataId]);

        RegisterVanillaEcho(
            id: "echo_core:monster_ceremonial_beast",
            nameKey: "ECHO_CORE_ECHO_CEREMONIAL_BEAST.name",
            descriptionKey: "ECHO_CORE_ECHO_CEREMONIAL_BEAST.description",
            sourceMonsterId: "CEREMONIAL_BEAST",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardCeremonialBeast>(),
            echoClass: EchoClass.Overlord,
            cost: 4,
            dropTags: ["act1", "boss", "ceremonial_beast"]);
    }

    private static void RegisterVanillaEcho(
        string id,
        string nameKey,
        string descriptionKey,
        string sourceMonsterId,
        string skillCardId,
        EchoClass echoClass,
        int cost,
        IReadOnlyList<string> dropTags,
        IReadOnlyList<string>? sonataIds = null)
    {
        EchoRegistry.RegisterEcho(new EchoDefinition(
            id,
            nameKey,
            descriptionKey,
            DefaultIconPath,
            OwnerModId,
            echoClass,
            cost,
            EchoFormType.TacticalCard,
            sonataIds ?? [UniversalSonataId],
            skillCardId,
            GetDefaultSkillCooldownTurns(echoClass),
            dropTags,
            sourceMonsterId,
            [],
            BasicAffixPoolId,
            100));
    }

    private static int GetDefaultSkillCooldownTurns(EchoClass echoClass)
    {
        return echoClass switch
        {
            EchoClass.Common => 3,
            EchoClass.Elite => 4,
            EchoClass.Overlord => 5,
            EchoClass.Calamity => 5,
            _ => 4,
        };
    }
}
