using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content.Echoes;

/// <summary>
/// 墨宝声骸定义。
/// 当前借用 Morph 形态承载 Buff 型主动技，后续若增加专用枚举可再迁移。
/// </summary>
public static class InkletEchoContent
{
    public static EchoDefinition Create()
    {
        return EchoContentFactory.CreateVanillaEcho(
            id: "echo_core:monster_inklet",
            nameKey: "ECHO_CORE_ECHO_INKLET.name",
            descriptionKey: "ECHO_CORE_ECHO_INKLET.description",
            sourceMonsterId: "INKLET",
            skillCardId: null,
            buffSkillId: EchoContentConstants.InkletSlipperyBuffSkillId,
            echoClass: EchoClass.Common,
            cost: 1,
            dropTags: ["act1", "inklet", "common"],
            formType: EchoFormType.Morph);
    }
}
