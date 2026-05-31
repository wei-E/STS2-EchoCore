using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Bowlbug Silk 声骸定义。
/// </summary>
public static class BowlbugSilkEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_bowlbug_silk",
            nameKey: "ECHO_CORE_ECHO_BOWLBUG_SILK.name",
            descriptionKey: "ECHO_CORE_ECHO_BOWLBUG_SILK.description",
            sourceMonsterId: "BOWLBUG_SILK",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardBowlbugSilk>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "bowlbug", "common"]);
    }
}
