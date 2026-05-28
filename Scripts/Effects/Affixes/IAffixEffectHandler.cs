using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Entities.Players;

namespace EchoCore.Scripts.Effects.Affixes;

/// <summary>
/// 词条效果处理器接口。
/// 公共战斗服务只做分发，具体词条效果由各自实现承担。
/// </summary>
public interface IAffixEffectHandler
{
    string AffixId { get; }

    Task OnCombatStart(Player player, EchoInstance instance, EchoAffixInstance affix);
}
