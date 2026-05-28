namespace EchoCore.Scripts.BuffSkills;

/// <summary>
/// Buff 型主动技定义。它描述“点击主声骸按钮后，要给谁施加什么 Power”，
/// 生命周期仍尽量交给具体 Power 自己管理。
/// </summary>
public sealed record BuffSkillDefinition(
    string Id,
    string NameKey,
    string DescriptionKey,
    IReadOnlyList<BuffSkillPowerDefinition> AppliedPowers
);

/// <summary>
/// 单条 Power 施加规则。MVP 先只支持对自己施加，后续再扩展敌人或全队目标。
/// </summary>
public sealed record BuffSkillPowerDefinition(
    string PowerTypeId,
    decimal Amount,
    BuffSkillTargetType TargetType
);

public enum BuffSkillTargetType
{
    Self,
}
