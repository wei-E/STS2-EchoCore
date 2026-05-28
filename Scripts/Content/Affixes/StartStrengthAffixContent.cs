using EchoCore.Scripts.Affixes;

namespace EchoCore.Scripts.Content.Affixes;

/// <summary>
/// 开战力量词条定义。
/// </summary>
public static class StartStrengthAffixContent
{
    public static EchoAffixDefinition Create()
    {
        return EchoContentFactory.CreateTieredAffix(
            EchoContentConstants.StrengthStartAffixId,
            "ECHO_CORE_AFFIX_STRENGTH_START.name",
            "ECHO_CORE_AFFIX_STRENGTH_START.description",
            1m,
            2m,
            3m);
    }
}
