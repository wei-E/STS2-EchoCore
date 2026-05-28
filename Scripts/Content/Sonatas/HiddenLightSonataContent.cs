using EchoCore.Scripts.Sonata;

namespace EchoCore.Scripts.Content.Sonatas;

/// <summary>
/// 隐世回光合鸣定义。
/// </summary>
public static class HiddenLightSonataContent
{
    public static SonataDefinition Create()
    {
        return new SonataDefinition(
            EchoContentConstants.HiddenLightSonataId,
            "ECHO_CORE_HIDDEN_LIGHT.name",
            "ECHO_CORE_HIDDEN_LIGHT.description",
            EchoContentConstants.DefaultIconPath,
            [
                new SonataBreakpointDefinition(2, "ECHO_CORE_HIDDEN_LIGHT.breakpoint_2"),
                new SonataBreakpointDefinition(3, "ECHO_CORE_HIDDEN_LIGHT.breakpoint_3"),
                new SonataBreakpointDefinition(5, "ECHO_CORE_HIDDEN_LIGHT.breakpoint_5"),
            ]);
    }
}
