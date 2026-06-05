using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 冷凝棱镜声骸定义。
/// </summary>
public static class GlacioPrismEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_glacio_prism",
            nameKey: "ECHO_CORE_ECHO_GLACIO_PRISM.name",
            descriptionKey: "ECHO_CORE_ECHO_GLACIO_PRISM.description",
            sourceMonsterId: "ECHO_CORE_MONSTER_GLACIO_PRISM",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardGlacioPrism>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "wuwa", "prism", "common"],
            iconPath: "res://echo-core/ui/echoes/icons/wuwa/glacio_prism.webp");
    }
}
