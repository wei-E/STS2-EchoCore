using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace EchoCore.Scripts.Monsters.Wuwa;

/// <summary>
/// 二层鸣潮精英：踏光兽。
/// 通过半血狂暴和抽牌震慑把压力从“吃伤害”扩展到“处理手牌”。
/// </summary>
[CustomID("ECHO_CORE_ELITE_LIGHTTREADER_BEAST")]
public sealed class WuwaLighttreaderBeast : WuwaStaticMonsterBase
{
    private bool _hasEnraged;
    private bool _hasQueuedEnrageRush;

    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/lighttreader_beast_battle.png";

    protected override Vector2 VisualScale => new(0.44f, 0.44f);

    protected override Vector2 VisualPosition => new(-38f, -142f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 174, 166);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 180, 172);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

    private int RadiantPounceDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 19, 17);

    private int PrismaticRendDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);

    private int DawntreadRushDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 24);

    private int EnrageStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 4, 3);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState pounce = new("RADIANT_POUNCE", RadiantPounceMove, new SingleAttackIntent(RadiantPounceDamage));
        MoveState stare = new("STAGGERING_STARE", StaggeringStareMove, new DebuffIntent());
        MoveState rend = new("PRISMATIC_REND", PrismaticRendMove, new MultiAttackIntent(PrismaticRendDamage, 2));
        MoveState rush = new("DAWNTREAD_RUSH", DawntreadRushMove, new SingleAttackIntent(DawntreadRushDamage), new BuffIntent());

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(stare, () => true);

        ConditionalBranchState afterPounce = new("AFTER_POUNCE_BRANCH");
        afterPounce.AddState(rush, ShouldUseEnrageRush);
        afterPounce.AddState(stare, () => !AllPlayersAreStaggered());
        afterPounce.AddState(rend, () => true);

        ConditionalBranchState afterRend = new("AFTER_REND_BRANCH");
        afterRend.AddState(rush, ShouldUseEnrageRush);
        afterRend.AddState(stare, () => !AllPlayersAreStaggered());
        afterRend.AddState(pounce, () => true);

        pounce.FollowUpState = afterPounce;
        stare.FollowUpState = rend;
        rend.FollowUpState = afterRend;
        rush.FollowUpState = afterRend;

        states.AddRange([opening, pounce, stare, rend, rush, afterPounce, afterRend]);
        return new MonsterMoveStateMachine(states, opening);
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        await base.AfterCurrentHpChanged(creature, delta);

        if (creature != Creature || HasEnraged || creature.CurrentHp <= 0)
        {
            return;
        }

        if (creature.CurrentHp * 2 > creature.MaxHp)
        {
            return;
        }

        _hasEnraged = true;
        _hasQueuedEnrageRush = true;
        NRunMusicController.Instance?.TriggerEliteSecondPhase();
        await PowerCmd.Apply<LighttreaderEnragedPower>(creature, 1m, creature, null);
        await PowerCmd.Apply<StrengthPower>(creature, EnrageStrength, creature, null);
    }

    private bool HasEnraged => _hasEnraged || Creature.HasPower<LighttreaderEnragedPower>();

    private bool AllPlayersAreStaggered()
    {
        return CombatState.Players.All(player => player.Creature.HasPower<LighttreaderStaggerPower>());
    }

    private bool ShouldUseEnrageRush()
    {
        if (!HasEnraged || !_hasQueuedEnrageRush)
        {
            return false;
        }

        _hasQueuedEnrageRush = false;
        return true;
    }

    private async Task RadiantPounceMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(RadiantPounceDamage).FromMonster(this).WithHitFx("vfx/vfx_bite").Execute(null);
    }

    private async Task StaggeringStareMove(IReadOnlyList<Creature> targets)
    {
        foreach (Creature target in targets)
        {
            await PowerCmd.SetAmount<LighttreaderStaggerPower>(target, 2m, Creature, null);
        }
    }

    private async Task PrismaticRendMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(PrismaticRendDamage).WithHitCount(2).OnlyPlayAnimOnce().FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task DawntreadRushMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(DawntreadRushDamage).FromMonster(this).WithHitFx("vfx/vfx_bite").Execute(null);
        await PowerCmd.Apply<VigorPower>(Creature, 6m, Creature, null);
    }
}
