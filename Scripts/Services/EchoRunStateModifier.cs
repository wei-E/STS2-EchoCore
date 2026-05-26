using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace EchoCore.Scripts.Services;

/// <summary>
/// 把声骸库存、装备槽和调谐状态挂到本局 Run 的 modifier 上。
/// 这里不直接拆成大量 SavedProperty 字段，而是统一存一份 JSON 快照，后续扩字段时兼容成本更低。
/// </summary>
public sealed class EchoRunStateModifier : ModifierModel
{
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string EchoCoreSnapshotJson { get; private set; } = string.Empty;

    public void SetSnapshot(string snapshotJson)
    {
        AssertMutable();
        EchoCoreSnapshotJson = snapshotJson ?? string.Empty;
    }

    protected override void AfterRunLoaded(RunState runState)
    {
        EchoPersistenceService.RestoreFromSavedModifier(runState, this);
    }
}
