using EchoCore.Scripts.Affixes;

namespace EchoCore.Scripts.Content.Affixes;

/// <summary>
/// 开战格挡词条定义。
/// </summary>
public static class StartBlockAffixContent
{
    public static EchoAffixDefinition Create()
    {
        return EchoContentFactory.CreateTieredAffix(
            EchoContentConstants.BlockStartAffixId,
            "ECHO_CORE_AFFIX_BLOCK_START.name",
            "ECHO_CORE_AFFIX_BLOCK_START.description",
            3m,
            6m,
            9m);
    }
}
