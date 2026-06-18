using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_FISSION_SAURIAN")]
public sealed class WuwaFissionSaurianEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_fission_saurian.tscn";

    public override IReadOnlyList<string> Slots => ["left", "middle", "right"];

    public WuwaFissionSaurianEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => false;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Crawler, EncounterTag.Slimes];

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<WuwaFissionJunrock>(),
        ModelDb.Monster<WuwaBabyViridblazeSaurian>(),
    ];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Hive;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<WuwaBabyViridblazeSaurian>().ToMutable(), "left"),
            (ModelDb.Monster<WuwaFissionJunrock>().ToMutable(), "middle"),
        ];
    }
}
