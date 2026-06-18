using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_EXCARAT_JUNROCK")]
public sealed class WuwaExcaratJunrockEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_excarat_junrock.tscn";

    public override IReadOnlyList<string> Slots => ["front", "back"];

    public WuwaExcaratJunrockEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => false;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Burrower, EncounterTag.Slimes];

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<WuwaExcarat>(),
        ModelDb.Monster<WuwaVanguardJunrock>(),
    ];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Hive;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        WuwaExcarat excarat = (WuwaExcarat)ModelDb.Monster<WuwaExcarat>().ToMutable();
        excarat.OpenWithBurrow = true;

        WuwaVanguardJunrock junrock = (WuwaVanguardJunrock)ModelDb.Monster<WuwaVanguardJunrock>().ToMutable();
        junrock.OpenWithListen = false;

        return
        [
            (junrock, "front"),
            (excarat, "back"),
        ];
    }
}
