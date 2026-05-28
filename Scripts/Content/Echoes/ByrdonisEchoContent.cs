using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 多尼斯异鸟声骸定义。
/// </summary>
public static class ByrdonisEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_byrdonis",
            nameKey: "ECHO_CORE_ECHO_BYRDONIS.name",
            descriptionKey: "ECHO_CORE_ECHO_BYRDONIS.description",
            sourceMonsterId: "BYRDONIS",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardByrdonis>(),
            buffSkillId: null,
            echoClass: EchoClass.Elite,
            cost: 3,
            dropTags: ["act1", "elite", "byrdonis"],
            sonataIds: [EchoContentConstants.UniversalSonataId, EchoContentConstants.HiddenLightSonataId]);
    }
}
