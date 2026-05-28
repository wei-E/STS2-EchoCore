using EchoCore.Scripts.BuffSkills;

namespace EchoCore.Scripts.Content.Skills;

/// <summary>
/// 墨宝的 Buff 型主动技定义。
/// 当前直接复用原版 SlipperyPower，不额外增加持续回合封装。
/// </summary>
public static class InkletSlipperyBuffSkillContent
{
    public static BuffSkillDefinition Create()
    {
        return new BuffSkillDefinition(
            EchoContentConstants.InkletSlipperyBuffSkillId,
            "ECHO_CORE_BUFF_SKILL_INKLET.name",
            "ECHO_CORE_BUFF_SKILL_INKLET.description",
            [
                new BuffSkillPowerDefinition("SLIPPERY", 1m, BuffSkillTargetType.Self),
            ]);
    }
}
