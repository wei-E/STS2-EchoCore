namespace EchoCore.Scripts.Echoes;

/// <summary>
/// 声骸静态定义。定义只描述模板；玩家实际获得的数据放在 EchoInstance 中。
/// 这里登记的是“可归属的候选合鸣列表”，单个实例最终只会从中抽中一个。
/// </summary>
public sealed record EchoDefinition(
    string Id,
    string NameKey,
    string DescriptionKey,
    string IconPath,
    string OwnerModId,
    EchoClass Class,
    int Cost,
    EchoFormType FormType,
    IReadOnlyList<string> SonataIds,
    string? SkillCardId,
    int SkillCooldownTurns,
    IReadOnlyList<string> DropTags,
    string? SourceMonsterId,
    IReadOnlyList<string> AllowedCharacters,
    string? BaseAffixPoolId,
    int RarityWeight
);

/// <summary>
/// 玩家获得的声骸实例。MVP 阶段还不会大量创建实例，但先固定保存结构。
/// 注意：一个实例只会持有一个最终合鸣；即使定义层允许挂多个候选合鸣，掉落时也只随机选中其中一个。
/// </summary>
public sealed record EchoInstance(
    string InstanceId,
    string DefinitionId,
    string? SelectedSonataId,
    int Level,
    bool Locked,
    IReadOnlyList<Affixes.EchoAffixInstance> Affixes,
    int TuningCount,
    int AcquiredAtFloor,
    EchoSourceType SourceType,
    string? SourceId
);
