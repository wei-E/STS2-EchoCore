using EchoCore.Scripts.Sonata;

namespace EchoCore.Scripts.Content.Sonatas;

/// <summary>
/// 基础残响合鸣定义。
/// </summary>
public static class UniversalResonanceSonataContent
{
    public static SonataDefinition Create()
    {
        return new SonataDefinition(
            EchoContentConstants.UniversalSonataId,
            "ECHO_CORE_UNIVERSAL_RESONANCE.name",
            "ECHO_CORE_UNIVERSAL_RESONANCE.description",
            EchoContentConstants.DefaultIconPath,
            [
                new SonataBreakpointDefinition(2, "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_2"),
                new SonataBreakpointDefinition(3, "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_3"),
                new SonataBreakpointDefinition(5, "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_5"),
            ]);
    }
}
