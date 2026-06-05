using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content;

/// <summary>
/// EchoCore 原生内容的公共构造辅助。
/// 它只负责把重复模板收拢，避免每个内容文件重复拼同样的定义参数。
/// </summary>
public static class EchoContentFactory
{
    public static EchoAffixDefinition CreateTieredAffix(
        string id,
        string nameKey,
        string descriptionKey,
        decimal tier1,
        decimal tier2,
        decimal tier3)
    {
        return new EchoAffixDefinition(
            id,
            nameKey,
            descriptionKey,
            [
                new EchoAffixTierDefinition(1, tier1, EchoAffixTierRarity.Common, 70),
                new EchoAffixTierDefinition(2, tier2, EchoAffixTierRarity.Rare, 25),
                new EchoAffixTierDefinition(3, tier3, EchoAffixTierRarity.Epic, 5),
            ]);
    }

    public static EchoDefinition CreateVanillaEcho(
        string id,
        string nameKey,
        string descriptionKey,
        string sourceMonsterId,
        string? skillCardId,
        string? buffSkillId,
        EchoClass echoClass,
        int cost,
        IReadOnlyList<string> dropTags,
        string? iconPath = null,
        EchoFormType formType = EchoFormType.TacticalCard,
        IReadOnlyList<string>? sonataIds = null,
        int? skillCooldownTurnsOverride = null)
    {
        return new EchoDefinition(
            id,
            nameKey,
            descriptionKey,
            iconPath ?? EchoContentConstants.DefaultIconPath,
            EchoContentConstants.OwnerModId,
            echoClass,
            cost,
            formType,
            sonataIds ?? [EchoContentConstants.EndlessEchoSonataId],
            skillCardId,
            buffSkillId,
            skillCooldownTurnsOverride ?? EchoContentConstants.GetDefaultSkillCooldownTurns(echoClass),
            dropTags,
            sourceMonsterId,
            [],
            EchoContentConstants.BasicAffixPoolId,
            100);
    }
}
