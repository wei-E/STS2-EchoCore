using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 惊蛰猎手声骸定义。
/// </summary>
public static class ElectroPredatorEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_electro_predator",
            nameKey: "ECHO_CORE_ECHO_ELECTRO_PREDATOR.name",
            descriptionKey: "ECHO_CORE_ECHO_ELECTRO_PREDATOR.description",
            sourceMonsterId: "ECHO_CORE_MONSTER_ELECTRO_PREDATOR",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardElectroPredator>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "wuwa", "predator", "common"],
            iconPath: "res://echo-core/ui/echoes/icons/wuwa/electro_predator.webp");
    }
}
