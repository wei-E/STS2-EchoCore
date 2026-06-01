using EchoCore.Scripts.Sonata;

namespace EchoCore.Scripts.Content.Sonatas;

/// <summary>
/// 不绝余音合鸣定义。
/// </summary>
public static class EndlessEchoSonataContent
{
    public static SonataDefinition Create()
    {
        return new SonataDefinition(
            EchoContentConstants.EndlessEchoSonataId,
            "ECHO_CORE_ENDLESS_ECHO.name",
            "ECHO_CORE_ENDLESS_ECHO.description",
            EchoContentConstants.DefaultIconPath,
            [
                new SonataBreakpointDefinition(2, "ECHO_CORE_ENDLESS_ECHO.breakpoint_2"),
                new SonataBreakpointDefinition(3, "ECHO_CORE_ENDLESS_ECHO.breakpoint_3"),
                new SonataBreakpointDefinition(5, "ECHO_CORE_ENDLESS_ECHO.breakpoint_5"),
            ]);
    }
}
