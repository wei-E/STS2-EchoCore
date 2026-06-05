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

[CustomID("ECHO_CORE_MONSTER_GLACIO_PRISM")]
public sealed class WuwaGlacioPrism : WuwaStaticMonsterBase
{
    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/glacio_prism_battle.png";

    protected override Vector2 VisualScale => new(0.56f, 0.56f);

    protected override Vector2 VisualPosition => new(0f, -108f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 34, 30);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 38, 34);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

    private int RefractDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

    private int GlimmerBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 5, 4);

    private int GlimmerStrength => 1;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        Creature.Died += OnDied;
    }

    public override void BeforeRemovedFromRoom()
    {
        Creature.Died -= OnDied;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState glimmer = new("CONDENSE_GLIMMER", CondenseGlimmerMove, new BuffIntent());
        MoveState refract = new("FROST_REFRACT", FrostRefractMove, new SingleAttackIntent(RefractDamage));

        glimmer.FollowUpState = refract;
        refract.FollowUpState = glimmer;

        states.AddRange([glimmer, refract]);
        return new MonsterMoveStateMachine(states, glimmer);
    }

    private async Task CondenseGlimmerMove(IReadOnlyList<Creature> targets)
    {
        IReadOnlyList<Creature> allies = CombatState.Enemies.Where(enemy => enemy != Creature && enemy.IsAlive).ToList();
        if (allies.Count == 0)
        {
            await CreatureCmd.GainBlock(Creature, GlimmerBlock, ValueProp.Move, null);
            return;
        }

        foreach (Creature ally in allies)
        {
            await CreatureCmd.GainBlock(ally, GlimmerBlock, ValueProp.Move, null);
        }

        await PowerCmd.Apply<StrengthPower>(allies, GlimmerStrength, Creature, null);
    }

    private async Task FrostRefractMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(RefractDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private void OnDied(Creature deadCreature)
    {
        Creature.Died -= OnDied;
        _ = TriggerShatterBuffAsync();
    }

    private async Task TriggerShatterBuffAsync()
    {
        IReadOnlyList<Creature> allies = CombatState.Enemies.Where(enemy => enemy != Creature && enemy.IsAlive).ToList();
        if (allies.Count == 0)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(allies, 1m, Creature, null);
    }
}
