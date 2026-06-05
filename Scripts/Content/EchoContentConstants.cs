using EchoCore.Scripts.Echoes;

namespace EchoCore.Scripts.Content;

/// <summary>
/// EchoCore 原生内容使用的稳定常量。
/// 这些 ID 会被定义层、效果层、UI 和外部扩展共同引用，因此集中维护。
/// </summary>
public static class EchoContentConstants
{
    public const string OwnerModId = "EchoCore";
    public const string DefaultIconPath = "res://echo-core/ui/echoes/icons/default_echo_icon.png";

    public const string UniversalSonataId = "echo_core:universal_resonance";
    public const string HiddenLightSonataId = "echo_core:hidden_light";
    public const string EndlessEchoSonataId = "echo_core:endless_echo";

    public const string BasicAffixPoolId = "echo_core:basic";

    public const string StrengthStartAffixId = "echo_core:strength_start";
    public const string DexterityStartAffixId = "echo_core:dexterity_start";
    public const string BlockStartAffixId = "echo_core:block_start";

    public const string InkletSlipperyBuffSkillId = "echo_core:inklet_slippery";
    public const string SoulFyshBuffSkillId = "echo_core:soul_fysh_beckon";
    public const string TunnelerBuffSkillId = "echo_core:tunneler_burrow";

    public static int GetDefaultSkillCooldownTurns(EchoClass echoClass)
    {
        return echoClass switch
        {
            EchoClass.Common => 3,
            EchoClass.Elite => 4,
            EchoClass.Overlord => 5,
            EchoClass.Calamity => 5,
            _ => 4,
        };
    }
}
