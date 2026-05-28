using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Effects.Skills;
using EchoCore.Scripts.Registry;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 声骸主动技服务。
/// 当前支持两类主动技：
/// 1. TacticalCard：生成绑定卡牌到手牌
/// 2. BuffSkill（当前借用 Morph 形态）：直接施加一个或多个 Buff
/// </summary>
public static class EchoActiveSkillService
{
    private const int MainEchoSlotIndex = 0;
    private const int MaxHandSize = 10;

    private static readonly Dictionary<ulong, int> CooldownByPlayerNetId = new();
    private static readonly HashSet<ulong> ActivatingPlayers = [];

    public static void ResetForCombat(CombatState combatState)
    {
        CooldownByPlayerNetId.Clear();
        ActivatingPlayers.Clear();

        foreach (var player in combatState.Players)
        {
            CooldownByPlayerNetId[player.NetId] = 0;
        }
    }

    public static void OnPlayerTurnStart(Player player)
    {
        if (!CooldownByPlayerNetId.TryGetValue(player.NetId, out int remaining))
        {
            CooldownByPlayerNetId[player.NetId] = 0;
            return;
        }

        if (remaining > 0)
        {
            CooldownByPlayerNetId[player.NetId] = remaining - 1;
        }
    }

    public static ActiveSkillStatus GetStatus(Player player)
    {
        if (!TryGetMainEchoSkill(player, out _, out var definition, out string? unavailableReason))
        {
            return new ActiveSkillStatus(false, unavailableReason ?? "未装备主声骸", 0, null);
        }

        if (definition == null)
        {
            return new ActiveSkillStatus(false, "主声骸定义缺失", 0, null);
        }

        int remainingCooldown = GetRemainingCooldown(player);
        if (remainingCooldown > 0)
        {
            return new ActiveSkillStatus(false, $"冷却 {remainingCooldown}", remainingCooldown, definition);
        }

        if (!CanActInCurrentTurn(player))
        {
            return new ActiveSkillStatus(false, "非可行动回合", remainingCooldown, definition);
        }

        if (RequiresHandSpace(definition) && PileType.Hand.GetPile(player).Cards.Count >= MaxHandSize)
        {
            return new ActiveSkillStatus(false, "手牌已满", remainingCooldown, definition);
        }

        if (ActivatingPlayers.Contains(player.NetId))
        {
            return new ActiveSkillStatus(false, "生成中", remainingCooldown, definition);
        }

        return new ActiveSkillStatus(true, "可释放", remainingCooldown, definition);
    }

    public static async Task<bool> TryActivate(Player player)
    {
        ActiveSkillStatus status = GetStatus(player);
        if (!status.CanUse || status.Definition == null)
        {
            return false;
        }

        if (!TryGetMainEchoSkill(player, out _, out var definition, out _)
            || definition == null)
        {
            return false;
        }

        ActivatingPlayers.Add(player.NetId);

        try
        {
            var combatState = player.Creature.CombatState;
            if (combatState == null)
            {
                return false;
            }

            bool activated = await TryActivateByForm(player, definition, combatState);
            if (!activated)
            {
                return false;
            }

            CooldownByPlayerNetId[player.NetId] = Math.Max(1, definition.SkillCooldownTurns);
            Log.Info($"[EchoCore] Activated echo skill. player={player.NetId}, echo={definition.Id}, formType={definition.FormType}, cooldown={definition.SkillCooldownTurns}");
            return true;
        }
        catch (Exception exception)
        {
            Log.Error($"[EchoCore] Failed to activate echo skill: {exception}");
            return false;
        }
        finally
        {
            ActivatingPlayers.Remove(player.NetId);
        }
    }

    private static bool TryGetMainEchoSkill(Player player, out EchoInstance? instance, out EchoDefinition? definition, out string? unavailableReason)
    {
        instance = null;
        definition = null;
        unavailableReason = null;

        var slots = EchoInventory.GetEquippedInstanceIds(player);
        if (slots.Count <= MainEchoSlotIndex || string.IsNullOrWhiteSpace(slots[MainEchoSlotIndex]))
        {
            unavailableReason = "槽位 1 未装备主声骸";
            return false;
        }

        instance = EchoInventory.FindByInstanceId(player, slots[MainEchoSlotIndex]);
        if (instance == null)
        {
            unavailableReason = "主声骸不存在";
            return false;
        }

        if (!EchoRegistry.TryGetEcho(instance.DefinitionId, out definition))
        {
            unavailableReason = "主声骸定义缺失";
            return false;
        }

        if (!HasUsableActiveSkill(definition))
        {
            unavailableReason = "主声骸没有可用主动技";
            return false;
        }

        return true;
    }

    private static int GetRemainingCooldown(Player player)
    {
        return CooldownByPlayerNetId.TryGetValue(player.NetId, out int remaining)
            ? Math.Max(0, remaining)
            : 0;
    }

    private static bool CanActInCurrentTurn(Player player)
    {
        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.PlayerActionsDisabled)
        {
            return false;
        }

        var combatState = player.Creature.CombatState;
        return combatState != null
            && combatState.CurrentSide == CombatSide.Player
            && CombatManager.Instance.IsPartOfPlayerTurn(player)
            && !CombatManager.Instance.IsPlayerReadyToEndTurn(player);
    }

    private static bool RequiresHandSpace(EchoDefinition definition)
    {
        return TryGetSkillHandler(definition, out var handler) && handler.RequiresHandSpace(definition);
    }

    private static bool HasUsableActiveSkill(EchoDefinition definition)
    {
        return TryGetSkillHandler(definition, out var handler) && handler.HasUsableSkill(definition);
    }

    private static async Task<bool> TryActivateByForm(Player player, EchoDefinition definition, CombatState combatState)
    {
        if (!TryGetSkillHandler(definition, out var handler))
        {
            Log.Error($"[EchoCore] Unsupported echo active skill form. echo={definition.Id}, formType={definition.FormType}");
            return false;
        }

        return await handler.TryActivate(player, definition, combatState);
    }

    private static bool TryGetSkillHandler(EchoDefinition definition, out IActiveSkillHandler handler)
    {
        return EchoRegistry.TryGetActiveSkillHandler(definition.FormType, out handler);
    }

    public sealed record ActiveSkillStatus(
        bool CanUse,
        string Reason,
        int RemainingCooldown,
        EchoDefinition? Definition
    );
}
