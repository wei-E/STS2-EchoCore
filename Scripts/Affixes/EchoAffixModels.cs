namespace EchoCore.Scripts.Affixes;

/// <summary>
/// 词条档位稀有度。权重仍由具体档位定义决定，稀有度主要用于 UI 展示和调试。
/// </summary>
public enum EchoAffixTierRarity
{
    Common,
    Rare,
    Epic,
}

/// <summary>
/// 单个词条档位定义，例如“战斗开始获得力量 2 点，权重 25”。
/// </summary>
public sealed record EchoAffixTierDefinition(
    int Tier,
    decimal Value,
    EchoAffixTierRarity Rarity,
    int Weight
);

/// <summary>
/// 词条静态定义。一个词条可以有多个档位，随机时在所有可用档位中按权重抽取。
/// </summary>
public sealed record EchoAffixDefinition(
    string Id,
    string NameKey,
    string DescriptionKey,
    IReadOnlyList<EchoAffixTierDefinition> Tiers
);

/// <summary>
/// 玩家实际获得的词条实例，保存最终抽中的档位和值。
/// </summary>
public sealed record EchoAffixInstance(
    string AffixId,
    decimal Value,
    int Tier,
    EchoAffixTierRarity TierRarity,
    int Weight,
    bool IsTuned
);
