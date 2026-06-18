using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using System;

namespace EchoCore.Scripts.Monsters.Wuwa;

[CustomID("ECHO_CORE_MONSTER_FISSION_JUNROCK")]
public sealed class WuwaFissionJunrock : WuwaStaticMonsterBase
{
    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/fission_junrock_battle.png";

    protected override Vector2 VisualScale => new(0.58f, 0.58f);

    protected override Vector2 VisualPosition => new(0f, -112f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 64, 60);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 68, 64);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

    private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

    private int RushDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);

    private int RushHits => 2;

    private int UnstableBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 8, 7);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState slam = new("JAGGED_SLAM", JaggedSlamMove, new SingleAttackIntent(SlamDamage));
        MoveState hum = new("UNSTABLE_HUM", UnstableHumMove, new DefendIntent(), new BuffIntent());
        MoveState rush = new("CRACKED_RUSH", CrackedRushMove, new MultiAttackIntent(RushDamage, RushHits));

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(slam, () => true);

        RandomBranchState cycle = new("CYCLE_BRANCH");
        cycle.AddBranch(hum, MoveRepeatType.CannotRepeat);
        cycle.AddBranch(rush, MoveRepeatType.CannotRepeat);
        cycle.AddBranch(slam, MoveRepeatType.CannotRepeat);

        slam.FollowUpState = cycle;
        hum.FollowUpState = rush;
        rush.FollowUpState = cycle;

        states.AddRange([opening, slam, hum, rush, cycle]);
        return new MonsterMoveStateMachine(states, opening);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<FissionSplitPower>(Creature, 1m, Creature, null);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        await base.AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength);
        if (creature != Creature || wasRemovalPrevented || !creature.HasPower<FissionSplitPower>() || CombatState?.Encounter == null)
        {
            return;
        }

        string originalSlot = creature.SlotName;
        string nextSlot = CombatState.Encounter.GetNextSlot(CombatState);

        WuwaFissionJunrockShard leftShard = (WuwaFissionJunrockShard)ModelDb.Monster<WuwaFissionJunrockShard>().ToMutable();
        leftShard.OverrideHp = Math.Max(1, (int)Math.Ceiling(creature.MaxHp * 0.5m));

        WuwaFissionJunrockShard rightShard = (WuwaFissionJunrockShard)ModelDb.Monster<WuwaFissionJunrockShard>().ToMutable();
        rightShard.OverrideHp = Math.Max(1, (int)Math.Ceiling(creature.MaxHp * 0.5m));

        await CreatureCmd.Add(leftShard, CombatState, CombatSide.Enemy, originalSlot);

        if (!string.IsNullOrEmpty(nextSlot))
        {
            await CreatureCmd.Add(rightShard, CombatState, CombatSide.Enemy, nextSlot);
        }
    }

    private async Task JaggedSlamMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(SlamDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task UnstableHumMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, UnstableBlock, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(Creature, 1m, Creature, null);
    }

    private async Task CrackedRushMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(RushDamage).WithHitCount(RushHits).FromMonster(this).OnlyPlayAnimOnce().WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
