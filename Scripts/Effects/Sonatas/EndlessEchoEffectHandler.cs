using EchoCore.Scripts.Content;
using EchoCore.Scripts.Powers;
using EchoCore.Scripts.Services;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Effects.Sonatas;

/// <summary>
/// 不绝余音合鸣效果实现。
/// </summary>
public sealed class EndlessEchoEffectHandler : ISonataEffectHandler
{
    public string SonataId => EchoContentConstants.EndlessEchoSonataId;

    public async Task OnCombatStart(Player player, EchoCombatEffectService.ActiveSonataSummary summary)
    {
        foreach (int breakpoint in summary.ActiveBreakpoints)
        {
            switch (breakpoint)
            {
                case 2:
                    await PowerCmd.Apply<StrengthPower>(player.Creature, 1m, player.Creature, null);
                    break;

                case 5:
                    await PowerCmd.Apply<EndlessEchoPower>(player.Creature, 1m, player.Creature, null);
                    break;
            }

            Log.Info($"[EchoCore] Applied sonata effect. sonata={summary.Definition.Id}, equipped={summary.EquippedCount}, breakpoint={breakpoint}");
        }
    }
}
