# 长路启航之星合鸣接入规划

- 日期：2026-05-30
- 范围：EchoCore 对外 API、爱弥斯软依赖接入、长路启航之星合鸣注册
- 目标仓库：
  - EchoCore：`E:\code\mods\STS2-EchoCore`
  - 爱弥斯：`E:\code\mods\aemeath-ww`
  - BaseLib：`E:\code\mods\BaseLib-StS2`

## 目标效果

新增爱弥斯专属合鸣套装：

- ID 建议：`aemeath-ww:long_voyage_star`
- 中文名：`长路启航之星`
- 2 件套：开局对随机敌人附加 3 点聚爆。
- 3 件套：额外增加，开局获得 3 点同步率。
- 5 件套：额外增加，开局获得 1 点共鸣率。

同时把 EchoCore 当前全部声骸都加入 `长路启航之星` 的候选合鸣池，使后续掉落的声骸都有机会随机归属该合鸣。

## 当前 EchoCore 合鸣机制

EchoCore 当前合鸣链路如下：

- `SonataDefinition` 描述合鸣元数据与断点文案。
- `EchoDefinition.SonataIds` 是声骸可归属的候选合鸣池。
- 掉落时 `EchoDropService` 从候选合鸣池中随机选中一个，写入 `EchoInstance.SelectedSonataId`。
- 开战时 `EchoCombatEffectService.GetActiveSonataSummaries` 统计已装备声骸的合鸣件数。
- 统计规则是同一套装内，同一个 `DefinitionId` 最多计数一次，避免同名声骸重复刷件数。
- `ISonataEffectHandler.OnCombatStart` 按已激活断点执行效果。

因此，把某个合鸣追加到所有声骸定义后，主要影响后续掉落的新实例。已经获得的旧声骸实例不会自动改变 `SelectedSonataId`。

## EchoCore 需要新增的对外 API

当前 `EchoRegistry` 已有底层方法：

- `RegisterSonata(SonataDefinition definition)`
- `RegisterSonataEffectHandler(ISonataEffectHandler handler)`
- `TryAddSonataToEcho(string echoId, string sonataId)`

但这些方法暴露了 EchoCore 类型，第三方 Mod 直接引用会形成硬依赖。建议新增稳定门面：

```csharp
namespace EchoCore.Scripts.Api;

public static class EchoCoreApi
{
    public static bool RegisterSonata(
        string id,
        string nameKey,
        string descriptionKey,
        string iconPath,
        IReadOnlyList<(int RequiredCount, string DescriptionKey)> breakpoints);

    public static bool RegisterSonataOnCombatStartHandler(
        string sonataId,
        Func<Player, int, IReadOnlyList<int>, Task> handler);

    public static bool TryAddSonataToEcho(string echoId, string sonataId);

    public static int AddSonataToAllEchoes(string sonataId);
}
```

设计要点：

- API 参数尽量使用基础类型、`Player`、`Func`、`Task`，避免第三方必须引用 EchoCore 的 record/interface。
- `RegisterSonataOnCombatStartHandler` 内部用 adapter 包装成 `ISonataEffectHandler`。
- `AddSonataToAllEchoes` 遍历 `EchoRegistry.Echoes`，对所有当前已注册声骸调用 `TryAddSonataToEcho`，返回成功追加或已存在的数量。
- 重复注册时建议返回 `false` 或记录日志，不让第三方 Mod 因二次初始化直接崩溃。
- 保留 `EchoRegistry` 作为内部/硬依赖高级入口，但推荐外部软依赖只走 `EchoCoreApi`。

## 爱弥斯软依赖方案

爱弥斯不在 manifest 中声明 `EchoCore` 依赖，仍保持：

```json
"dependencies": ["BaseLib"]
```

软依赖通过 BaseLib 的 `ModInterop` 实现：

- 在爱弥斯新增 `EchoCoreInterop` wrapper。
- 使用 `[ModInterop("EchoCore", "EchoCore.Scripts.Api.EchoCoreApi")]` 指向 EchoCore API。
- wrapper 方法签名与 `EchoCoreApi` 保持一致。
- 如果 EchoCore 未加载，BaseLib 不会生成桥接方法，爱弥斯侧注册逻辑应捕获异常并静默跳过。

建议在爱弥斯新增一个 Harmony patch：

- patch 目标：`LocManager.Initialize`
- 时机：`HarmonyPostfix`
- 原因：BaseLib 的 ModInterop 在 `LocManager.Initialize` prefix 阶段生成桥接方法，爱弥斯在 postfix 调用最稳。
- 增加一次性 guard，避免语言切换或重复初始化导致重复注册。

