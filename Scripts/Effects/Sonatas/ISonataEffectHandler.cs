using EchoCore.Scripts.Services;
using MegaCrit.Sts2.Core.Entities.Players;

namespace EchoCore.Scripts.Effects.Sonatas;

/// <summary>
/// 合鸣效果处理器接口。
/// 公共服务只负责统计件数并派发，具体断点收益由各合鸣单独实现。
/// </summary>
public interface ISonataEffectHandler
{
    string SonataId { get; }

    Task OnCombatStart(Player player, EchoCombatEffectService.ActiveSonataSummary summary);
}
