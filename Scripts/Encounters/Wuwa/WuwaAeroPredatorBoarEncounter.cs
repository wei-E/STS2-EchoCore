using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_AERO_PREDATOR_BOAR")]
public sealed class WuwaAeroPredatorBoarEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_aero_predator_boar.tscn";

    public override IReadOnlyList<string> Slots => ["front", "back"];

    public WuwaAeroPredatorBoarEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => false;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Crawler];

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<WuwaAeroPredator>(),
        ModelDb.Monster<WuwaSabyrBoar>(),
    ];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Hive;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        WuwaSabyrBoar boar = (WuwaSabyrBoar)ModelDb.Monster<WuwaSabyrBoar>().ToMutable();
        WuwaAeroPredator predator = (WuwaAeroPredator)ModelDb.Monster<WuwaAeroPredator>().ToMutable();
        predator.OpenWithGust = true;

        return
        [
            (boar, "front"),
            (predator, "back"),
        ];
    }
}
