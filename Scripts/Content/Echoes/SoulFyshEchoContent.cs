using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 灵魂异鱼声骸定义。
/// 主动技直接施加灵体，并向抽牌堆塞入两张 Beckon。
/// </summary>
public static class SoulFyshEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_soul_fysh",
            nameKey: "ECHO_CORE_ECHO_SOUL_FYSH.name",
            descriptionKey: "ECHO_CORE_ECHO_SOUL_FYSH.description",
            sourceMonsterId: "SOUL_FYSH",
            skillCardId: null,
            buffSkillId: EchoContentConstants.SoulFyshBuffSkillId,
            echoClass: EchoClass.Overlord,
            cost: 4,
            dropTags: ["boss", "soul_fysh", "overlord"],
            formType: EchoFormType.Morph,
            sonataIds: [EchoContentConstants.HiddenLightSonataId],
            skillCooldownTurnsOverride: 4);
    }
}
