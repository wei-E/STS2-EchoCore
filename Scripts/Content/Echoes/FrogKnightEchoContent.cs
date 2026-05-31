using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Frog Knight 声骸定义。
/// </summary>
public static class FrogKnightEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_frog_knight",
            nameKey: "ECHO_CORE_ECHO_FROG_KNIGHT.name",
            descriptionKey: "ECHO_CORE_ECHO_FROG_KNIGHT.description",
            sourceMonsterId: "FROG_KNIGHT",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardFrogKnight>(),
            buffSkillId: null,
            echoClass: EchoClass.Overlord,
            cost: 3,
            dropTags: ["act2", "knight", "boss"]);
    }
}
