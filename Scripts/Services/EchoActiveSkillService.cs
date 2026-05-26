using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 声骸主动技 MVP：槽位 1 的主声骸在战斗中提供一个按钮，点击后生成绑定的主动技卡。
/// 主动技卡由声骸定义固定绑定，不参与词条随机和调谐。
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

        int remainingCooldown = GetRemainingCooldown(player);
        if (remainingCooldown > 0)
        {
            return new ActiveSkillStatus(false, $"冷却 {remainingCooldown}", remainingCooldown, definition);
        }

        if (!CanActInCurrentTurn(player))
        {
            return new ActiveSkillStatus(false, "非可行动回合", remainingCooldown, definition);
        }

        if (PileType.Hand.GetPile(player).Cards.Count >= MaxHandSize)
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
            || definition == null
            || string.IsNullOrWhiteSpace(definition.SkillCardId))
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

            if (!EchoSkillCardRegistry.TryGetCanonicalCard(definition.SkillCardId, out CardModel? canonicalCard)
                || canonicalCard == null)
            {
                Log.Error($"[EchoCore] Echo skill card model not found. echo={definition.Id}, skillCardId={definition.SkillCardId}");
                return false;
            }

            var card = combatState.CreateCard(canonicalCard, player);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);

            CooldownByPlayerNetId[player.NetId] = Math.Max(1, definition.SkillCooldownTurns);
            Log.Info($"[EchoCore] Activated echo skill. player={player.NetId}, echo={definition.Id}, card={definition.SkillCardId}, cooldown={definition.SkillCooldownTurns}");
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

        if (definition.FormType != EchoFormType.TacticalCard || string.IsNullOrWhiteSpace(definition.SkillCardId))
        {
            unavailableReason = "主声骸没有卡牌主动技";
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

    public sealed record ActiveSkillStatus(
        bool CanUse,
        string Reason,
        int RemainingCooldown,
        EchoDefinition? Definition
    );
}
