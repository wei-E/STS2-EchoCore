using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Monsters.Wuwa;

[CustomID("ECHO_CORE_MONSTER_ELECTRO_PREDATOR")]
public sealed class WuwaElectroPredator : WuwaStaticMonsterBase
{
    private bool _hasUsedOpeningMark;

    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/electro_predator_battle.png";

    protected override Vector2 VisualScale => new(0.58f, 0.58f);

    protected override Vector2 VisualPosition => new(0f, -118f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 48, 44);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 52, 48);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

    private int ShotDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

    private int PierceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 12);

    private int RetreatBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 8, 7);

    private int MarkVulnerable => 2;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState shot = new("THUNDER_SHOT", ThunderShotMove, new SingleAttackIntent(ShotDamage));
        MoveState mark = new("HUNTER_MARK", HunterMarkMove, new DebuffIntent());
        MoveState pierce = new("SPRING_THRUST", SpringThrustMove, new SingleAttackIntent(PierceDamage));
        MoveState retreat = new("RETREAT_AND_AIM", RetreatAndAimMove, new DefendIntent());

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(mark, () => !_hasUsedOpeningMark);
        opening.AddState(shot, () => true);

        ConditionalBranchState attackDecision = new("ATTACK_DECISION");
        attackDecision.AddState(pierce, TargetHasVulnerable);
        attackDecision.AddState(retreat, () => Creature.CurrentHp <= Creature.MaxHp / 2);
        attackDecision.AddState(mark, () => !TargetHasVulnerable());
        attackDecision.AddState(shot, () => true);

        shot.FollowUpState = attackDecision;
        mark.FollowUpState = attackDecision;
        pierce.FollowUpState = attackDecision;
        retreat.FollowUpState = attackDecision;

        states.AddRange([opening, shot, mark, pierce, retreat, attackDecision]);
        return new MonsterMoveStateMachine(states, opening);
    }

    private async Task ThunderShotMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ShotDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task HunterMarkMove(IReadOnlyList<Creature> targets)
    {
        _hasUsedOpeningMark = true;
        await PowerCmd.Apply<VulnerablePower>(targets, MarkVulnerable, Creature, null);
    }

    private async Task SpringThrustMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(PierceDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task RetreatAndAimMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, RetreatBlock, ValueProp.Move, null);
    }

    private bool TargetHasVulnerable()
    {
        return CombatState.Players.Any(player => player.Creature.IsAlive && player.Creature.HasPower<VulnerablePower>());
    }
}
