using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 树叶史莱姆（小）声骸定义。
/// </summary>
public static class LeafSlimeSEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_leaf_slime_s",
            nameKey: "ECHO_CORE_ECHO_LEAF_SLIME_S.name",
            descriptionKey: "ECHO_CORE_ECHO_LEAF_SLIME_S.description",
            sourceMonsterId: "LEAF_SLIME_S",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardLeafSlimeS>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "slime", "common"]);
    }
}
