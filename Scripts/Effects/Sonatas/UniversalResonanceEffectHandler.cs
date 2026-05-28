using EchoCore.Scripts.Content;
using EchoCore.Scripts.Services;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Effects.Sonatas;

/// <summary>
/// 基础残响合鸣效果实现。
/// </summary>
public sealed class UniversalResonanceEffectHandler : ISonataEffectHandler
{
    public string SonataId => EchoContentConstants.UniversalSonataId;

    public async Task OnCombatStart(Player player, EchoCombatEffectService.ActiveSonataSummary summary)
    {
        foreach (int breakpoint in summary.ActiveBreakpoints)
        {
            switch (breakpoint)
            {
                case 2:
                    await CreatureCmd.GainBlock(player.Creature, 4m, ValueProp.Unpowered, null);
                    break;

                case 3:
                    await PowerCmd.Apply<StrengthPower>(player.Creature, 1m, player.Creature, null);
                    break;

                case 5:
                    await PowerCmd.Apply<DexterityPower>(player.Creature, 1m, player.Creature, null);
                    break;
            }

            Log.Info($"[EchoCore] Applied sonata effect. sonata={summary.Definition.Id}, equipped={summary.EquippedCount}, breakpoint={breakpoint}");
        }
    }
}
