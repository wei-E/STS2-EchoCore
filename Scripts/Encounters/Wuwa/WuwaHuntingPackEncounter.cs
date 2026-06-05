using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_HUNTING_PACK")]
public sealed class WuwaHuntingPackEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_hunting_pack.tscn";

    public override IReadOnlyList<string> Slots => ["left", "middle", "right"];

    public WuwaHuntingPackEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => false;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Crawler];

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<WuwaVanguardJunrock>(),
        ModelDb.Monster<WuwaSabyrBoar>(),
        ModelDb.Monster<WuwaGlacioPrism>(),
    ];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Overgrowth;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<WuwaVanguardJunrock>().ToMutable(), "left"),
            (ModelDb.Monster<WuwaSabyrBoar>().ToMutable(), "middle"),
            (ModelDb.Monster<WuwaGlacioPrism>().ToMutable(), "right"),
        ];
    }
}
