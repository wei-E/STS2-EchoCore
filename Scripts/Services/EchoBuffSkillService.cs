using EchoCore.Scripts.BuffSkills;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.Powers;

namespace EchoCore.Scripts.Services;

/// <summary>
/// Buff 型主动技执行器。
/// 当前先把“点击主动技后直接施加 Power”做成独立服务，避免把 EchoActiveSkillService 挤成多职责文件。
/// </summary>
public static class EchoBuffSkillService
{
    public static async Task<bool> TryActivate(Player player, EchoDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(definition.BuffSkillId))
        {
            return false;
        }

        if (!EchoRegistry.TryGetBuffSkill(definition.BuffSkillId, out BuffSkillDefinition buffSkill))
        {
            Log.Error($"[EchoCore] Buff skill definition not found. echo={definition.Id}, buffSkillId={definition.BuffSkillId}");
            return false;
        }

        foreach (var appliedPower in buffSkill.AppliedPowers)
        {
            bool success = await TryApplyPower(player, appliedPower);
            if (!success)
            {
                Log.Error($"[EchoCore] Failed to apply buff skill power. echo={definition.Id}, buffSkillId={buffSkill.Id}, powerTypeId={appliedPower.PowerTypeId}");
                return false;
            }
        }

        Log.Info($"[EchoCore] Activated echo buff skill. player={player.NetId}, echo={definition.Id}, buffSkillId={buffSkill.Id}");
        return true;
    }

    /// <summary>
    /// MVP 先用显式分支，保证可读性和可控性。
    /// 等 Buff 型主动技变多后，再考虑把 PowerTypeId -> Apply 委托表抽离。
    /// </summary>
    private static async Task<bool> TryApplyPower(Player player, BuffSkillPowerDefinition appliedPower)
    {
        switch (appliedPower.TargetType)
        {
            case BuffSkillTargetType.Self:
                return await TryApplySelfPower(player, appliedPower);

            default:
                return false;
        }
    }

    private static async Task<bool> TryApplySelfPower(Player player, BuffSkillPowerDefinition appliedPower)
    {
        switch (appliedPower.PowerTypeId)
        {
            case "SLIPPERY":
                await PowerCmd.Apply<SlipperyPower>(player.Creature, appliedPower.Amount, player.Creature, null);
                return true;

            default:
                return false;
        }
    }
}
