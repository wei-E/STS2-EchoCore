using EchoCore.Scripts.Content.Affixes;
using EchoCore.Scripts.Content.Echoes;
using EchoCore.Scripts.Content.Skills;
using EchoCore.Scripts.Content.Sonatas;
using EchoCore.Scripts.Effects.Affixes;
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
        EchoRegistry.RegisterSonata(UniversalResonanceSonataContent.Create());
        EchoRegistry.RegisterSonata(HiddenLightSonataContent.Create());
    }

    private static void RegisterEchoContent()
    {
        EchoRegistry.RegisterEcho(LeafSlimeSEchoContent.Create());
        EchoRegistry.RegisterEcho(ShrinkerBeetleEchoContent.Create());
        EchoRegistry.RegisterEcho(NibbitEchoContent.Create());
        EchoRegistry.RegisterEcho(InkletEchoContent.Create());
        EchoRegistry.RegisterEcho(ByrdonisEchoContent.Create());
        EchoRegistry.RegisterEcho(CeremonialBeastEchoContent.Create());
    }

    private static void RegisterEffectHandlers()
    {
        EchoRegistry.RegisterActiveSkillHandler(new TacticalCardActiveSkillHandler());
        EchoRegistry.RegisterActiveSkillHandler(new BuffSkillActiveSkillHandler());

        EchoRegistry.RegisterAffixEffectHandler(new StartStrengthAffixEffectHandler());
        EchoRegistry.RegisterAffixEffectHandler(new StartDexterityAffixEffectHandler());
        EchoRegistry.RegisterAffixEffectHandler(new StartBlockAffixEffectHandler());

        EchoRegistry.RegisterSonataEffectHandler(new UniversalResonanceEffectHandler());
        EchoRegistry.RegisterSonataEffectHandler(new HiddenLightEffectHandler());
    }

    private static void RegisterEchoEffectHandlers()
    {
        // Phase B 先把特殊声骸 handler 的注册入口稳定下来。
        // 当前首批声骸仍主要依赖词条、合鸣和主动技，本轮暂未新增额外的独立开战规则。
    }
}
