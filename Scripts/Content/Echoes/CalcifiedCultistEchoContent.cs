using EchoCore.Scripts.Cards;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// Calcified Cultist 声骸定义。
/// </summary>
public static class CalcifiedCultistEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_calcified_cultist",
            nameKey: "ECHO_CORE_ECHO_CALCIFIED_CULTIST.name",
            descriptionKey: "ECHO_CORE_ECHO_CALCIFIED_CULTIST.description",
            sourceMonsterId: "CALCIFIED_CULTIST",
            skillCardId: EchoSkillCardRegistry.GetCardEntry<EchoCoreCardCalcifiedCultist>(),
            buffSkillId: null,
            echoClass: EchoClass.Elite,
            cost: 2,
            dropTags: ["act1", "cultist", "elite"]);
    }
}
