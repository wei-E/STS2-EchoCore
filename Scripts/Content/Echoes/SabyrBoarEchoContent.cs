using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 碎獠猪声骸定义。
/// </summary>
public static class SabyrBoarEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_sabyr_boar",
            nameKey: "ECHO_CORE_ECHO_SABYR_BOAR.name",
            descriptionKey: "ECHO_CORE_ECHO_SABYR_BOAR.description",
            sourceMonsterId: "ECHO_CORE_MONSTER_SABYR_BOAR",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardSabyrBoar>(),
            buffSkillId: null,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "wuwa", "boar", "common"],
            iconPath: "res://echo-core/ui/echoes/icons/wuwa/sabyr_boar.webp");
    }
}
