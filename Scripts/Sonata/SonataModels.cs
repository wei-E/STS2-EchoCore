namespace EchoCore.Scripts.Sonata;

/// <summary>
/// 合鸣套装阈值定义。MVP 先只登记元数据，实际效果在战斗生效阶段再接入。
/// </summary>
public sealed record SonataBreakpointDefinition(
    int RequiredCount,
    string DescriptionKey
);

/// <summary>
/// 合鸣套装静态定义，供声骸装备盘计算 2/3/5 件效果使用。
/// </summary>
public sealed record SonataDefinition(
    string Id,
    string NameKey,
    string DescriptionKey,
    string IconPath,
    IReadOnlyList<SonataBreakpointDefinition> Breakpoints
);
