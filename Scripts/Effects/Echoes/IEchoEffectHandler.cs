using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Entities.Players;

namespace EchoCore.Scripts.Effects.Echoes;

/// <summary>
/// 特殊声骸效果处理器。
/// 只有当某只声骸存在“独立于词条 / 合鸣之外的额外战斗规则”时，才需要实现这个接口。
/// </summary>
public interface IEchoEffectHandler
{
    string EchoId { get; }

    Task OnCombatStart(Player player, EchoInstance instance, EchoDefinition definition);
}
