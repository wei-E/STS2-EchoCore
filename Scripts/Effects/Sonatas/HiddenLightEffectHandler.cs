using EchoCore.Scripts.Content;
using EchoCore.Scripts.Services;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Effects.Sonatas;

/// <summary>
/// 隐世回光合鸣效果实现。
/// </summary>
public sealed class HiddenLightEffectHandler : ISonataEffectHandler
{
    public string SonataId => EchoContentConstants.HiddenLightSonataId;

    public async Task OnCombatStart(Player player, EchoCombatEffectService.ActiveSonataSummary summary)
    {
        foreach (int breakpoint in summary.ActiveBreakpoints)
        {
            switch (breakpoint)
            {
                case 2:
                    await CreatureCmd.Heal(player.Creature, 1m);
                    break;

                case 3:
                    await CreatureCmd.GainBlock(player.Creature, 3m, ValueProp.Unpowered, null);
                    break;

                case 5:
                    await PowerCmd.Apply<DexterityPower>(player.Creature, 1m, player.Creature, null);
                    break;
            }

            Log.Info($"[EchoCore] Applied sonata effect. sonata={summary.Definition.Id}, equipped={summary.EquippedCount}, breakpoint={breakpoint}");
        }
    }
}