## 长路启航之星效果实现建议

爱弥斯侧注册逻辑：

1. 调用 `EchoCoreInterop.RegisterSonata(...)` 注册：
   - `aemeath-ww:long_voyage_star`
   - `AEMEATH_ECHO_LONG_VOYAGE_STAR.name`
   - `AEMEATH_ECHO_LONG_VOYAGE_STAR.description`
   - 图标先可用爱弥斯现有资源，后续替换正式合鸣图标。
   - 断点：2、3、5。
2. 调用 `EchoCoreInterop.RegisterSonataOnCombatStartHandler(...)` 注册开战效果。
3. 调用 `EchoCoreInterop.AddSonataToAllEchoes("aemeath-ww:long_voyage_star")`，把当前 EchoCore 已注册声骸全部加入候选池。

效果处理伪代码：

```csharp
private static async Task ApplyLongVoyageStar(Player player, int equippedCount, IReadOnlyList<int> activeBreakpoints)
{
    if (activeBreakpoints.Contains(2))
    {
        Creature? target = player.Creature.CombatState?.HittableEnemies
            .Where(enemy => !enemy.IsDead)
            .ToList()
            .RandomFrom(player.PlayerRng.CombatTargets);

        if (target != null)
        {
            await AemeathFusionBurstState.TryAddFusionBurst(target, 3, player.Creature, null);
        }
    }

    if (activeBreakpoints.Contains(3))
    {
        await AemeathResourceState.GainSync(player.Creature, 3, player.Creature, null);
    }

    if (activeBreakpoints.Contains(5))
    {
        await AemeathResourceState.GainResonance(player.Creature, 1, player.Creature, null);
    }
}
```

实现时需要按爱弥斯仓库现有 RNG API 写法调整随机敌人选择，不必照抄伪代码。

## 本地化规划

爱弥斯新增本地化 key，建议放在 `aemeath-ww/localization/*/monsters.json` 或新增专用表之前先沿用 EchoCore 当前 UI 读取的 `monsters` 表。

中文建议：

```json
{
  "AEMEATH_ECHO_LONG_VOYAGE_STAR.name": "长路启航之星",
  "AEMEATH_ECHO_LONG_VOYAGE_STAR.description": "属于爱弥斯的合鸣，回应漫长航路上的第一颗启明星。",
  "AEMEATH_ECHO_LONG_VOYAGE_STAR.breakpoint_2": "2件：开局对随机敌人附加3点聚爆。",
  "AEMEATH_ECHO_LONG_VOYAGE_STAR.breakpoint_3": "3件：额外开局获得3点同步率。",
  "AEMEATH_ECHO_LONG_VOYAGE_STAR.breakpoint_5": "5件：额外开局获得1点共鸣率。"
}
```

英文可后续补齐，先保持可读占位。

## 兼容性与风险

- 旧存档中已经获得的声骸不会自动变成 `长路启航之星`，因为实例保存的是最终 `SelectedSonataId`。
- 如果需要让旧声骸也能转换，需要额外设计迁移或调谐机制，本阶段不建议做。
- 如果爱弥斯未加载，EchoCore 不应出现任何爱弥斯合鸣。
- 如果 EchoCore 未加载，爱弥斯应正常运行，只跳过合鸣注册。
- `AddSonataToAllEchoes` 会扩大所有 EchoCore 声骸的候选池，意味着原有合鸣出现概率下降；这是本需求的预期结果。
- 当前 EchoCore UI 的本地化读取主要走 `monsters` 表，因此爱弥斯合鸣文案也应先放进 `monsters.json`，或者后续统一把合鸣 UI 改为读取 `sonatas` 表。

## 推荐实施顺序

1. EchoCore 新增 `Scripts/Api/EchoCoreApi.cs`。
2. EchoCore 为 `Func<Player, int, IReadOnlyList<int>, Task>` 增加内部 `DelegateSonataEffectHandler` adapter。
3. EchoCore 增加 `AddSonataToAllEchoes` API，并补日志。
4. EchoCore build 验证。
5. 爱弥斯新增 `EchoCoreInterop` 与一次性注册 patch。
6. 爱弥斯新增长路启航之星本地化。
7. 爱弥斯 build 验证。
8. 进游戏验证：
   - 仅爱弥斯 + BaseLib：正常启动，无 EchoCore 报错。
   - EchoCore + 爱弥斯 + BaseLib：日志显示合鸣注册成功。
   - 新掉落声骸有机会出现 `长路启航之星`。
   - 装备 2/3/5 件后开战分别触发聚爆、同步率、共鸣率效果。
