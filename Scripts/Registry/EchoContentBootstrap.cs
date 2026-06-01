using EchoCore.Scripts.Content.Affixes;
using EchoCore.Scripts.Content.Echoes;
using EchoCore.Scripts.Content.Skills;
using EchoCore.Scripts.Content.Sonatas;
using EchoCore.Scripts.Effects.Affixes;
using EchoCore.Scripts.Effects.Echoes;
using EchoCore.Scripts.Effects.Skills;
using EchoCore.Scripts.Effects.Sonatas;

namespace EchoCore.Scripts.Registry;

/// <summary>
/// EchoCore 自带内容的聚合注册入口。
/// 这里不再内联具体定义细节，只负责把 Content / Effects 层组装进注册表。
/// </summary>
public static class EchoContentBootstrap
{
    public static void RegisterAll()
    {
        RegisterAffixContent();
        RegisterSkillContent();
        RegisterSonataContent();
        RegisterEchoContent();
        RegisterEchoEffectHandlers();
        RegisterEffectHandlers();
    }

    private static void RegisterAffixContent()
    {
        EchoRegistry.RegisterAffix(StartStrengthAffixContent.Create());
        EchoRegistry.RegisterAffix(StartDexterityAffixContent.Create());
        EchoRegistry.RegisterAffix(StartBlockAffixContent.Create());
    }

    private static void RegisterSkillContent()
    {
        EchoRegistry.RegisterBuffSkill(InkletSlipperyBuffSkillContent.Create());
    }

    private static void RegisterSonataContent()
    {
        EchoRegistry.RegisterSonata(HiddenLightSonataContent.Create());
        EchoRegistry.RegisterSonata(EndlessEchoSonataContent.Create());
    }

    private static void RegisterEchoContent()
    {
        EchoRegistry.RegisterEcho(LeafSlimeSEchoContent.Create());
        EchoRegistry.RegisterEcho(ShrinkerBeetleEchoContent.Create());
        EchoRegistry.RegisterEcho(NibbitEchoContent.Create());
        EchoRegistry.RegisterEcho(InkletEchoContent.Create());
        EchoRegistry.RegisterEcho(ChomperEchoContent.Create());
        EchoRegistry.RegisterEcho(ByrdonisEchoContent.Create());
        EchoRegistry.RegisterEcho(CeremonialBeastEchoContent.Create());
        EchoRegistry.RegisterEcho(AxebotEchoContent.Create());
        EchoRegistry.RegisterEcho(BowlbugSilkEchoContent.Create());
        EchoRegistry.RegisterEcho(CalcifiedCultistEchoContent.Create());
        EchoRegistry.RegisterEcho(FlailKnightEchoContent.Create());
        EchoRegistry.RegisterEcho(FlyconidEchoContent.Create());
        EchoRegistry.RegisterEcho(FossilStalkerEchoContent.Create());
        EchoRegistry.RegisterEcho(FrogKnightEchoContent.Create());
        EchoRegistry.RegisterEcho(GlobeHeadEchoContent.Create());
    }

    private static void RegisterEffectHandlers()
    {
        EchoRegistry.RegisterActiveSkillHandler(new TacticalCardActiveSkillHandler());
        EchoRegistry.RegisterActiveSkillHandler(new BuffSkillActiveSkillHandler());

        EchoRegistry.RegisterAffixEffectHandler(new StartStrengthAffixEffectHandler());
        EchoRegistry.RegisterAffixEffectHandler(new StartDexterityAffixEffectHandler());
        EchoRegistry.RegisterAffixEffectHandler(new StartBlockAffixEffectHandler());

        EchoRegistry.RegisterSonataEffectHandler(new HiddenLightEffectHandler());
        EchoRegistry.RegisterSonataEffectHandler(new EndlessEchoEffectHandler());
    }

    private static void RegisterEchoEffectHandlers()
    {
        EchoRegistry.RegisterEchoEffectHandler(new ChomperEchoEffectHandler());
    }
}
