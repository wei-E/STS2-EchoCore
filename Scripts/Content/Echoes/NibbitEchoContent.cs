using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 小啃兽声骸定义。
/// </summary>
public static class NibbitEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_nibbit",
            nameKey: "ECHO_CORE_ECHO_NIBBIT.name",
            descriptionKey: "ECHO_CORE_ECHO_NIBBIT.description",
            sourceMonsterId: "NIBBIT",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardNibbit>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "nibbit", "common"]);
    }
}
