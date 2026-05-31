using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Fossil Stalker 声骸定义。
/// </summary>
public static class FossilStalkerEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_fossil_stalker",
            nameKey: "ECHO_CORE_ECHO_FOSSIL_STALKER.name",
            descriptionKey: "ECHO_CORE_ECHO_FOSSIL_STALKER.description",
            sourceMonsterId: "FOSSIL_STALKER",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardFossilStalker>(),
            buffSkillId: null,
            echoClass: EchoClass.Elite,
            cost: 2,
            dropTags: ["act2", "fossil", "elite"]);
    }
}
