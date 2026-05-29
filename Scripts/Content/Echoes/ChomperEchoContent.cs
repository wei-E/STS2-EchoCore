using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Chomper 声骸定义。
/// 这只声骸会额外挂一个独立规则：开战获得 1 层人工制品。
/// </summary>
public static class ChomperEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_chomper",
            nameKey: "ECHO_CORE_ECHO_CHOMPER.name",
            descriptionKey: "ECHO_CORE_ECHO_CHOMPER.description",
            sourceMonsterId: "CHOMPER",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardChomper>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "chomper", "common"]);
    }
}
