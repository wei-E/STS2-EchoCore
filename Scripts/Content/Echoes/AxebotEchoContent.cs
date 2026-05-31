using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Axebot 声骸定义。
/// </summary>
public static class AxebotEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_axebot",
            nameKey: "ECHO_CORE_ECHO_AXEBOT.name",
            descriptionKey: "ECHO_CORE_ECHO_AXEBOT.description",
            sourceMonsterId: "AXEBOT",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardAxebot>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act2", "robot", "common"]);
    }
}
