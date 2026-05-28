using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Content;
using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Effects.Affixes;

/// <summary>
/// 开战格挡词条效果。
/// </summary>
public sealed class StartBlockAffixEffectHandler : IAffixEffectHandler
{
    public string AffixId => EchoContentConstants.BlockStartAffixId;

    public async Task OnCombatStart(Player player, EchoInstance instance, EchoAffixInstance affix)
    {
        await CreatureCmd.GainBlock(player.Creature, affix.Value, ValueProp.Unpowered, null);
        Log.Info($"[EchoCore] Applied echo affix effect. echo={instance.DefinitionId}, affix={affix.AffixId}, tier={affix.Tier}, value={affix.Value}, effect=Block");
    }
}
