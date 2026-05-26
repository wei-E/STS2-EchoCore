# Echo Core 开发日志

- 项目：`mods/EchoCore`
- 最后更新：`2026-05-26`
- 当前阶段：`Phase 5 主动技 MVP`

## 2026-05-26 - 主动技卡模型注册修复

### Summary
- 修复战斗中点击 `声骸技` 按钮时报 `CARD.ECHO_CORE_CARD_* not found` 的问题。
- 根因是主动技卡虽然有定义和 UI 入口，但没有在 Mod 初始化阶段显式注入 `ModelDb`，导致首用时按 ID 取卡失败。

### Changes
- `Scripts/Cards/EchoSkillCardRegistry.cs`：新增主动技卡显式注册器，初始化时先构造卡模型，再调用 `ModelDb.Inject` 保证可按 ID 读取。
- `Scripts/Init/Entry.cs`：在注册声骸定义前先执行主动技卡注册。
- `Scripts/Registry/VanillaEchoBootstrap.cs`：声骸定义不再手写 `SkillCardId`，改为从主动技卡类型自动推导 entry，避免字符串和真实模型 ID 漂移。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Runtime file sync：PASS，`EchoCore.dll` 已同步到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\`。
- Notes：最初尝试通过初始化阶段手动 `new CardModel + ModelDb.Inject` 修复，结果触发 `DuplicateModelException`。现已回退该做法，改为只维护“skillCardId -> 卡牌类型”的映射，并在释放时按类型从 `ModelDb` 读取 canonical model。本次仅改 C#，未改资源和本地化，因此未重新导出 `EchoCore.pck`。

### Next
- 进游戏验证 `声骸技` 按钮能正常向手牌加入对应主动技卡。
- 若仍有报错，下一步直接在初始化日志里打印 5 张主动技卡的最终 `ModelId` 与 `ModelDb.Contains` 状态。

## 2026-05-26 - 主动技卡本地化键修复

### Summary
- 修复主动技卡牌名称和描述显示为 `cards.ECHOCORE-ECHO_CORE_CARD_*` key 的问题。
- 根因是 BaseLib 给自定义卡注入本地化时使用的是运行时 `content.Key.Entry`，而主动技卡实际 entry 带有 `ECHOCORE-` 前缀。

### Changes
- `EchoCore/localization/zhs/cards.json`：为 5 张主动技卡补齐 `ECHOCORE-ECHO_CORE_CARD_*` 的标题与描述键。
- `EchoCore/localization/eng/cards.json`：同步补齐英文本地化键，保持中英一致。

### Verification
- JSON：PASS，`cards.json` 中英文件 `ConvertFrom-Json` 校验通过。
- Export：PASS，重新导出 `EchoCore.pck`。
- Runtime file sync：PASS，已同步到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`。

### Next
- 进游戏确认主动技卡的标题、正文和数值动态变量显示正常。

## 2026-05-26 - Phase 5 主声骸主动技 MVP

### Summary
- 实现槽位 1 作为主声骸槽，主声骸在战斗中提供主动技按钮。
- 战斗中隐藏原声骸库存按钮，显示 `声骸技` 按钮。
- 点击按钮后生成该声骸定义固定绑定的主动技卡到手牌，并进入冷却。
- 主动技和声骸定义绑定，不参与词条随机、调谐或合鸣随机。

### Changes
- `Scripts/Echoes/EchoDefinition.cs`：新增 `SkillCooldownTurns`，用于配置主动技冷却。
- `Scripts/Cards/*`：新增 5 张 MVP 声骸主动技卡，分别绑定首批 5 个原版怪物声骸。
- `Scripts/Services/EchoActiveSkillService.cs`：新增主动技可用性、卡牌生成、冷却推进逻辑。
- `Scripts/Patches/HookAfterPlayerTurnStartEchoPatch.cs`：玩家回合开始时减少主动技冷却。
- `Scripts/UI/EchoInventoryOverlay.cs`：战斗中隐藏库存按钮，显示主动技按钮和冷却状态。
- `EchoCore/localization/*/cards.json`：新增主动技卡中英文文本。
- `E:\Code\sts2mod-dev\美术资源\声骸系统\需求.md`：同步主动技按钮与冷却规则。

