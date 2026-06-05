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

[CustomID("ECHO_CORE_MONSTER_VANGUARD_JUNROCK")]
public sealed class WuwaVanguardJunrock : WuwaStaticMonsterBase
{
    private bool _usedListenLastTurn;
    private bool _openWithListen;

    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/vanguard_junrock_battle.png";

    protected override Vector2 VisualScale => new(0.6f, 0.6f);

    protected override Vector2 VisualPosition => new(0f, -108f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 40);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 48, 44);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

    private int SlamDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

    private int ChargeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);

    private int ChargeHits => 2;

    private int ListenBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 7, 6);

    private int ListenStrength => 1;

    /// <summary>
    /// 同模板怪如果都走同一条开局分支，观感会很像“复制粘贴”。
    /// 这里允许遭遇在生成实例时直接指定开场偏好，让同类怪在第一回合就分流。
    /// </summary>
    public bool OpenWithListen
    {
        get => _openWithListen;
        set
        {
            AssertMutable();
            _openWithListen = value;
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState slam = new("ROCK_SHELL_SLAM", RockShellSlamMove, new SingleAttackIntent(SlamDamage));
        MoveState listen = new("SHATTER_LISTEN", ShatterListenMove, new DefendIntent(), new BuffIntent());
        MoveState charge = new("RAMPAGE_CHARGE", RampageChargeMove, new MultiAttackIntent(ChargeDamage, ChargeHits));

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(listen, () => OpenWithListen);
        opening.AddState(slam, () => true);

        ConditionalBranchState afterSlam = new("AFTER_SLAM_BRANCH");
        afterSlam.AddState(listen, () => !Creature.HasPower<StrengthPower>() && Rng.NextBool());
        afterSlam.AddState(charge, () => !Creature.HasPower<StrengthPower>());
        afterSlam.AddState(slam, () => true);

        ConditionalBranchState afterListen = new("AFTER_LISTEN_BRANCH");
        afterListen.AddState(charge, () => true);

        ConditionalBranchState afterCharge = new("AFTER_CHARGE_BRANCH");
        afterCharge.AddState(listen, () => !_usedListenLastTurn && !Creature.HasPower<StrengthPower>() && Rng.NextBool());
        afterCharge.AddState(slam, () => true);

        slam.FollowUpState = afterSlam;
        listen.FollowUpState = afterListen;
        charge.FollowUpState = afterCharge;

        states.AddRange([opening, slam, listen, charge, afterSlam, afterListen, afterCharge]);
        return new MonsterMoveStateMachine(states, opening);
    }

    private async Task RockShellSlamMove(IReadOnlyList<Creature> targets)
    {
        _usedListenLastTurn = false;
        await DamageCmd.Attack(SlamDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task ShatterListenMove(IReadOnlyList<Creature> targets)
    {
        _usedListenLastTurn = true;
        await CreatureCmd.GainBlock(Creature, ListenBlock, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(Creature, ListenStrength, Creature, null);
    }

    private async Task RampageChargeMove(IReadOnlyList<Creature> targets)
    {
        _usedListenLastTurn = false;
        await DamageCmd.Attack(ChargeDamage).WithHitCount(ChargeHits).FromMonster(this).OnlyPlayAnimOnce().WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
