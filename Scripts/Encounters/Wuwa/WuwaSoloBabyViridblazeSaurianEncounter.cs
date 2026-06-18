using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_SOLO_BABY_VIRIDBLAZE_SAURIAN")]
public sealed class WuwaSoloBabyViridblazeSaurianEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_solo_baby_viridblaze_saurian.tscn";

    public override IReadOnlyList<string> Slots => ["solo"];

    public WuwaSoloBabyViridblazeSaurianEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => true;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Crawler];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<WuwaBabyViridblazeSaurian>()];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Overgrowth;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<WuwaBabyViridblazeSaurian>().ToMutable(), "solo"),
        ];
    }
}