### Active Skill Rules
- 槽位 1 是主声骸槽。
- 只有主声骸提供主动技。
- MVP 主动技只支持 TacticalCard：点击按钮生成 1 张绑定卡牌到手牌。
- 使用后进入冷却；Common 默认 3 回合，Elite 默认 4 回合，Overlord / Calamity 默认 5 回合。
- 只有玩家可行动回合、未结束回合、冷却为 0 且手牌未满时可用。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Export：PASS，Godot `--export-pack` 成功生成 `EchoCore.pck`。
- Runtime file sync：PASS，已同步到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\`。

### Next
- 进游戏验证槽位 1 装备不同声骸时，按钮生成对应主动技卡。
- 后续补主动技 UI 图标、冷却悬浮详情和多人保护模式。

## 2026-05-26 - Phase 4 调谐 MVP

### Summary
- 在火堆新增 `调谐` 选项。
- 选择火堆调谐后，现有声骸面板进入一次性调谐模式，可对一个声骸重骰唯一词条。
- 调谐会按声骸类型扣除金币，并立刻刷新实例词条与面板显示。

### Changes
- `Scripts/Services/EchoTuningService.cs`：新增调谐状态、费用计算、词条重骰与金币扣除逻辑。
- `Scripts/Services/EchoInventory.cs`：新增 `ReplaceInstance`，用于调谐后原地替换库存实例。
- `Scripts/RestSite/EchoTuneRestSiteOption.cs`：新增火堆调谐选项。
- `Scripts/Patches/RestSiteEchoTuningPatches.cs`：在火堆选项列表中追加调谐入口，并给按钮覆盖图标。
- `Scripts/UI/EchoInventoryOverlay.cs`：新增调谐模式提示与每个声骸的 `调谐` 按钮。
- `EchoCore/localization/*/rest_site_ui.json`：新增火堆调谐选项的中英文文案。

### Tuning Rules
- Common：`50` 金
- Elite：`75` 金
- Overlord：`100` 金
- Calamity：`125` 金
- 每次火堆调谐只允许成功完成 `1` 次。
- 当前 MVP 只重骰唯一词条，不新增词条槽。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。

## 2026-05-26 - 持久化接入（库存 / 装备 / 调谐状态）

### Summary
- 将声骸库存、装备槽和火堆调谐待处理状态接入本局 Run 存档。
- 退出到主菜单后继续游戏，已获得声骸、装备位和调谐待处理状态会随读档恢复。

### Changes
- `Scripts/Services/EchoRunStateModifier.cs`：新增本局状态 modifier，使用 `SavedProperty` 保存 EchoCore JSON 快照。
- `Scripts/Services/EchoPersistenceService.cs`：新增运行时状态与存档快照双向同步服务。
- `Scripts/Services/EchoInventory.cs`：新增运行时快照导出/恢复/清空接口，并在增删改装备后自动写回持久化。
- `Scripts/Services/EchoTuningService.cs`：将调谐待处理状态纳入导出/恢复，并在开启/关闭调谐时自动写回持久化。

### Persistence Scope
- 已保存：
  - 声骸库存实例
  - 5 个装备槽的实例绑定
  - 火堆调谐待处理状态
- 仍未保存：
  - 独立声骸 UI 面板开关状态
  - 未来尚未实现的主动技冷却、COST 战斗态等临时状态

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。

### Next
- 进游戏验证 `获得声骸 -> 装备 -> 退主菜单 -> 继续游戏` 的恢复链路。
- 若恢复稳定，再把调谐结果提示和更多战斗态字段接入同一套快照结构。

## 2026-05-26 - 多候选合鸣、实例单归属与外部扩展入口

### Summary
- 调整合鸣归属模型：声骸定义可挂多个候选合鸣，但单个掉落实例只会随机归属其中 1 个。
- 新增对外扩展入口，允许外部 Mod 为“已注册声骸”追加新的候选合鸣。
- 将多尼斯异鸟作为样例声骸，允许在 `基础残响 / 隐世回光` 两个候选合鸣之间随机掉落。

### Changes
- `Scripts/Echoes/EchoDefinition.cs`：保留定义层 `SonataIds` 作为候选合鸣列表；为 `EchoInstance` 新增 `SelectedSonataId`。
- `Scripts/Registry/EchoRegistry.cs`：新增 `TryAddSonataToEcho`，用于为已注册声骸追加候选合鸣。
- `Scripts/Registry/VanillaEchoBootstrap.cs`：新增 `隐世回光` 合鸣定义，并让 `多尼斯异鸟声骸` 挂上双候选合鸣。
- `Scripts/Services/EchoDropService.cs`：掉落实例时，从候选合鸣列表中随机选出 1 个写入 `SelectedSonataId`。
- `Scripts/Services/EchoCombatEffectService.cs`：套装统计改为按实例 `SelectedSonataId` 结算；同一声骸定义在同一套装中最多计数 1 次。
- `Scripts/Rewards/EchoReward.cs` / `Scripts/UI/EchoInventoryOverlay.cs`：奖励与库存界面显示实例最终归属的合鸣。
- `EchoCore/localization/*/monsters.json`：补充 `隐世回光` 本地化。
- `E:\Code\sts2mod-dev\美术资源\声骸系统\需求.md`：同步文档规则与外部扩展接口说明。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。

## 2026-05-26 - Phase 2/3 奖励、装备与战斗生效 MVP

### Summary
- 实现战斗胜利后按来源怪物追加 1 个声骸奖励，当前掉率固定为 `100%` 方便验证。
- 实现最小库存与 5 个装备槽 UI，支持装备、卸下、切槽。
- 实现最小战斗生效：装备词条在战斗开始时生效，基础残响合鸣的 2/3/5 件效果在战斗开始时生效。
- 修复库存和装备槽名称显示，改为读取正式本地化文本。

### Changes
- `Scripts/Rewards/EchoReward.cs`：新增自定义奖励，领取后把声骸实例放入临时库存。
- `Scripts/Services/EchoDropService.cs`：从战斗敌人来源匹配声骸定义，生成 1 条随机词条并追加奖励。
- `Scripts/Services/EchoInventory.cs`：新增内存库存、5 个装备槽、装备/卸下/查询接口。
- `Scripts/UI/EchoInventoryOverlay.cs`：新增战斗内/跑图内浮层入口，显示装备槽、库存与当前激活合鸣。
- `Scripts/Patches/RewardsSetEchoDropPatch.cs`：在原版奖励生成后追加声骸奖励。
- `Scripts/Patches/NRunEchoInventoryOverlayPatch.cs`：进入运行界面后挂载库存浮层。
- `Scripts/Services/EchoCombatEffectService.cs`：实现开战词条和基础残响合鸣效果。
- `Scripts/Patches/HookBeforeCombatStartEchoPatch.cs`：在 `Hook.BeforeCombatStart` 后应用已装备声骸效果。
- `EchoCore/localization/*/monsters.json`：补充运行时实际读取的声骸、词条、合鸣本地化。

### Combat Effect Scope
- 开战词条：
  - `echo_core:strength_start` -> 开战获得力量
  - `echo_core:dexterity_start` -> 开战获得敏捷
  - `echo_core:block_start` -> 开战获得格挡
- 基础残响合鸣：
  - `2` 件：开战获得 `4` 点格挡
  - `3` 件：额外获得 `1` 点力量
  - `5` 件：额外获得 `1` 点敏捷

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Export：PASS，Godot `--export-pack` 成功生成 `EchoCore.pck`。
- Runtime file sync：PASS，已同步到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\`。

### Next
- Phase 3 后续：
  - 实现更多触发时机的词条，而不仅限于开战。
  - 为声骸主动技 / COST 限制建立正式战斗入口。
  - 将库存与装备状态接入存档持久化。

## 2026-05-25 - Echo Core Phase 1 项目骨架

### Summary
- 新建 Godot + C# STS2 Mod 项目 `EchoCore`，接入 BaseLib、STS2、Harmony、Steamworks 引用。
- 实现声骸静态定义、实例、词条、合鸣、注册器和首批 5 个原版怪物声骸注册。

### Changes
- `EchoCore.csproj`：使用 `Godot.NET.Sdk/4.5.1`、`net9.0`、`Nullable=enable`，加入 BaseLib 版本目录回退路径，并在构建后复制 DLL/manifest 到游戏 mods 目录。
- `EchoCore.json`：新增 STS2 Mod manifest，声明依赖 BaseLib。
- `project.godot` / `export_presets.cfg`：新增 Godot 项目与 PCK 导出配置。
- `Scripts/Echoes/*`：新增声骸类型枚举、声骸定义和声骸实例模型。
- `Scripts/Affixes/EchoAffixModels.cs`：新增词条定义、档位定义、词条实例和档位稀有度。
- `Scripts/Sonata/SonataModels.cs`：新增合鸣套装与阈值定义。
- `Scripts/Registry/EchoRegistry.cs`：新增全局注册器，支持按声骸 ID 和怪物 ID 查询。
- `Scripts/Registry/VanillaEchoBootstrap.cs`：注册基础词条、占位合鸣和首批原版怪物声骸。
- `Scripts/Init/Entry.cs`：新增 Mod 初始化入口，清理并重建注册表，执行 Harmony PatchAll，输出注册数量日志。
- `echo-core/ui/echoes/icons/`：接入默认声骸图标与 1/3/4 COST 图标。
- `echo-core/localization/*/`：新增中英文 echoes/affixes/sonatas 本地化占位。

### Dependency Check
- `E:\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll`: OK
- `E:\Steam\steamapps\common\Slay the Spire 2\mods\BaseLib.3.1.3\BaseLib.dll`: OK
- `E:\Steam\steamapps\common\Slay the Spire 2\mods\BaseLib.3.1.3\BaseLib.pck`: OK
- `E:\Code\sts2mod-dev\mods\sts2-source-code`: OK
- `E:\Code\sts2mod-dev\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe`: OK

### Verification
- JSON 校验：PASS，`EchoCore.json` 与中英文本地化 JSON 均可 `ConvertFrom-Json`。
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Export：PASS，Godot `--export-pack` 成功生成 `EchoCore.pck`。
- Runtime file check：PASS，`E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\` 下存在 `EchoCore.dll`、`EchoCore.json`、`EchoCore.pck`。
- Notes：尚未进游戏验证初始化日志；当前只完成 Phase 1 数据与注册骨架，尚未实现奖励、装备、保存和战斗生效。

### Next
- Phase 2：实现战斗胜利后按来源怪物生成 1 个声骸奖励，并建立最小库存/装备状态。
