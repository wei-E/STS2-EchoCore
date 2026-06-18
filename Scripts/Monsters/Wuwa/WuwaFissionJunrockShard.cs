using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace EchoCore.Scripts.Monsters.Wuwa;

[CustomID("ECHO_CORE_MONSTER_FISSION_JUNROCK_SHARD")]
public sealed class WuwaFissionJunrockShard : WuwaStaticMonsterBase
{
    private int? _overrideHp;

    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/fission_junrock_battle.png";

    protected override Vector2 VisualScale => new(0.4f, 0.4f);

    protected override Vector2 VisualPosition => new(0f, -94f);

    public override int MinInitialHp => _overrideHp ?? AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 30);

    public override int MaxInitialHp => _overrideHp ?? AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 38, 34);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

    private int PeckDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);

    private int ScrapeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);

    private int ScrapeHits => 2;

    public int? OverrideHp
    {
        get => _overrideHp;
        set
        {
            AssertMutable();
            _overrideHp = value;
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState peck = new("SHARD_PECK", ShardPeckMove, new SingleAttackIntent(PeckDamage));
        MoveState scrape = new("SHARD_SCRAPE", ShardScrapeMove, new MultiAttackIntent(ScrapeDamage, ScrapeHits));

        RandomBranchState cycle = new("CYCLE_BRANCH");
        cycle.AddBranch(peck, MoveRepeatType.CannotRepeat);
        cycle.AddBranch(scrape, MoveRepeatType.CannotRepeat);

        peck.FollowUpState = cycle;
        scrape.FollowUpState = cycle;

        states.AddRange([peck, scrape, cycle]);
        return new MonsterMoveStateMachine(states, peck);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        int burstDamage = Math.Max(1, (int)Math.Ceiling(Creature.MaxHp * 0.2m));
        await PowerCmd.SetAmount<FissionBurstOnDeathPower>(Creature, burstDamage, Creature, null);
    }

    private async Task ShardPeckMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(PeckDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task ShardScrapeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ScrapeDamage).WithHitCount(ScrapeHits).FromMonster(this).OnlyPlayAnimOnce().WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }
}
