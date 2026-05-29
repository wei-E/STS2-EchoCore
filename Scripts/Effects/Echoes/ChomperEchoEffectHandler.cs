using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Effects.Echoes;

/// <summary>
/// Chomper 的独立声骸规则。
/// 它不走随机词条，而是固定在战斗开始时提供 1 层人工制品。
/// </summary>
public sealed class ChomperEchoEffectHandler : IEchoEffectHandler
{
    public string EchoId => "echo_core:monster_chomper";

    public async Task OnCombatStart(Player player, EchoInstance instance, EchoDefinition definition)
    {
        await PowerCmd.Apply<ArtifactPower>(player.Creature, 1m, player.Creature, null);
        Log.Info($"[EchoCore] Applied Chomper echo effect. player={player.NetId}, instance={instance.InstanceId}, artifact=1");
    }
}
