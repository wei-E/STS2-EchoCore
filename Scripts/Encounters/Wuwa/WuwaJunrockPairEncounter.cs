using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

[CustomID("ECHO_CORE_ENCOUNTER_JUNROCK_PAIR")]
public sealed class WuwaJunrockPairEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_junrock_pair.tscn";

    public override IReadOnlyList<string> Slots => ["front", "back"];

    public WuwaJunrockPairEncounter() : base(RoomType.Monster)
    {
    }

    public override bool IsWeak => false;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Slimes];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<WuwaVanguardJunrock>()];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Overgrowth;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        WuwaVanguardJunrock front = (WuwaVanguardJunrock)ModelDb.Monster<WuwaVanguardJunrock>().ToMutable();
        WuwaVanguardJunrock back = (WuwaVanguardJunrock)ModelDb.Monster<WuwaVanguardJunrock>().ToMutable();

        // 同一遭遇里故意让两只幼岩分流开场，避免总是同步出招。
        front.OpenWithListen = true;
        back.OpenWithListen = false;

        return
        [
            (front, "front"),
            (back, "back"),
        ];
    }
}
