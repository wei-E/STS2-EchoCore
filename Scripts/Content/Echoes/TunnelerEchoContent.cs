using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 地道虫声骸定义。
/// 主动技偏向“本回合蛰伏防守，下回合自动伏击”的怪物特色。
/// </summary>
public static class TunnelerEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_tunneler",
            nameKey: "ECHO_CORE_ECHO_TUNNELER.name",
            descriptionKey: "ECHO_CORE_ECHO_TUNNELER.description",
            sourceMonsterId: "TUNNELER",
            skillCardId: null,
            buffSkillId: EchoContentConstants.TunnelerBuffSkillId,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["common", "tunneler", "burrow"],
            formType: EchoFormType.Morph,
            sonataIds: [EchoContentConstants.EndlessEchoSonataId],
            skillCooldownTurnsOverride: 4);
    }
}
