using EchoCore.Scripts.Affixes;

namespace EchoCore.Scripts.Content.Affixes;

/// <summary>
/// 开战敏捷词条定义。
/// </summary>
public static class StartDexterityAffixContent
{
    public static EchoAffixDefinition Create()
    {
        return EchoContentFactory.CreateTieredAffix(
            EchoContentConstants.DexterityStartAffixId,
            "ECHO_CORE_AFFIX_DEXTERITY_START.name",
            "ECHO_CORE_AFFIX_DEXTERITY_START.description",
            1m,
            2m,
            3m);
    }
}
