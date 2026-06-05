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

[CustomID("ECHO_CORE_MONSTER_SABYR_BOAR")]
public sealed class WuwaSabyrBoar : WuwaStaticMonsterBase
{
    private bool _usedSnortLastTurn;

    protected override string TexturePath => "res://echo-core/ui/monsters/wuwa/sabyr_boar_battle.png";

    protected override Vector2 VisualScale => new(0.58f, 0.58f);

    protected override Vector2 VisualPosition => new(0f, -88f);

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 46, 42);

    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 46);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;

    private int GoreDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 10);

    private int RushDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 6);

    private int RushHits => 2;

    private int SnortStrength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = [];

        MoveState gore = new("WILD_GORE", WildGoreMove, new SingleAttackIntent(GoreDamage));
        MoveState rush = new("TUSK_RUSH", TuskRushMove, new MultiAttackIntent(RushDamage, RushHits));
        MoveState snort = new("FERAL_SNORT", FeralSnortMove, new BuffIntent());

        ConditionalBranchState opening = new("OPENING_BRANCH");
        opening.AddState(gore, () => true);

        ConditionalBranchState followUp = new("FOLLOW_UP_BRANCH");
        followUp.AddState(rush, () => _usedSnortLastTurn || Creature.HasPower<StrengthPower>());
        followUp.AddState(snort, () => !_usedSnortLastTurn && !Creature.HasPower<StrengthPower>());
        followUp.AddState(gore, () => true);

        gore.FollowUpState = followUp;
        rush.FollowUpState = followUp;
        snort.FollowUpState = followUp;

        states.AddRange([opening, gore, rush, snort, followUp]);
        return new MonsterMoveStateMachine(states, opening);
    }

    private async Task WildGoreMove(IReadOnlyList<Creature> targets)
    {
        _usedSnortLastTurn = false;
        await DamageCmd.Attack(GoreDamage).FromMonster(this).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task TuskRushMove(IReadOnlyList<Creature> targets)
    {
        _usedSnortLastTurn = false;
        await DamageCmd.Attack(RushDamage).WithHitCount(RushHits).FromMonster(this).OnlyPlayAnimOnce().WithHitFx("vfx/vfx_attack_slash").Execute(null);
    }

    private async Task FeralSnortMove(IReadOnlyList<Creature> targets)
    {
        _usedSnortLastTurn = true;
        await PowerCmd.Apply<StrengthPower>(Creature, SnortStrength, Creature, null);
    }
}
