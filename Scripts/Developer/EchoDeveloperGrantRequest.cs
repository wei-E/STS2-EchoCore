namespace EchoCore.Scripts.Developer;

/// <summary>
/// 开发者菜单提交给服务层的最小请求对象。
/// UI 只负责采集参数，不直接拼装声骸实例，避免后续功能扩展时把业务逻辑散落在控件里。
/// </summary>
public sealed record EchoDeveloperGrantRequest(
    string DefinitionId,
    string? SelectedSonataId,
    string AffixId,
    int AffixTier
);
