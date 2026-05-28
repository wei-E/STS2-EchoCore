using EchoCore.Scripts.Echoes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

namespace EchoCore.Scripts.Effects.Skills;

/// <summary>
/// 主动技形态处理器。
/// 它按 FormType 接管“是否可用、如何显示、如何释放”的统一入口。
/// </summary>
public interface IActiveSkillHandler
{
    EchoFormType FormType { get; }

    bool HasUsableSkill(EchoDefinition definition);

    bool RequiresHandSpace(EchoDefinition definition);

    string GetSkillSummary(EchoDefinition definition);

    Task<bool> TryActivate(Player player, EchoDefinition definition, CombatState combatState);
}
