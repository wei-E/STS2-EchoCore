using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_SOLO_AERO_PREDATOR")]
public sealed class WuwaSoloAeroPredatorEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_solo_aero_predator.tscn";

    public override IReadOnlyList<string> Slots => ["solo"];

    public WuwaSoloAeroPredatorEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => true;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Crawler];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<WuwaAeroPredator>()];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Overgrowth;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        WuwaAeroPredator predator = (WuwaAeroPredator)ModelDb.Monster<WuwaAeroPredator>().ToMutable();
        predator.OpenWithGust = true;

        return
        [
            (predator, "solo"),
        ];
    }
}
