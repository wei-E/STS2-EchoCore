using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 先锋幼岩声骸定义。
/// </summary>
public static class VanguardJunrockEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_vanguard_junrock",
            nameKey: "ECHO_CORE_ECHO_VANGUARD_JUNROCK.name",
            descriptionKey: "ECHO_CORE_ECHO_VANGUARD_JUNROCK.description",
            sourceMonsterId: "ECHO_CORE_MONSTER_VANGUARD_JUNROCK",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardVanguardJunrock>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "wuwa", "junrock", "common"],
            iconPath: "res://echo-core/ui/echoes/icons/wuwa/vanguard_junrock.webp");
    }
}
