using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Content;
using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Effects.Affixes;

/// <summary>
/// 开战敏捷词条效果。
/// </summary>
public sealed class StartDexterityAffixEffectHandler : IAffixEffectHandler
{
    public string AffixId => EchoContentConstants.DexterityStartAffixId;

    public async Task OnCombatStart(Player player, EchoInstance instance, EchoAffixInstance affix)
    {
        await PowerCmd.Apply<DexterityPower>(player.Creature, affix.Value, player.Creature, null);
        Log.Info($"[EchoCore] Applied echo affix effect. echo={instance.DefinitionId}, affix={affix.AffixId}, tier={affix.Tier}, value={affix.Value}, effect=Dexterity");
    }
}
