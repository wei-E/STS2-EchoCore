namespace EchoCore.Scripts.Echoes;

/// <summary>
/// 声骸的来源级别，MVP 中直接映射到 COST 与掉落来源。
/// </summary>
public enum EchoClass
{
    Common,
    Elite,
    Overlord,
    Calamity,
}

/// <summary>
/// 声骸激活形态。当前只实现 TacticalCard，其它类型先作为后续扩展占位。
/// </summary>
public enum EchoFormType
{
    TacticalCard,
    Morph,
    Minion,
}

/// <summary>
/// 声骸实例的获得来源，用于保存、调试和后续掉落规则回溯。
/// </summary>
public enum EchoSourceType
{
    CombatDrop,
    Monster,
    Elite,
    Boss,
    Event,
    Shop,
    Debug,
}
