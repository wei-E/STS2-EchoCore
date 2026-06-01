using System.Reflection;
using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Developer;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.UI;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 开发者菜单的服务层。
/// 负责枚举可选内容、校验组合是否合法，并创建最终声骸实例写入库存。
/// </summary>
public static class EchoDeveloperService
{
    private static readonly FieldInfo? NRunStateField = typeof(NRun).GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);

    public static IReadOnlyList<EchoDefinition> GetAvailableEchoes()
    {
        return EchoRegistry.Echoes
            .OrderBy(definition => EchoUiTextService.GetEchoTitle(definition), StringComparer.CurrentCulture)
            .ToList();
    }

    public static IReadOnlyList<Sonata.SonataDefinition> GetAvailableSonatas(EchoDefinition definition)
    {
        return definition.SonataIds
            .Select(id => EchoRegistry.TryGetSonata(id, out var sonata) ? sonata : null)
            .OfType<Sonata.SonataDefinition>()
            .OrderBy(sonata => EchoUiTextService.GetLocalizedTextOrFallback(sonata.NameKey), StringComparer.CurrentCulture)
            .ToList();
    }

    public static IReadOnlyList<EchoAffixDefinition> GetAvailableAffixes()
    {
        return EchoRegistry.Affixes
            .OrderBy(definition => EchoUiTextService.GetLocalizedTextOrFallback(definition.NameKey), StringComparer.CurrentCulture)
            .ToList();
    }

    public static bool TryGrantToLocalPlayer(EchoDeveloperGrantRequest request, out string message)
    {
        message = string.Empty;

        var player = TryGetLocalPlayer();
        if (player == null)
        {
            message = "未找到当前玩家，无法添加声骸。";
            return false;
        }

        if (!EchoRegistry.TryGetEcho(request.DefinitionId, out var definition))
        {
            message = $"声骸定义不存在：{request.DefinitionId}";
            return false;
        }

        if (!TryResolveSelectedSonata(definition, request.SelectedSonataId, out var selectedSonataId, out message))
        {
            return false;
        }

        if (!TryCreateAffix(request.AffixId, request.AffixTier, out var affix, out message))
        {
            return false;
        }

        int floor = player.RunState?.ActFloor ?? 0;
        var instance = new EchoInstance(
            InstanceId: $"echo-dev-{Guid.NewGuid():N}",
            DefinitionId: definition.Id,
            SelectedSonataId: selectedSonataId,
            Level: 0,
            Locked: false,
            Affixes: [affix],
            TuningCount: 0,
            AcquiredAtFloor: floor,
            SourceType: EchoSourceType.Debug,
            SourceId: "echo_core:developer_menu");

        EchoInventory.Add(player, instance);
        message = $"已添加：{EchoUiTextService.GetEchoTitle(definition)}";
        return true;
    }

    public static Player? TryGetLocalPlayer()
    {
        RunState? state = TryGetCurrentRunState();
        if (state == null)
        {
            return null;
        }

        return LocalContext.GetMe(state.Players) ?? state.Players.FirstOrDefault();
    }

    public static bool IsCombatActive()
    {
        return NRun.Instance?.CombatRoom != null;
    }

    private static RunState? TryGetCurrentRunState()
    {
        if (NRun.Instance == null || NRunStateField == null)
        {
            return null;
        }

        return NRunStateField.GetValue(NRun.Instance) as RunState;
    }

    private static bool TryResolveSelectedSonata(EchoDefinition definition, string? selectedSonataId, out string? resolvedSonataId, out string message)
    {
        message = string.Empty;
        resolvedSonataId = null;

        if (string.IsNullOrWhiteSpace(selectedSonataId))
        {
            if (definition.SonataIds.Count == 0)
            {
                return true;
            }

            resolvedSonataId = definition.SonataIds[0];
            return true;
        }

        if (!definition.SonataIds.Any(id => string.Equals(id, selectedSonataId, StringComparison.OrdinalIgnoreCase)))
        {
            message = "所选合鸣不属于该声骸。";
            return false;
        }

        resolvedSonataId = definition.SonataIds.First(id => string.Equals(id, selectedSonataId, StringComparison.OrdinalIgnoreCase));
        return true;
    }

    private static bool TryCreateAffix(string affixId, int tierNumber, out EchoAffixInstance affix, out string message)
    {
        affix = null!;
        message = string.Empty;

        if (!EchoRegistry.TryGetAffix(affixId, out var definition))
        {
            message = $"词条定义不存在：{affixId}";
            return false;
        }

        var tier = definition.Tiers.FirstOrDefault(item => item.Tier == tierNumber);
        if (tier == null)
        {
            message = "所选词条档位不存在。";
            return false;
        }

        affix = new EchoAffixInstance(
            definition.Id,
            tier.Value,
            tier.Tier,
            tier.Rarity,
            tier.Weight,
            IsTuned: false);
        return true;
    }
}
