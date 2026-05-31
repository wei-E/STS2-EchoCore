using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Flail Knight 声骸定义。
/// </summary>
public static class FlailKnightEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_flail_knight",
            nameKey: "ECHO_CORE_ECHO_FLAIL_KNIGHT.name",
            descriptionKey: "ECHO_CORE_ECHO_FLAIL_KNIGHT.description",
            sourceMonsterId: "FLAIL_KNIGHT",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardFlailKnight>(),
            buffSkillId: null,
            echoClass: EchoClass.Elite,
            cost: 2,
            dropTags: ["act2", "knight", "elite"]);
    }
}
