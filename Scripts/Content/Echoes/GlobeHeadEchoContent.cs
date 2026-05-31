using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Globe Head 声骸定义。
/// </summary>
public static class GlobeHeadEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_globe_head",
            nameKey: "ECHO_CORE_ECHO_GLOBE_HEAD.name",
            descriptionKey: "ECHO_CORE_ECHO_GLOBE_HEAD.description",
            sourceMonsterId: "GLOBE_HEAD",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardGlobeHead>(),
            buffSkillId: null,
            echoClass: EchoClass.Overlord,
            cost: 3,
            dropTags: ["act2", "robot", "boss"]);
    }
}
