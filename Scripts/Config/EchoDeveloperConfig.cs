using BaseLib.Config;

namespace EchoCore.Scripts.Config;

[ConfigHoverTipsByDefault]
internal sealed class EchoDeveloperConfig : SimpleModConfig
{
    /// <summary>
    /// 开发开关只负责控制按钮与菜单显隐，不参与任何运行时战斗逻辑。
    /// </summary>
    public static bool EnableEchoDeveloperMenu { get; set; } = false;
}
