using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Powers;
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

[CustomID("ECHO_CORE_MONSTER_BABY_VIRIDBLAZE_SAURIAN")]
public sealed class WuwaBabyViridblazeSaurian : WuwaStaticMonsterBase
{
    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/baby_viridblaze_saurian_battle.png";

    protected override Vector2 VisualScale => new(0.19f, 0.19f);

    protected override Vector2 VisualPosition => new(0f, -108f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 68, 62);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 72, 68);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

    private int ScorchBiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);

    private int HeatwavePounceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 15);

    private int EmpoweredPounceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 18);

    private int HardenedHideBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 11, 10);

    private int HeatThreshold => 3;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState bite = new("SCORCH_BITE", ScorchBiteMove, new SingleAttackIntent(ScorchBiteDamage));
        MoveState hide = new("HARDENED_HIDE", HardenedHideMove, new DefendIntent(), new BuffIntent());
        MoveState pounce = new("HEATWAVE_POUNCE", HeatwavePounceMove, new SingleAttackIntent(HeatwavePounceDamage), new DebuffIntent());

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(hide, () => true);

        ConditionalBranchState cycle = new("CYCLE_BRANCH");
        cycle.AddState(pounce, HasReachedHeatThreshold);
        cycle.AddState(hide, () => !HasReachedHeatThreshold() && Rng.NextBool());
        cycle.AddState(bite, () => true);

        bite.FollowUpState = cycle;
        hide.FollowUpState = cycle;
        pounce.FollowUpState = cycle;

        states.AddRange([opening, bite, hide, pounce, cycle]);
        return new MonsterMoveStateMachine(states, opening);
    }

    private async Task ScorchBiteMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(ScorchBiteDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
        await GainHeat(1);
    }

    private async Task HardenedHideMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, HardenedHideBlock, ValueProp.Move, null);
        await GainHeat(1);
    }

    private async Task HeatwavePounceMove(IReadOnlyList<Creature> targets)
    {
        bool empowered = HasReachedHeatThreshold();
        int damage = empowered ? EmpoweredPounceDamage : HeatwavePounceDamage;

        await DamageCmd.Attack(damage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        await PowerCmd.Apply<FrailPower>(targets, 1m, Creature, null);

        if (empowered)
        {
            await PowerCmd.Remove<SaurianHeatPower>(Creature);
        }
        else
        {
            await GainHeat(1);
        }
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<SaurianHeatPower>(Creature, 0m, Creature, null);
    }

    private async Task GainHeat(int amount)
    {
        await PowerCmd.Apply<SaurianHeatPower>(Creature, amount, Creature, null);
    }

    private bool HasReachedHeatThreshold()
    {
        return Creature.GetPower<SaurianHeatPower>()?.Amount >= HeatThreshold;
    }
}
