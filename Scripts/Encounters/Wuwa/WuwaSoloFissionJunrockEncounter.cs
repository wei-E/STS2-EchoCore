using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_SOLO_FISSION_JUNROCK")]
public sealed class WuwaSoloFissionJunrockEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_solo_fission_junrock.tscn";

    public override IReadOnlyList<string> Slots => ["main", "split"];

    public WuwaSoloFissionJunrockEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => true;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Slimes];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<WuwaFissionJunrock>()];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Overgrowth;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<WuwaFissionJunrock>().ToMutable(), "main"),
        ];
    }
}
