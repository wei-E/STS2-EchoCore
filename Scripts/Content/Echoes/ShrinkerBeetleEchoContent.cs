using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 缩小甲虫声骸定义。
/// </summary>
public static class ShrinkerBeetleEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_shrinker_beetle",
            nameKey: "ECHO_CORE_ECHO_SHRINKER_BEETLE.name",
            descriptionKey: "ECHO_CORE_ECHO_SHRINKER_BEETLE.description",
            sourceMonsterId: "SHRINKER_BEETLE",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardShrinkerBeetle>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "beetle", "common"]);
    }
}
