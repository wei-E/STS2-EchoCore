using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using EchoCore.Scripts.Sonata;
using EchoCore.Scripts.Effects.Sonatas;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;

namespace EchoCore.Scripts.Api;

/// <summary>
/// Stable public entry points for optional third-party EchoCore integrations.
/// Keep this API narrow and based on common STS2/BCL types so other mods can
/// call it through BaseLib ModInterop without referencing EchoCore directly.
/// </summary>
public static class EchoCoreApi
{
    public static bool RegisterSonata(
        string id,
        string nameKey,
        string descriptionKey,
        string iconPath,
        IReadOnlyList<(int RequiredCount, string DescriptionKey)> breakpoints)
    {
        try
        {
            if (EchoRegistry.TryGetSonata(id, out _))
            {
                Log.Info($"[EchoCore] Sonata already registered through API. sonata={id}");
                return false;
            }

            EchoRegistry.RegisterSonata(new SonataDefinition(
                id,
                nameKey,
                descriptionKey,
                iconPath,
                breakpoints
                    .Select(breakpoint => new SonataBreakpointDefinition(
                        breakpoint.RequiredCount,
                        breakpoint.DescriptionKey))
                    .ToList()));

            Log.Info($"[EchoCore] Registered sonata through API. sonata={id}, breakpoints={breakpoints.Count}");
            return true;
        }
        catch (Exception exception)
        {
            Log.Error($"[EchoCore] Failed to register sonata through API. sonata={id}, error={exception}");
            return false;
        }
    }

    public static bool RegisterSonataOnCombatStartHandler(
        string sonataId,
        Func<Player, int, IReadOnlyList<int>, Task> handler)
    {
        try
        {
            if (EchoRegistry.TryGetSonataEffectHandler(sonataId, out _))
            {
                Log.Info($"[EchoCore] Sonata effect handler already registered through API. sonata={sonataId}");
                return false;
            }

            EchoRegistry.RegisterSonataEffectHandler(new DelegateSonataEffectHandler(sonataId, handler));
            Log.Info($"[EchoCore] Registered sonata effect handler through API. sonata={sonataId}");
            return true;
        }
        catch (Exception exception)
        {
            Log.Error($"[EchoCore] Failed to register sonata effect handler through API. sonata={sonataId}, error={exception}");
            return false;
        }
    }

    public static bool TryAddSonataToEcho(string echoId, string sonataId)
    {
        try
        {
            return EchoRegistry.TryAddSonataToEcho(echoId, sonataId);
        }
        catch (Exception exception)
        {
            Log.Error($"[EchoCore] Failed to add sonata to echo through API. echo={echoId}, sonata={sonataId}, error={exception}");
            return false;
        }
    }

    public static int AddSonataToAllEchoes(string sonataId)
    {
        try
        {
            int count = 0;
            foreach (var echo in EchoRegistry.Echoes.ToList())
            {
                if (EchoRegistry.TryAddSonataToEcho(echo.Id, sonataId))
                {
                    count++;
                }
            }

            Log.Info($"[EchoCore] Added sonata to all registered echoes through API. sonata={sonataId}, echoes={count}");
            return count;
        }
        catch (Exception exception)
        {
            Log.Error($"[EchoCore] Failed to add sonata to all echoes through API. sonata={sonataId}, error={exception}");
            return 0;
        }
    }

    private sealed class DelegateSonataEffectHandler(
        string sonataId,
        Func<Player, int, IReadOnlyList<int>, Task> handler)
        : ISonataEffectHandler
    {
        public string SonataId { get; } = sonataId;

        public Task OnCombatStart(Player player, EchoCombatEffectService.ActiveSonataSummary summary)
        {
            return handler(player, summary.EquippedCount, summary.ActiveBreakpoints);
        }
    }
}
