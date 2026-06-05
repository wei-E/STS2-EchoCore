using EchoCore.Scripts.BuffSkills;

namespace EchoCore.Scripts.Content.Skills;

/// <summary>
/// 灵魂异鱼的 Buff 型主动技定义。
/// 这里把“获得灵体”和“往抽牌堆塞入 Beckon”拆成两条规则，便于服务层按顺序执行。
/// </summary>
public static class SoulFyshBuffSkillContent
{
    public static BuffSkillDefinition Create()
    {
        return new BuffSkillDefinition(
            EchoContentConstants.SoulFyshBuffSkillId,
            "ECHO_CORE_BUFF_SKILL_SOUL_FYSH.name",
            "ECHO_CORE_BUFF_SKILL_SOUL_FYSH.description",
            [
                new BuffSkillPowerDefinition("INTANGIBLE", 1m, BuffSkillTargetType.Self),
                new BuffSkillPowerDefinition("ADD_BECKON_TO_DRAW", 1m, BuffSkillTargetType.Self),
            ]);
    }
}
