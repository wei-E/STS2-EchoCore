using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_PREDATOR_AMBUSH")]
public sealed class WuwaPredatorAmbushEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_predator_ambush.tscn";

    public override IReadOnlyList<string> Slots => ["front", "back"];

    public WuwaPredatorAmbushEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => false;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Nibbit];

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<WuwaVanguardJunrock>(),
        ModelDb.Monster<WuwaElectroPredator>(),
    ];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Overgrowth;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<WuwaVanguardJunrock>().ToMutable(), "front"),
            (ModelDb.Monster<WuwaElectroPredator>().ToMutable(), "back"),
        ];
    }
}
