using EchoCore.Scripts.BuffSkills;

namespace EchoCore.Scripts.Content.Skills;

/// <summary>
/// 地道虫的 Buff 型主动技定义。
/// 它先给当前回合防守资源，再交给专属 Power 负责“禁攻 + 下回合随机伤害”。
/// </summary>
public static class TunnelerBuffSkillContent
{
    public static BuffSkillDefinition Create()
    {
        return new BuffSkillDefinition(
            EchoContentConstants.TunnelerBuffSkillId,
            "ECHO_CORE_BUFF_SKILL_TUNNELER.name",
            "ECHO_CORE_BUFF_SKILL_TUNNELER.description",
            [
                new BuffSkillPowerDefinition("GAIN_BLOCK", 12m, BuffSkillTargetType.Self),
                new BuffSkillPowerDefinition("TUNNELER_BURROW_POWER", 20m, BuffSkillTargetType.Self),
            ]);
    }
}
