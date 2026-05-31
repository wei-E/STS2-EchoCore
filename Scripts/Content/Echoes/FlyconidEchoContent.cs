using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Flyconid 声骸定义。
/// </summary>
public static class FlyconidEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_flyconid",
            nameKey: "ECHO_CORE_ECHO_FLYCONID.name",
            descriptionKey: "ECHO_CORE_ECHO_FLYCONID.description",
            sourceMonsterId: "FLYCONID",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardFlyconid>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "spore", "common"]);
    }
}
