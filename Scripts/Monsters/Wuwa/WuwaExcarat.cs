using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace EchoCore.Scripts.Monsters.Wuwa;

[CustomID("ECHO_CORE_MONSTER_EXCARAT")]
public sealed class WuwaExcarat : WuwaStaticMonsterBase
{
    private bool _openWithBurrow;

    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/excarat_battle.png";

    protected override Vector2 VisualScale => new(0.14f, 0.14f);

    protected override Vector2 VisualPosition => new(0f, -90f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 36);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 40);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

    private int BiteDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);

    private int TunnelChokeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

    private int BurrowBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 32, 30);

    private int TunnelStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 2, 1);

    private int TunnelWeak => 1;

    /// <summary>
    /// 多只遁地鼠同场时，允许遭遇显式拆开开场分支，避免全部同步钻地。
    /// </summary>
    public bool OpenWithBurrow
    {
        get => _openWithBurrow;
        set
        {
            AssertMutable();
            _openWithBurrow = value;
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState bite = new("BITE_MOVE", BiteMove, new SingleAttackIntent(BiteDamage));
        MoveState burrow = new("BURROW_MOVE", BurrowMove, new BuffIntent(), new DefendIntent());
        MoveState tunnelChoke = new("TUNNEL_CHOKE_MOVE", TunnelChokeMove, new SingleAttackIntent(TunnelChokeDamage), new DebuffIntent(), new BuffIntent());
        MoveState dizzyDust = new("DIZZY_DUST_MOVE", DizzyDustMove, new StatusIntent(1));

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(burrow, () => OpenWithBurrow);
        opening.AddState(bite, () => true);

        ConditionalBranchState surfaceCycle = new("SURFACE_CYCLE_BRANCH");
        surfaceCycle.AddState(burrow, () => !Creature.HasPower<BurrowedPower>() && Rng.NextBool());
        surfaceCycle.AddState(bite, () => !Creature.HasPower<BurrowedPower>());
        surfaceCycle.AddState(tunnelChoke, () => true);

        ConditionalBranchState undergroundCycle = new("UNDERGROUND_CYCLE_BRANCH");
        undergroundCycle.AddState(tunnelChoke, () => Creature.HasPower<BurrowedPower>());
        undergroundCycle.AddState(bite, () => true);

        bite.FollowUpState = surfaceCycle;
        burrow.FollowUpState = tunnelChoke;
        tunnelChoke.FollowUpState = dizzyDust;
        dizzyDust.FollowUpState = undergroundCycle;

        states.AddRange([opening, bite, burrow, tunnelChoke, dizzyDust, surfaceCycle, undergroundCycle]);
        return new MonsterMoveStateMachine(states, opening);
    }

    private async Task BiteMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(BiteDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task BurrowMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<BurrowedPower>(Creature, 1m, Creature, null);
        await CreatureCmd.GainBlock(Creature, BurrowBlock, ValueProp.Move, null);
    }

    private async Task TunnelChokeMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TunnelChokeDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_slash").Execute(null);
        await PowerCmd.Apply<WeakPower>(targets, TunnelWeak, Creature, null);
        await PowerCmd.Apply<StrengthPower>(Creature, TunnelStrength, Creature, null);
    }

    private async Task DizzyDustMove(IReadOnlyList<Creature> targets)
    {
        foreach (Creature target in targets.Where(target => target.IsAlive))
        {
            await CardPileCmd.AddToCombatAndPreview<Dazed>(
                target,
                PileType.Draw,
                1,
                addedByPlayer: false,
                CardPilePosition.Random);
        }
    }
}
