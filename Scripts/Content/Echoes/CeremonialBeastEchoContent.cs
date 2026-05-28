using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 仪式兽声骸定义。
/// </summary>
public static class CeremonialBeastEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_ceremonial_beast",
            nameKey: "ECHO_CORE_ECHO_CEREMONIAL_BEAST.name",
            descriptionKey: "ECHO_CORE_ECHO_CEREMONIAL_BEAST.description",
            sourceMonsterId: "CEREMONIAL_BEAST",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardCeremonialBeast>(),
            buffSkillId: null,
            echoClass: EchoClass.Overlord,
            cost: 4,
            dropTags: ["act1", "boss", "ceremonial_beast"]);
    }
}
