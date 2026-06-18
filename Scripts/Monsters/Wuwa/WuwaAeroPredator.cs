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

namespace EchoCore.Scripts.Monsters.Wuwa;

[CustomID("ECHO_CORE_MONSTER_AERO_PREDATOR")]
public sealed class WuwaAeroPredator : WuwaStaticMonsterBase
{
    private bool _openWithGust;

    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/aero_predator_battle.png";

    protected override Vector2 VisualScale => new(0.12f, 0.12f);

    protected override Vector2 VisualPosition => new(0f, -120f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 54, 50);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 58, 54);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

    private int ThrowDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

    private int HuntDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);

    private int HuntHits => 3;

    private int GustAmount => 1;

    /// <summary>
    /// 允许遭遇控制开场是先压制还是先投掷，避免同模板怪固定同一拍点。
    /// </summary>
    public bool OpenWithGust
    {
        get => _openWithGust;
        set
        {
            AssertMutable();
            _openWithGust = value;
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState throwMove = new("GALE_THROW", GaleThrowMove, new SingleAttackIntent(ThrowDamage));
        MoveState gustMove = new("CUTTING_GUST", CuttingGustMove, new DebuffIntent());
        MoveState huntMove = new("RETURNING_HUNT", ReturningHuntMove, new MultiAttackIntent(HuntDamage, HuntHits));

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(gustMove, () => OpenWithGust);
        opening.AddState(throwMove, () => true);

        ConditionalBranchState cycle = new("CYCLE_BRANCH");
        cycle.AddState(huntMove, TargetHasWeakOrFrail);
        cycle.AddState(gustMove, () => !TargetHasWeakOrFrail());
        cycle.AddState(throwMove, () => true);

        throwMove.FollowUpState = cycle;
        gustMove.FollowUpState = huntMove;
        huntMove.FollowUpState = cycle;

        states.AddRange([opening, throwMove, gustMove, huntMove, cycle]);
        return new MonsterMoveStateMachine(states, opening);
    }

    private async Task GaleThrowMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ThrowDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task CuttingGustMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<WeakPower>(targets, GustAmount, Creature, null);
    }

    private async Task ReturningHuntMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(HuntDamage).WithHitCount(HuntHits).FromMonster(this).OnlyPlayAnimOnce().WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private bool TargetHasWeakOrFrail()
    {
        return CombatState.Players.Any(player =>
            player.Creature.IsAlive &&
            (player.Creature.HasPower<WeakPower>() || player.Creature.HasPower<FrailPower>()));
    }
}
