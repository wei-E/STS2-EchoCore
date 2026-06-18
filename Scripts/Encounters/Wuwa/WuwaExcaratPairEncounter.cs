using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_EXCARAT_PAIR")]
public sealed class WuwaExcaratPairEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_excarat_pair.tscn";

    public override IReadOnlyList<string> Slots => ["front", "back"];

    public WuwaExcaratPairEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => true;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Burrower];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<WuwaExcarat>()];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Hive;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        WuwaExcarat front = (WuwaExcarat)ModelDb.Monster<WuwaExcarat>().ToMutable();
        front.OpenWithBurrow = false;

        WuwaExcarat back = (WuwaExcarat)ModelDb.Monster<WuwaExcarat>().ToMutable();
        back.OpenWithBurrow = true;

        return
        [
            (front, "front"),
            (back, "back"),
        ];
    }
}
