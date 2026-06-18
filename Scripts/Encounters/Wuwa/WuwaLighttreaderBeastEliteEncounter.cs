using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using EchoCore.Scripts.Monsters.Wuwa;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;

namespace EchoCore.Scripts.Encounters.Wuwa;

/// <summary>
/// 二层鸣潮精英：踏光兽。
/// 第一版先做单体精英，优先验证震慑机制本身。
/// </summary>
[CustomID("ECHO_CORE_ENCOUNTER_LIGHTTREADER_BEAST_ELITE")]
public sealed class WuwaLighttreaderBeastEliteEncounter : CustomEncounterModel
{
    public override string? CustomScenePath => "res://scenes/encounters/echo_core_encounter_lighttreader_beast_elite.tscn";

    public override IReadOnlyList<string> Slots => ["boss"];

    public WuwaLighttreaderBeastEliteEncounter() : base(RoomType.Elite)
    {
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<WuwaLighttreaderBeast>()];

    public override bool IsValidForAct(ActModel act)
    {
        return act is Hive;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return
        [
            (ModelDb.Monster<WuwaLighttreaderBeast>().ToMutable(), "boss"),
        ];
    }
}
