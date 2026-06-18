using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_SOLO_GLACIO_PRISM")]
public sealed class WuwaSoloGlacioPrismEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_solo_glacio_prism.tscn";

    public override IReadOnlyList<string> Slots => ["left", "right"];

    public WuwaSoloGlacioPrismEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => true;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Workers];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<WuwaGlacioPrism>()];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Overgrowth;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<WuwaGlacioPrism>().ToMutable(), "left"),
            (ModelDb.Monster<WuwaGlacioPrism>().ToMutable(), "right"),
        ];
    }
}
