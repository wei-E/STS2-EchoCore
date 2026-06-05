# Echo Core 开发日志

- 项目：`mods/EchoCore`
- 最后更新：`2026-05-26`
- 当前阶段：`Phase 5 主动技 MVP`

## 2026-05-27 - 变身主动技规划文档

### Summary
- 输出 `Morph / 变身型主动技` 的独立设计文档，先固定玩法态和视觉态边界。
- 当前结论是把变身主动技拆成两阶段：先做玩法态 MVP，再做视觉替换 MVP。

### Changes
- 新增文档：`E:\Code\sts2mod-dev\美术资源\声骸系统\声骸变身主动技设计.md`
- 文档内容包含：
  - `Morph` 与 `TacticalCard` 的区别
  - `ActiveMorphState`、`MorphDefinition`、`MorphBuffDefinition` 草案
  - `Inklet -> Slippery 1` 的首个样例
  - Buff 回收规则
  - 冷却、持续时间、重复施放限制
  - 视觉替换复杂度评估与分阶段实施建议

### Verification
- 本次为文档规划，不涉及代码和资源导出。

### Next
- 进入 `Morph-1`：只做玩法态 MVP，不做模型替换。
- 首个实现样例使用 `Inklet`，效果为 `Slippery 1`，持续 `2` 回合，冷却 `4` 回合。

## 2026-05-27 - Buff 型主动技基线文档

### Summary
- 根据多人适配复杂度评估，调整主动技扩展方向。
- 当前推荐路线从“视觉化身优先”切换为“Buff 型主动技优先”。

### Changes
- 保留旧文档 `E:\Code\sts2mod-dev\美术资源\声骸系统\声骸变身主动技设计.md` 作为视觉化身参考存档。
- 新增基线文档：`E:\Code\sts2mod-dev\美术资源\声骸系统\声骸Buff型主动技设计.md`
- 新文档明确：
  - 主动技点击后直接施加 Buff
  - Buff 是否限时由具体 `Power` 自己决定，不强制统一回合数
  - 首个样例仍为 `Inklet -> Slippery 1`
  - 当前不做视觉替换

### Verification
- 本次为文档调整，不涉及代码和资源导出。

### Next
- 后续实现按 `声骸Buff型主动技设计.md` 作为优先基线推进。
- `Inklet` MVP 先直接施加原版 `SlipperyPower 1`，不额外叠加统一回合限制。
- 若单机后续需要视觉化身，再回到旧文档继续扩展。

## 2026-05-27 - Inklet Buff 型主动技 MVP

### Summary
- 正式接入第一只 Buff 型主动技声骸：`Inklet / 墨宝`。
- 当前实现不生成卡牌，点击主声骸主动技按钮后直接给玩家施加原版 `SlipperyPower 1`。

### Changes
- `Scripts/Echoes/EchoDefinition.cs`：为声骸定义新增 `BuffSkillId`，支持卡牌主动技和 Buff 主动技并存。
- `Scripts/BuffSkills/BuffSkillModels.cs`：新增 Buff 主动技定义与施加规则模型。
- `Scripts/Registry/EchoRegistry.cs`：新增 Buff 主动技注册与查询。
- `Scripts/Registry/VanillaEchoBootstrap.cs`：
  - 注册 `echo_core:inklet_slippery`
  - 新增 `echo_core:monster_inklet`
  - 将 `Inklet` 设为 `Morph` 形态下的 Buff 型主动技样例
- `Scripts/Services/EchoBuffSkillService.cs`：新增 Buff 型主动技执行器，MVP 先支持 `SLIPPERY -> SlipperyPower`。
- `Scripts/Services/EchoActiveSkillService.cs`：主动技按钮从单一卡牌分支扩展为“卡牌 / Buff”双分支，并保留统一冷却。
- `Scripts/UI/EchoInventoryOverlay.cs`：右侧主动技详情支持显示 Buff 型主动技描述，不再只支持卡牌描述。
- `EchoCore/localization/*/monsters.json`：补充 `Inklet` 声骸和 `Inklet` Buff 主动技中英文文案。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Export：PASS，Godot `--export-pack` 成功重新导出 `EchoCore.pck`。
- Runtime file sync：PASS，已同步最新 `EchoCore.dll` 与 `EchoCore.pck` 到游戏 `mods/EchoCore/`。

### Next
- 进游戏验证 `Inklet` 掉落、装备到槽位 1 后，点击 `声骸技` 是否获得 `Slippery 1`。
- 若链路稳定，再扩第二只 Buff 型主动技样例。

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

## 2026-05-26 - 声骸仓库三栏 UI 首版

### Summary
- 将原来的文本式浮层重构为三栏仓库 UI：左侧装备栏、中间声骸列表、右侧详情面板。
- 接入现有素材 `背景 / 左侧装备栏 / 中间声骸方框`，右侧详情与缺失状态图标先用文字和纯色占位。
- 保留原有库存、装备、调谐、主动技逻辑，交互改为“先选中间声骸，再点左侧槽位装备”。

### Changes
- `Scripts/UI/EchoInventoryOverlay.cs`：重写仓库 UI 布局，新增全屏背景、装备栏槽位头像、网格化声骸列表、详情区、装备/卸下/调谐反馈。
- `echo-core/ui/echoes/layout/inventory_bg.png`：接入仓库背景素材。
- `echo-core/ui/echoes/layout/equipment_sidebar.png`：接入左侧装备栏素材。
- `echo-core/ui/echoes/layout/inventory_card_frame.png`：接入中间声骸卡片框素材。

### Interaction Notes
- 跑图中点击 `声骸` 打开仓库，战斗中仍只显示 `声骸技` 按钮。
- 中间列表点击声骸会刷新右侧详情。
- 左侧槽位点击后，会将当前选中的声骸装备到对应槽位。
- 右侧提供 `卸下` 与 `调谐` 按钮，并显示文本反馈。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Export：PASS，Godot `--export-pack` 成功重新导出 `EchoCore.pck`。
- Runtime file sync：PASS，游戏目录 `mods/EchoCore/EchoCore.pck` 与本地导出文件 SHA256 一致。

### Known Gaps
- 右侧详情中的主动技描述暂用本地化文本清洗后展示，尚未解析出运行时精确数值。
- 主声骸标记、已装备角标、稀有度边框、正式按钮美术仍缺资源，当前用文字与高亮占位。
- 尚未做拖拽装备与分辨率专项微调，当前先按 1080p 主布局适配。

### Next
- 进游戏验证三栏布局在 `1920x1080` 与更低分辨率下是否有遮挡或滚动异常。
- 根据试玩反馈继续补主声骸标识、已装备角标与右侧详情排版。

## 2026-05-26 - 仓库 UI 自适应与右侧详情排版修正

### Summary
- 将左侧装备栏改为按素材原始宽高比随高度缩放，确保始终与背景内容区同高。
- 槽位按钮坐标改为基于素材原图坐标缩放，避免分辨率变化后与底图错位。
- 右侧详情区改成更稳定的单行元信息布局，并加入纵向滚动，修复 `COST` 被挤成竖排的问题。

### Changes
- `Scripts/UI/EchoInventoryOverlay.cs`：新增左栏原始尺寸常量，按 `sidebarScale = contentHeight / SidebarSourceHeight` 计算左栏宽度和槽位坐标。
- `Scripts/UI/EchoInventoryOverlay.cs`：右侧 `COST` 行拆分为 `COST label + 类型 label`，不再依赖可换行文本。
- `Scripts/UI/EchoInventoryOverlay.cs`：详情面板内部新增 `ScrollContainer`，低分辨率下允许详情滚动查看。
- `Scripts/UI/EchoInventoryOverlay.cs`：说明文本改为分行展示 `形态 / 调谐次数 / 状态`，减少横向拥挤。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Export：PASS，Godot `--export-pack` 成功重新导出 `EchoCore.pck`。
- Runtime file sync：PASS，已同步最新 `EchoCore.pck` 到游戏 `mods/EchoCore/`。

### Next
- 进游戏确认左侧装备栏在不同分辨率下是否仍与素材槽位完全对齐。
- 若对齐稳定，下一轮再细修右侧标题字号、段落间距和中间列表的卡片信息密度。

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

## 2026-05-27 - 奖励悬浮持久化文案修正

### Summary
- 修正声骸奖励悬浮提示里仍显示“临时库存 / 暂未持久化”的过期文案。
- 当前持久化实现并未被移除；截图中的问题是提示文本停留在 Phase 2 早期版本，和实际功能状态不一致。

### Changes
- `Scripts/Rewards/EchoReward.cs`
  - 类注释改为“加入 EchoCore 本局库存，并通过持久化服务写入当前 Run modifier”。
  - 奖励悬浮提示改为“库存、装备槽和调谐状态会随当前 Run 存档恢复”。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Runtime file sync：PASS，构建后已自动同步最新 `EchoCore.dll` 到游戏目录。

### Next
- 进游戏确认奖励悬浮提示已更新，不再误导为“未持久化”。
- 如果实际恢复链路仍异常，再抓 `继续游戏` 前后的日志，排查 `EchoPersistenceService` / `EchoRunStateModifier`。

## 2026-05-27 - 持久化空快照与读档 Overlay 报错修复

### Summary
- 通过 `current_run.save` 确认 `MODIFIER.ECHO_RUN_STATE_MODIFIER` 已写入存档，但只有 `id` 没有 `props`，说明自定义 modifier 本体存在，而 `EchoCoreSnapshotJson` 没有进入序列化字段。
- 同时修复读档进入 `NRun` 时 `EchoInventoryOverlay` 使用过高 `ZIndex` 触发的 Godot 报错，避免 UI 初始化在恢复流程附近制造噪音。

### Root Cause
- `current_run.save` 中存在：
  - `id: "MODIFIER.ECHO_RUN_STATE_MODIFIER"`
  - 但缺少 `props`
- 这意味着 `EchoRunStateModifier` 很可能没有稳定进入 `SavedPropertiesTypeCache`，导致 `[SavedProperty] EchoCoreSnapshotJson` 没被序列化。
- 另一个独立问题是 `EchoInventoryOverlay._Ready()` 使用 `ZIndex = 5000`，超过 Godot `CanvasItem` 上限，读档恢复时会在 `NRun._Ready` 链路报错。

### Changes
- `Scripts/Init/Entry.cs`
  - 显式调用 `SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(EchoRunStateModifier))`，不再依赖框架扫描是否命中。
- `Scripts/UI/EchoInventoryOverlay.cs`
  - `ZIndex` 从 `5000` 调整为安全值 `1024`。
- `Scripts/Services/EchoPersistenceService.cs`
  - 新增持久化日志，记录快照字符数、玩家数、调谐待处理数量。
  - 新增 modifier 复用 / 首次挂载日志，便于判断运行时到底有没有把状态挂到当前 run。

### Verification
- Build：PASS，`dotnet build EchoCore.csproj -c Debug -v minimal`，0 warning / 0 error。
- Save inspection：PASS，已确认问题前的 `current_run.save` 中 modifier 存在但无 `props`，本轮修复就是针对这一点。

### Next
- 重新跑一遍：
  1. 获得一个声骸
  2. 装备到槽位
  3. 退主菜单
  4. 继续游戏
- 检查 `godot.log` 中是否出现：
  - `Added EchoRunStateModifier to current run.`
  - `Persisted echo snapshot. chars=...`
  - `Restored echo persistence snapshot. players=...`
- 再检查 `current_run.save` 中 `MODIFIER.ECHO_RUN_STATE_MODIFIER` 下是否出现 `props.strings -> EchoCoreSnapshotJson`

## 2026-05-28 - 项目结构重构方案文档

### Summary
- 在不改动现有 EchoCore 功能代码的前提下，整理出一份后续开发使用的结构重构方案文档。
- 重点约束内容定义、效果实现、运行时服务、UI 文本和本地化资源的职责边界。

### Output
- 新增文档：
  - `E:\Code\sts2mod-dev\美术资源\声骸系统\EchoCore项目结构重构方案.md`

### Key Decisions
- `Echoes/` 目录不按 `Act1/Elite/Boss` 继续细分，直接按声骸名称命名文件。
- 后续新增合鸣时，要求每个合鸣有独立的 effect handler 文件，不再把具体效果写死在 `EchoCombatEffectService`。
- 后续新增特殊声骸时，允许为该声骸建立独立 `EchoEffectHandler`，但不强制每只声骸都建空文件。
- UI 文本拼装后续应迁出 `EchoInventoryOverlay`，交给独立文本服务处理。
- 本地化资源按职责拆分，而不是继续混写。

### Verification
- 本次仅产出文档，无代码变更，无 build/export。

### Next
- 后续真正开始重构时，按文档中的 `Phase A -> Phase B -> Phase C` 顺序推进。
- 第一批推荐落地项：
  1. 拆 `VanillaEchoBootstrap`
  2. 抽 `ISonataEffectHandler`
  3. 抽 `IAffixEffectHandler`
  4. 抽 `EchoUiTextService`

## 2026-05-28 - Phase A 结构重构落地

### Summary
- 开始按重构方案执行 Phase A，把内容定义、效果实现和 UI 文本拼装从原有集中式文件中拆开。
- 本轮目标不是改玩法，而是把后续继续加声骸、合鸣和词条时最容易失控的几个入口先拆稳。

### Changes
- `Scripts/Content/`
  - 新增 `EchoContentConstants.cs`，集中定义内容层常量、默认图标路径和默认主动技冷却规则。
  - 新增 `EchoContentFactory.cs`，集中生成基础词条和原版怪物声骸定义，减少重复构造代码。
  - 新增 `Echoes/*.cs`，把首批声骸定义拆成按名称命名的独立内容文件：
    - `LeafSlimeSEchoContent.cs`
    - `ShrinkerBeetleEchoContent.cs`
    - `NibbitEchoContent.cs`
    - `InkletEchoContent.cs`
    - `ByrdonisEchoContent.cs`
    - `CeremonialBeastEchoContent.cs`
  - 新增 `Sonatas/*.cs`，把合鸣定义拆成：
    - `UniversalResonanceSonataContent.cs`
    - `HiddenLightSonataContent.cs`
  - 新增 `Affixes/*.cs`，把基础词条定义拆成：
    - `StartStrengthAffixContent.cs`
    - `StartDexterityAffixContent.cs`
    - `StartBlockAffixContent.cs`
  - 新增 `Skills/InkletSlipperyBuffSkillContent.cs`，把 Buff 型主动技定义从集中注册入口中拆出。
- `Scripts/Registry/`
  - 新增 `EchoContentBootstrap.cs` 作为新的统一注册入口，负责调用内容定义和效果 handler 注册。
  - 删除 `VanillaEchoBootstrap.cs`，不再让单个文件同时承担声骸、合鸣、词条和主动技的全部注册职责。
  - `EchoRegistry.cs` 新增：
    - `AffixEffectHandlersById`
    - `SonataEffectHandlersById`
    - `RegisterAffixEffectHandler(...)`
    - `RegisterSonataEffectHandler(...)`
    - `TryGetAffixEffectHandler(...)`
    - `TryGetSonataEffectHandler(...)`
  - `Clear()` 现在会同步清空词条 / 合鸣效果 handler 的运行时注册表。
- `Scripts/Effects/`
  - 新增 `Affixes/IAffixEffectHandler.cs` 和 3 个基础词条 handler：
    - `StartStrengthAffixEffectHandler.cs`
    - `StartDexterityAffixEffectHandler.cs`
    - `StartBlockAffixEffectHandler.cs`
  - 新增 `Sonatas/ISonataEffectHandler.cs` 和 2 个合鸣 handler：
    - `UniversalResonanceEffectHandler.cs`
    - `HiddenLightEffectHandler.cs`
  - 这些文件接收原来写死在总服务中的开战生效逻辑，后续新增内容时不再改总调度器。
- `Scripts/Services/EchoCombatEffectService.cs`
  - 改成纯分发器：
    - 统计已装备词条
    - 查表拿词条 handler
    - 统计激活合鸣
    - 查表拿合鸣 handler
  - 删除原来硬编码的：
    - `ApplyStartOfCombatAffix`
    - `ApplyStartOfCombatSonata`
    - `ApplyUniversalStartOfCombatSonata`
    - `ApplyHiddenLightStartOfCombatSonata`
- `Scripts/UI/`
  - 新增 `EchoUiTextService.cs`，承接声骸标题、描述、主动技说明、词条说明、合鸣说明的文本拼装和本地化 fallback。
  - `EchoInventoryOverlay.cs` 不再自己拼装这些文本，改为调用 `EchoUiTextService`。
  - `EchoReward.cs` 的悬浮说明也改为复用同一套 UI 文本服务，避免两处各写一套回退逻辑。
- `Scripts/Init/Entry.cs`
  - 初始化入口改为调用 `EchoContentBootstrap.RegisterAll()`。
- `Scripts/Patches/RestSiteEchoTuningPatches.cs`
  - 调谐入口的默认图标路径改为读取 `EchoContentConstants.DefaultIconPath`，不再依赖已删除的旧 bootstrap。

### Verification
- Residual check：PASS
  - 已确认运行时代码中不再引用 `VanillaEchoBootstrap`。
  - 已确认合鸣和基础词条的开战生效逻辑不再写死在 `EchoCombatEffectService` 中。
- Build：PASS
  - `dotnet build EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`

### Next
- 进游戏做一轮最小回归，重点确认：
  1. 声骸掉落与奖励文案仍正常
  2. 装备面板文本仍正常
  3. 开战词条和合鸣效果仍能生效
- Phase B 再继续处理：
  - 特殊声骸独立 effect handler
  - 主动技统一 skill handler
  - UI 文本与内容定义进一步解耦

## 2026-05-28 - Phase B 结构重构落地（第一轮）

### Summary
- 开始落实 Phase B，把“特殊声骸扩展点”和“主动技统一 handler”从原有公共服务中拆出来。
- 本轮仍以结构迁移为主，不改变当前已验证通过的战斗结果和 UI 结果。

### Dependencies / Paths
- BaseLib 运行时：
  - `E:\Steam\steamapps\common\Slay the Spire 2\mods\BaseLib\BaseLib.dll`
  - `E:\Steam\steamapps\common\Slay the Spire 2\mods\BaseLib\BaseLib.pck`
- STS2 本地源码镜像：
  - `E:\Code\sts2mod-dev\mods\sts2-source-code`
- EchoCore 项目文件：
  - `E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj`

### Changes
- `Scripts/Effects/Echoes/`
  - 新增 `IEchoEffectHandler.cs`
  - 约定只有存在“独立于词条 / 合鸣 / 主动技之外的特殊战斗规则”的声骸，才需要单独实现 handler。
- `Scripts/Effects/Skills/`
  - 新增 `IActiveSkillHandler.cs`
  - 新增 `TacticalCardActiveSkillHandler.cs`
  - 新增 `BuffSkillActiveSkillHandler.cs`
  - 把原来分散在 `EchoActiveSkillService` 和 `EchoUiTextService` 中的主动技形态判断迁移到 handler 内部。
- `Scripts/Registry/EchoRegistry.cs`
  - 新增：
    - `EchoEffectHandlersById`
    - `ActiveSkillHandlersByFormType`
    - `RegisterEchoEffectHandler(...)`
    - `TryGetEchoEffectHandler(...)`
    - `RegisterActiveSkillHandler(...)`
    - `TryGetActiveSkillHandler(...)`
  - `Clear()` 会同步清空这两组新注册表。
- `Scripts/Registry/EchoContentBootstrap.cs`
  - 新增 `RegisterEchoEffectHandlers()` 入口，先把特殊声骸 handler 的注册位置预留出来。
  - 注册统一主动技 handler：
    - `TacticalCardActiveSkillHandler`
    - `BuffSkillActiveSkillHandler`
- `Scripts/Services/EchoActiveSkillService.cs`
  - 不再自己 `switch (FormType)` 执行主动技。
  - 改为通过 `EchoRegistry.TryGetActiveSkillHandler(...)`：
    - 判断主动技是否可用
    - 判断是否占用手牌空间
    - 执行主动技
- `Scripts/UI/EchoUiTextService.cs`
  - 不再自己分 `TacticalCard / Morph` 拼主动技描述。
  - 改为调用 `IActiveSkillHandler.GetSkillSummary(...)`。
- `Scripts/Services/EchoCombatEffectService.cs`
  - 在词条和合鸣分发前，增加特殊声骸 handler 的开战分发入口：
    - `TryGetEchoEffectHandler(definition.Id, out handler)`
  - 当前首批声骸尚未注册独立 handler，所以本轮只是把框架搭好，不改变战斗结果。

### Verification
- Dependency check：PASS
  - 已确认 BaseLib DLL/PCK、STS2 源码镜像和 `EchoCore.csproj` 路径存在。
- Residual check：PASS
  - 主动技形态分支已收口到 `Scripts/Effects/Skills/*.cs`
  - `EchoActiveSkillService` 与 `EchoUiTextService` 不再直接写死 `TacticalCard / Morph` 逻辑
- Build：PASS
  - `dotnet build EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`
- Runtime sync：PASS
  - 构建后已自动同步最新 `EchoCore.dll` 到游戏目录。

### Known Issues
- `RegisterEchoEffectHandlers()` 当前仍为空，说明 Phase B 的“特殊声骸独立 handler”目前只完成了接入框架，尚未迁入首个真实样例。
- 主动技本地化 fallback 仍在各 handler 内部保留了少量重复判断，后续可以再收敛成共享 helper。

### Next
- 选一只最合适的样例声骸，真正接入首个 `IEchoEffectHandler`，验证特殊声骸规则不必再落到公共服务里。
- 继续整理主动技内容层，把卡牌型 / Buff 型主动技定义也进一步从内容和执行层分开。

## 2026-05-28 - Chomper 独立规则声骸样例

### Summary
- 基于 Phase B 刚搭好的 `IEchoEffectHandler` 框架，接入第一只真正使用独立规则的声骸：`Chomper`。
- 本轮目标是验证“某只声骸的固有被动不再写进公共服务，而是独立落文件并由统一分发器调用”。

### Changes
- `Scripts/Content/Echoes/ChomperEchoContent.cs`
  - 新增 `Chomper` 声骸定义。
  - 来源怪物：`CHOMPER`
  - COST：`1`
  - 形态：卡牌型主动技
- `Scripts/Effects/Echoes/ChomperEchoEffectHandler.cs`
  - 新增 `Chomper` 独立规则 handler。
  - 固有规则：战斗开始时获得 `1` 层 `Artifact`
  - 该效果不走随机词条，也不属于合鸣，而是 `Chomper` 自带的固定被动。
- `Scripts/Cards/EchoCoreCardChomper.cs`
  - 新增 `Chomper` 主动技卡。
  - 效果：获得 `1` 层 `Artifact`，并向弃牌堆加入 `1` 张 `Dazed`
  - 目的是保留原怪“硬壳 + Screech 副作用”的身份感
- `Scripts/Cards/EchoSkillCardRegistry.cs`
  - 注册 `EchoCoreCardChomper`
- `Scripts/Registry/EchoContentBootstrap.cs`
  - 注册 `ChomperEchoContent`
  - 在 `RegisterEchoEffectHandlers()` 中注册 `ChomperEchoEffectHandler`
- 本地化：
  - `EchoCore/localization/zhs/monsters.json`
  - `EchoCore/localization/eng/monsters.json`
  - `EchoCore/localization/zhs/cards.json`
  - `EchoCore/localization/eng/cards.json`
  - 新增 `Chomper` 声骸名、描述、主动技卡名与描述

### Verification
- Build：PASS
  - `dotnet build EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`
- Export：PASS
  - `MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`
  - `EchoCore.dll` 已由 build 自动同步

### Notes
- 这次 `Chomper` 是首个真正依赖 `IEchoEffectHandler` 的样例，说明框架已经可以承载“声骸固有被动”而不污染公共调度器。
- 当前独立规则只接到了 `OnCombatStart`，后续如果做“回合开始 / 回合结束 / 受伤后”类专属规则，还要继续扩 handler 生命周期接口。

### Next
- 进游戏验证 `Chomper`：
  1. 击败 `Chomper` 后是否掉落对应声骸
  2. 装备后开战是否固定获得 `Artifact 1`
  3. 战斗中点击主动技是否获得 `Artifact 1` 并向弃牌堆加入 `1` 张 `Dazed`
- 如果验证通过，可以继续做第二只独立规则声骸，例如 `Tunneler`。

## 2026-05-29 - Chomper 官方中文名修正

### Summary
- 校对原版中文本地化后，确认 `CHOMPER.name` 的官方译名是 `啃咬机`，不是此前 EchoCore 中写的 `啃噬花`。
- 本轮仅修正 EchoCore 里的中文显示文本，不改代码逻辑。

### Source
- 原版本地化资源：
  - `E:\Code\sts2mod-dev\mods\Slay the Spire 2-godot-resource\localization\zhs\monsters.json`
  - 对应条目：`"CHOMPER.name": "啃咬机"`

### Changes
- `EchoCore/localization/zhs/monsters.json`
  - `ECHO_CORE_ECHO_CHOMPER.name`：
    - `啃噬花声骸` -> `啃咬机声骸`
  - `ECHO_CORE_ECHO_CHOMPER.description`：
    - `以啃噬花为原型...` -> `以啃咬机为原型...`

### Verification
- Export：PASS
  - `MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏确认 `Chomper` 声骸在奖励界面、库存列表和详情面板中都显示为 `啃咬机声骸`。

## 2026-05-31 - 新增主动技卡牌本地化补齐

### Summary
- 修复新增一批 EchoCore 主动技卡牌在卡牌库中显示 `cards.ECHOCORE-...title/description` 原始 key 的问题。
- 本轮只补本地化资源，不改卡牌逻辑。

### Changes
- `EchoCore/localization/zhs/cards.json`
  - 补齐以下卡牌的中文名称与描述：
    - `Axebot`
    - `BowlbugSilk`
    - `CalcifiedCultist`
    - `FlailKnight`
    - `Flyconid`
    - `FossilStalker`
    - `FrogKnight`
    - `GlobeHead`
  - 同时补齐 `ECHO_CORE_CARD_*` 与 `ECHOCORE-ECHO_CORE_CARD_*` 两套 key，兼容当前卡牌库实际请求的前缀格式。
- `EchoCore/localization/eng/cards.json`
  - 同步补齐英文名称与描述，避免英文环境下出现同类缺失。

### Verification
- JSON：PASS
  - `ConvertFrom-Json` 校验 `zhs/cards.json` 与 `eng/cards.json` 通过。
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`
- Export：PASS
  - `MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏打开卡牌库，确认新增 EchoCore 主动技卡牌不再显示本地化 key。

## 2026-06-01 - 声骸开发者菜单 MVP（Phase 1）

### Summary
- 新增 EchoCore 自己的开发者菜单开关，接入 BaseLib 模组配置菜单。
- 新增独立的声骸开发者按钮与浮窗，不再把调试逻辑塞进 `EchoInventoryOverlay`。
- 第一版只支持选择 `声骸 + 合鸣 + 1 条词条 + 档位 + 等级` 并直接添加到当前玩家库存，不支持直接装备。

### Changes
- 配置：
  - `Scripts/Config/EchoDeveloperConfig.cs`
    - 新增 `EnableEchoDeveloperMenu` 配置项。
  - `Scripts/Init/Entry.cs`
    - 初始化时通过 `ModConfigRegistry.Register("EchoCore", new EchoDeveloperConfig())` 注册配置。
- 服务层：
  - `Scripts/Developer/EchoDeveloperGrantRequest.cs`
    - 新增开发者菜单请求 DTO。
  - `Scripts/Services/EchoDeveloperService.cs`
    - 新增开发者菜单服务层。
    - 负责读取注册表内容、校验声骸与合鸣归属、按指定词条档位创建实例，并写入 `EchoInventory`。
    - 当前通过反射读取 `NRun._state` 来获取当前 `RunState`，这样不需要把调试菜单直接绑死在房间 UI 或战斗 UI 上。
- UI：
  - `Scripts/UI/EchoDeveloperMenu.cs`
    - 新增独立开发者浮窗。
    - MVP 提供 5 个输入项：
      1. 声骸
      2. 合鸣（仅显示该声骸允许的候选合鸣）
      3. 词条
      4. 档位
      5. 等级
    - 提供 `添加到背包` 按钮。
  - `Scripts/UI/EchoDeveloperMenuHost.cs`
    - 新增独立宿主节点。
    - 负责在局内挂一个 `声骸开发` 按钮，并在非战斗场景下根据配置决定显隐。
  - `Scripts/Patches/NRunEchoInventoryOverlayPatch.cs`
    - 在现有 `NRun._Ready` Patch 中额外挂载 `EchoDeveloperMenuHost`。
- 本地化：
  - `EchoCore/localization/zhs/settings_ui.json`
  - `EchoCore/localization/eng/settings_ui.json`
    - 补配置菜单文案：`Enable Echo Developer Menu`

### Verification
- JSON：PASS
  - `EchoCore/localization/zhs/settings_ui.json`
  - `EchoCore/localization/eng/settings_ui.json`
  - `ConvertFrom-Json` 校验通过。
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`
  - `EchoCore.dll` 已由 build 自动同步

### Notes
- 这一版刻意把“入口按钮 / 弹窗 UI / 实例创建逻辑”拆开：
  - `Host` 只管入口显隐
  - `Menu` 只管采集输入
  - `Service` 只管业务校验和创建实例
- 当前版本不支持多词条，也不支持开发菜单里直接装备；这两个都可以在现有结构上继续追加，不需要回头重写 UI 主体。
- 当前版本为避免战斗和联机同步风险，只在非战斗场景显示按钮。

### Next
- 进游戏验证：
  1. 在 BaseLib -> EchoCore 配置里打开 `启用声骸开发者菜单`
  2. 进入局内非战斗场景，确认右上侧出现 `声骸开发` 按钮
  3. 打开菜单，选择任意 `声骸 + 合鸣 + 词条 + 档位`
  4. 点击 `添加到背包` 后，去声骸仓库确认实例已进入库存
- 如果这版稳定，下一步可以补：
  - 多词条输入
  - 直接装备
  - 预设模板

## 2026-06-01 - 开发者菜单移除等级字段

### Summary
- 开发者菜单移除了 `等级` 输入。
- 当前 EchoCore 没有任何等级成长逻辑，保留该输入只会制造误导，因此开发菜单新增实例统一固定为 `Level = 0`。

### Changes
- `Scripts/Developer/EchoDeveloperGrantRequest.cs`
  - 删除 `Level` 字段。
- `Scripts/Services/EchoDeveloperService.cs`
  - 开发者菜单创建实例时固定写入 `Level: 0`。
- `Scripts/UI/EchoDeveloperMenu.cs`
  - 删除 `等级` 输入控件与预览文本中的等级显示。
  - 面板高度同步缩小，避免空白区域过大。

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

## 2026-06-02 - 第一层鸣潮小怪与对应声骸首版接入

### Summary
- 开始把 EchoCore 从“只有声骸掉落和玩家补强”推进到“怪物生态 + 对应声骸闭环”。
- 本轮先接第一层普通怪强度，全部只注入 `Overgrowth`，避免过早进入更高层或精英强度。

### Dependencies / Paths
- BaseLib 运行时：
  - `E:\Steam\steamapps\common\Slay the Spire 2\mods\BaseLib.dll`
- STS2 本地源码镜像：
  - `E:\Code\sts2mod-dev\sts2-source-code`
- Aemeath 参考项目：
  - `E:\Code\sts2mod-dev\mods\aemeath-ww`
- 鸣潮怪物头像来源：
  - `E:\Code\sts2mod-dev\美术资源\ww_monster_icons_nanoka_3.4.3`

### Changes
- 怪物：
  - 新增 `Scripts/Monsters/Wuwa/`
  - 新增共享静态立绘基类 `WuwaStaticMonsterBase`
  - 新增 4 只鸣潮小怪：
    - `WuwaVanguardJunrock`
    - `WuwaElectroPredator`
    - `WuwaSabyrBoar`
    - `WuwaGlacioPrism`
- 遭遇：
  - 新增 `Scripts/Encounters/Wuwa/`
  - 新增 3 组只在 `Overgrowth` 生效的普通遭遇：
    - `WuwaJunrockPairEncounter`
    - `WuwaPredatorAmbushEncounter`
    - `WuwaHuntingPackEncounter`
  - 其中前两组标记为 `Weak`，用于第一层前段出现。
- 声骸与主动技：
  - 新增 4 个对应声骸内容文件：
    - `VanguardJunrockEchoContent`
    - `ElectroPredatorEchoContent`
    - `SabyrBoarEchoContent`
    - `GlacioPrismEchoContent`
  - 新增 4 张对应主动技卡：
    - `EchoCoreCardVanguardJunrock`
    - `EchoCoreCardElectroPredator`
    - `EchoCoreCardSabyrBoar`
    - `EchoCoreCardGlacioPrism`
  - `EchoSkillCardRegistry` 与 `EchoContentBootstrap` 已接入以上新卡和新声骸。
- 资源：
  - 新增共享静态怪物场景 `scenes/creature_visuals/echo_core_wuwa_monster_visuals.tscn`
  - 先接入 4 张占位头像到 `echo-core/ui/monsters/wuwa/`
- 本地化：
  - `EchoCore/localization/zhs|eng/monsters.json`
    - 新增 4 只怪物名称与 move 名
    - 新增 4 个声骸名称与描述
    - 新增 4 条主动技摘要
  - `EchoCore/localization/zhs|eng/cards.json`
    - 新增 4 张主动技卡牌的中英文名称与描述
  - 新增 `EchoCore/localization/zhs|eng/encounters.json`
    - 补 3 组遭遇标题

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`
- JSON：PASS
  - 已用 `ConvertFrom-Json` 校验：
    - `zhs/cards.json`
    - `eng/cards.json`
    - `zhs/monsters.json`
    - `eng/monsters.json`
    - `zhs/encounters.json`
    - `eng/encounters.json`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - `EchoCore.dll` 已由 build 自动同步
  - `EchoCore.pck` 已手动同步到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Known Issues
- 怪物立绘当前仍是静态头像占位，只保证战斗链路可跑；后续可替换为正式怪物图或动画。
- 暂未做局内专项试玩验证，因此当前结论只覆盖编译、导出和资源打包链路。
- `冷凝棱镜` 当前死亡增益逻辑已接入，但还需要进游戏确认死亡事件时机是否完全符合预期。

### Next
- 进游戏优先验证：
  1. 第一层是否能实际刷到这 3 组鸣潮遭遇
  2. 击败后是否会掉落对应 4 只声骸
  3. 怪物名、意图名、遭遇名、本地主动技文案是否都正常显示
  4. `冷凝棱镜` 死亡后是否确实给存活友军加力量
- 若链路稳定，下一轮再做：
  - `碎獠猪 + 冷凝棱镜` 这种更功能化的组合遭遇
  - 正式怪物立绘/动画资源替换
  - 第一层遭遇数量与数值微调

## 2026-06-02 - 鸣潮小怪战斗立绘与声骸图标分流

### Summary
- 把 4 只鸣潮小怪的战斗内立绘切到新的正式去背图。
- 原先使用的缩略头图不再继续充当战斗怪物立绘，改为保留给对应声骸图标使用。

### Resource Source
- 战斗内立绘来源：
  - `E:\Code\sts2mod-dev\美术资源\ww_monster_icons_nanoka_3.4.3\战斗内立绘\游戏文件`
- 缩略头图来源：
  - `E:\Code\sts2mod-dev\美术资源\ww_monster_icons_nanoka_3.4.3`

### Changes
- 新增战斗立绘资源到：
  - `echo-core/ui/monsters/wuwa/vanguard_junrock_battle.png`
  - `echo-core/ui/monsters/wuwa/electro_predator_battle.png`
  - `echo-core/ui/monsters/wuwa/sabyr_boar_battle.png`
  - `echo-core/ui/monsters/wuwa/glacio_prism_battle.png`
- 保留并复制缩略头图到声骸图标目录：
  - `echo-core/ui/echoes/icons/wuwa/*.webp`
- `WuwaVanguardJunrock`
  - `WuwaElectroPredator`
  - `WuwaSabyrBoar`
  - `WuwaGlacioPrism`
  已切换 `TexturePath` 指向新的战斗图，并按新图尺寸微调了 `VisualScale / VisualPosition`。
- `EchoContentFactory.CreateVanillaEcho(...)`
  - 新增可选 `iconPath` 参数，便于内容层为特定声骸显式指定图标。
- 以下 4 个声骸定义已改为使用对应缩略头图：
  - `VanguardJunrockEchoContent`
  - `ElectroPredatorEchoContent`
  - `SabyrBoarEchoContent`
  - `GlacioPrismEchoContent`

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
  - `0 warning / 0 error`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏确认：
  1. 战斗内显示的是新的正式怪物立绘，而不是旧缩略头图
  2. 奖励、仓库、装备栏中的对应声骸显示的是缩略头图
  3. 新立绘缩放和站位是否需要继续微调

## 2026-06-02 - 鸣潮小怪缩放下调与同模板开场意图分流

### Summary
- 把 4 只鸣潮小怪的战斗内立绘整体再缩小一档，减少遮挡和贴地感。
- 修正 `先锋幼岩` 同场时意图过于同步的问题，让相同怪物实例在开场和后续轮转上不再总是完全一致。

### Changes
- 下调以下怪物的 `VisualScale`：
  - `WuwaVanguardJunrock`
  - `WuwaElectroPredator`
  - `WuwaSabyrBoar`
  - `WuwaGlacioPrism`
- `WuwaVanguardJunrock`
  - 新增 `OpenWithListen` 开场偏好字段，允许遭遇层为不同实例指定不同首轮倾向。
  - 在 `GenerateMoveStateMachine()` 中补入额外随机分支，避免稳定落入单一路线。
- `WuwaJunrockPairEncounter`
  - 双 `先锋幼岩` 遭遇中，前后两只怪的开场偏好现在被显式拆开，不再默认镜像同步。

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏重点确认：
  1. 4 只怪当前缩放是否合适，是否还需要继续缩小
  2. `2x 先锋幼岩` 开场是否已出现不同意图
  3. `先锋幼岩 + 其他怪` 的中后期轮转是否已经足够自然

## 2026-06-02 - 鸣潮遭遇自定义槽位与紧凑站位

### Summary
- 修正鸣潮小怪遭遇默认站位过散的问题。
- 为 3 组第一层遭遇补上自定义 `Slots` 和遭遇场景，让怪群整体更靠右、更紧凑，不再压进玩家区域。

### Changes
- `WuwaJunrockPairEncounter`
  - 启用 `HasScene`
  - 新增 `front / back` 槽位
  - 两只 `先锋幼岩` 改为显式绑定槽位
- `WuwaPredatorAmbushEncounter`
  - 启用 `HasScene`
  - 新增 `front / back` 槽位
  - `先锋幼岩` 与 `惊蛰猎手` 改为显式绑定槽位
- `WuwaHuntingPackEncounter`
  - 启用 `HasScene`
  - 新增 `left / middle / right` 槽位
  - 三怪小队改为显式绑定槽位
- 新增遭遇场景：
  - `scenes/encounters/echo_core_encounter_junrock_pair.tscn`
  - `scenes/encounters/echo_core_encounter_predator_ambush.tscn`
  - `scenes/encounters/echo_core_encounter_hunting_pack.tscn`

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏确认：
  1. 左侧怪物是否已经不再进入玩家区域
  2. 三怪遭遇的横向间距是否还需要继续压缩
  3. 后排怪的纵深是否要再明显一点

## 2026-06-02 - 修正鸣潮遭遇自定义场景加载失败

### Summary
- 修复了“进入鸣潮遭遇后怪物完全不可见”的问题。
- 根因不是怪物没生成，而是遭遇自定义场景没有按 `CustomEncounterModel` 的正确方式挂载，导致战斗房间创建槽位场景时直接加载失败。

### Root Cause
- 上一版只给 3 个遭遇设置了 `HasScene = true`，但没有提供 `CustomScenePath`。
- `CustomEncounterModel` 的正确接法是显式覆写 `CustomScenePath`，由 BaseLib 的 `EncounterModel.ScenePath` Patch 重定向到自定义场景。
- 因为没有走这条路径，游戏仍尝试按默认规则查找场景，最终在战斗开始时抛出资源加载错误。

### Changes
- 为以下遭遇补上正式 `CustomScenePath`：
  - `WuwaJunrockPairEncounter`
  - `WuwaPredatorAmbushEncounter`
  - `WuwaHuntingPackEncounter`
- 移除手动硬写的 `HasScene = true` 覆写，改为依赖 `CustomEncounterModel` 的默认逻辑自动判断。

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 重新进游戏确认：
  1. 3 组鸣潮遭遇是否恢复正常显示
  2. 自定义站位是否已经生效
  3. 如果显示恢复，再继续微调坐标密度

## 2026-06-02 - 立绘回调放大、惊蛰猎手降伤、遭遇移出弱怪池

### Summary
- 把 4 只鸣潮小怪的战斗立绘回调放大一点，避免看起来过小。
- 下调 `惊蛰猎手` 在挂上易伤后的处决技伤害，减少第一层压迫感。
- 将 3 组鸣潮遭遇全部移出第一层弱怪池，避免开局前几战过早遇到影响体验。

### Changes
- 立绘缩放上调：
  - `WuwaVanguardJunrock`：`0.52 -> 0.56`
  - `WuwaElectroPredator`：`0.50 -> 0.54`
  - `WuwaSabyrBoar`：`0.50 -> 0.54`
  - `WuwaGlacioPrism`：`0.48 -> 0.52`
- `WuwaElectroPredator`
  - `SPRING_THRUST` 伤害下调：
    - 基础：`14 -> 12`
    - 致命敌人：`15 -> 13`
  - 保留 `2 Vulnerable` 机制，不改其“标记后重击”的身份。
- 遭遇池调整：
  - `WuwaJunrockPairEncounter.IsWeak = false`
  - `WuwaPredatorAmbushEncounter.IsWeak = false`
  - `WuwaHuntingPackEncounter.IsWeak = false`

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏确认：
  1. 怪物当前尺寸是否合适
  2. `惊蛰猎手` 在第一层的处决压力是否已合理
  3. 鸣潮遭遇是否已不再出现在第一层前几场弱怪战中

## 2026-06-03 - 新增灵魂异鱼与地道虫声骸

### Summary
- 新增 `灵魂异鱼声骸`，主动技改为直接施加 `1` 层灵体，并向抽牌堆洗入 `2` 张原版 `Beckon`。
- 新增 `地道虫声骸`，主动技改为“获得 `12` 点格挡，本回合不能打出攻击牌，下回合开始时对随机敌人造成 `20` 点伤害”。
- 两只新声骸都接入了 Buff 型主动技的独立摘要文案，不再依赖通用说明回退。

### Changes
- 新增 Buff 技能定义：
  - `SoulFyshBuffSkillContent`
  - `TunnelerBuffSkillContent`
- 新增声骸定义：
  - `SoulFyshEchoContent`
  - `TunnelerEchoContent`
- 新增自定义 Power：
  - `TunnelerBurrowPower`
- 扩展 `EchoBuffSkillService`：
  - 支持 `INTANGIBLE`
  - 支持 `GAIN_BLOCK`
  - 支持 `ADD_BECKON_TO_DRAW`
  - 支持 `TUNNELER_BURROW_POWER`
- 更新独立主动技摘要映射：
  - `ECHO_CORE_SKILL_SOUL_FYSH`
  - `ECHO_CORE_SKILL_TUNNELER`
- 更新中英文本地化：
  - 新增两只声骸的名称与描述
  - 新增两组 Buff 技能文案
  - 新增两组按钮摘要文案

### Dependencies / Paths
- 项目：
  - `E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj`
- 原版源码参考：
  - `E:\Code\sts2mod-dev\mods\sts2-source-code\MegaCrit.Sts2.Core.Models.Monsters\SoulFysh.cs`
  - `E:\Code\sts2mod-dev\mods\sts2-source-code\MegaCrit.Sts2.Core.Models.Cards\Beckon.cs`
  - `E:\Code\sts2mod-dev\mods\sts2-source-code\MegaCrit.Sts2.Core.Models.Monsters\Tunneler.cs`
  - `E:\Code\sts2mod-dev\mods\sts2-source-code\MegaCrit.Sts2.Core.Models.Powers\ChainsOfBindingPower.cs`
- 导出工具：
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe`
- 运行时同步目录：
  - `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\`

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Localization JSON：PASS
  - `EchoCore/localization/zhs/monsters.json`
  - `EchoCore/localization/eng/monsters.json`
- Export：PASS
  - `E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Known Issues
- 本轮只做了编译、打包和资源同步，尚未做实机战斗验证。
- `灵魂异鱼` 当前摘要文案里直接沿用原版卡名 `Beckon`，若后续需要完整中文化，可再补一层显示替换。

### Next
- 进游戏确认：
  1. `灵魂异鱼声骸` 点击后是否正确获得 `1` 层灵体，并向抽牌堆加入 `2` 张 `Beckon`
  2. `地道虫声骸` 点击后本回合是否确实无法打出攻击牌
  3. `地道虫声骸` 是否会在下个我方回合开始时，对随机敌人正确造成 `20` 点伤害
  4. 两只声骸在库存界面与战斗按钮上的独立文案是否都正常显示

## 2026-06-03 - 隐世回光 5件确认与全声骸扩展

### Summary
- 确认 `隐世回光 5件` 当前实现已经是“本场战斗只生效一次”。
- 将 `隐世回光` 追加到所有 EchoCore 已注册声骸的候选合鸣池中。

### Changes
- 检查 `HiddenLightRevivePower`：
  - 触发致命伤保命后会立即 `PowerCmd.Remove(this)`
  - 因此同一场战斗不会再次触发
- 更新 `EchoContentBootstrap.RegisterAll()`：
  - 在声骸注册后统一执行默认合鸣扩展
  - 把 `HiddenLightSonataId` 追加到所有已注册声骸
- 新增注册辅助：
  - `RegisterDefaultSonataAssignments()`

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Runtime DLL sync：PASS
  - `EchoCore.dll` 已通过 build 自动同步到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\`

### Known Issues
- 本轮未重新导出 `EchoCore.pck`，因为没有资源或本地化变更，仅涉及 C# 注册逻辑。
- 仍需实机确认所有声骸在库存 / 掉落 / 开发菜单中都能看到 `隐世回光` 候选项。

### Next
- 进游戏确认：
  1. 任意声骸现在是否都能调谐到 `隐世回光`
  2. `隐世回光 5件` 在同一场战斗里是否仍然只会触发一次
  3. 新增全局候选后，奖励掉落和开发菜单的合鸣下拉是否都正常

## 2026-06-02 - 弱怪池补充 4 个鸣潮单怪遭遇

### Summary
- 为当前已实现的 4 只鸣潮小怪各补了一个单怪弱遭遇。
- 现在第一层弱怪池负责“单怪认识战”，而原有 3 组鸣潮组合遭遇继续留在强怪池。

### Changes
- 新增弱怪遭遇：
  - `WuwaSoloJunrockEncounter`
  - `WuwaSoloElectroPredatorEncounter`
  - `WuwaSoloSabyrBoarEncounter`
  - `WuwaSoloGlacioPrismEncounter`
- 设计约束：
  - 全部 `RoomType.Monster`
  - 全部 `IsWeak = true`
  - 全部只生成 1 只怪
  - 第一版不加自定义遭遇场景，直接使用默认单怪站位
- 新增本地化标题：
  - `ECHO_CORE_ENCOUNTER_SOLO_JUNROCK`
  - `ECHO_CORE_ENCOUNTER_SOLO_ELECTRO_PREDATOR`
  - `ECHO_CORE_ENCOUNTER_SOLO_SABYR_BOAR`
  - `ECHO_CORE_ENCOUNTER_SOLO_GLACIO_PRISM`

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏确认：
  1. 第一层前几战是否会刷到这 4 个单怪遭遇
  2. 原有 `幼岩群 / 猎手伏击 / 狩猎队` 是否仍然只在强怪池出现
  3. 单怪默认站位是否已经足够自然

## 2026-06-02 - 立绘继续放大并放松组合站位

### Summary
- 应用户反馈，把 4 只鸣潮小怪的立绘继续放大一档。
- 调整组合遭遇槽位：不再贴得过紧，整体向右平移，并拉开横向间距。

### Changes
- 立绘缩放上调：
  - `WuwaVanguardJunrock`：`0.56 -> 0.60`
  - `WuwaElectroPredator`：`0.54 -> 0.58`
  - `WuwaSabyrBoar`：`0.54 -> 0.58`
  - `WuwaGlacioPrism`：`0.52 -> 0.56`
- 遭遇场景站位调整：
  - `echo_core_encounter_hunting_pack.tscn`
  - `echo_core_encounter_junrock_pair.tscn`
  - `echo_core_encounter_predator_ambush.tscn`
- 调整方向：
  - 所有组合槽位整体更靠右
  - 横向间距扩大，减少“贴脸挤团”观感

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏确认：
  1. 4 只怪当前尺寸是否已经到位
  2. 三怪组合是否已经不再过挤
  3. 整体右移后是否还需要再往右一点

## 2026-06-02 - 狩猎队移除惊蛰猎手

### Summary
- 按用户反馈，把 `狩猎队` 遭遇中的 `惊蛰猎手` 移除。
- 该遭遇现在改为 `先锋幼岩 + 碎獠猪 + 冷凝棱镜`，保留三怪结构，但去掉易伤处决压力点。

### Changes
- `WuwaHuntingPackEncounter`
  - `AllPossibleMonsters` 中移除 `WuwaElectroPredator`
  - `GenerateMonsters()` 中右侧单位改为 `WuwaGlacioPrism`

### Verification
- Build：PASS
  - `dotnet build E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.csproj -c Debug -v minimal`
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`

### Next
- 进游戏确认：
  1. `狩猎队` 是否已不再出现 `惊蛰猎手`
  2. 新版三怪压力是否已明显下降

## 2026-06-04 - 双冷凝棱镜弱遭遇

### Summary
- 将 `WuwaSoloGlacioPrismEncounter` 从单棱镜改为双棱镜，提升弱怪池中的该遭遇强度。
- 同步把遭遇标题调整为复数语义，避免文案和实际战斗内容不一致。

### Changes
- `WuwaSoloGlacioPrismEncounter`
  - `GenerateMonsters()` 由 `1x 冷凝棱镜` 改为 `2x 冷凝棱镜`
- 本地化：
  - `zhs`: `冷凝棱镜 -> 冷凝棱镜群`
  - `eng`: `Glacio Prism -> Glacio Prism Pair`

### Verification
- Export：PASS
  - `E:\Code\sts2mod-dev\GodotSharp\MegaDot_v4.5.1-stable_mono_win64_console.exe --headless --path E:\Code\sts2mod-dev\mods\EchoCore --export-pack "Windows Desktop" E:\Code\sts2mod-dev\mods\EchoCore\EchoCore.pck`
- Runtime file sync：PASS
  - 已同步最新 `EchoCore.pck` 到 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.pck`
- Build：
  - 代码编译产物已生成，但 `dotnet build` 最后同步 `EchoCore.dll` 到游戏目录时失败
  - 原因：`SlayTheSpire2.exe` 正在占用 `E:\Steam\steamapps\common\Slay the Spire 2\mods\EchoCore\EchoCore.dll`

### Next
- 关闭游戏后再跑一轮完整 build，可恢复 DLL 同步验证链路。
- 进游戏确认 `冷凝棱镜群` 是否已按双怪出现。

## 2026-06-05 - 鸣潮二层怪开发文档

### Summary
- 新增一份二层鸣潮怪开发规划文档，明确首批普通怪、二层精英、推荐机制、实现边界与开发顺序。
- 这轮只做规划沉淀，不改动实际战斗逻辑。

### Changes
- 新增文档：
  - `Docs/WuwaFloor2MonsterPlan.md`
- 文档内容覆盖：
  - 首批二层普通怪：`裂变幼岩`、`遁地鼠`、`巡徊猎手`、`绿熔蜥（稚形）`
  - 二层精英：`蚀脊龙`
  - `裂变幼岩` 的死亡分裂方案
  - `蚀脊龙` 的 `震慑` Power / 卡牌状态方案
  - 推荐遭遇分层与实现顺序

### Dependencies / Paths
- 项目：
  - `E:\Code\sts2mod-dev\mods\EchoCore`
- 原版源码参考：
  - `E:\Code\sts2mod-dev\sts2-source-code\MegaCrit.Sts2.Core.Models.Monsters\MagiKnight.cs`
  - `E:\Code\sts2mod-dev\sts2-source-code\MegaCrit.Sts2.Core.Models.Monsters\FrogKnight.cs`
  - `E:\Code\sts2mod-dev\sts2-source-code\MegaCrit.Sts2.Core.Models.Monsters\SoulFysh.cs`
  - `E:\Code\sts2mod-dev\sts2-source-code\MegaCrit.Sts2.Core.Models.Monsters\Tunneler.cs`
- 机制与怪物资料：
  - `E:\Code\sts2mod-dev\美术资源\ww_monster_icons_nanoka_3.4.3\monster_mechanics_reference.md`
  - `E:\Code\sts2mod-dev\美术资源\ww_monster_icons_nanoka_3.4.3\monster_echo_reference.md`

### Verification
- 文档检查：PASS
  - `Docs/WuwaFloor2MonsterPlan.md` 已创建
- Build / Export：
  - 本轮未执行
  - 原因：仅新增开发文档，无代码或资源改动

### Known Issues
- 文档中的数值和遭遇分层仍属于第一版建议值，后续需要实机测试后微调。
- `蚀脊龙` 的 `震慑` 实现仍是当前批次里技术风险最高的部分，尤其是抽牌事件挂接与卡牌状态显示。

### Next
- 按文档顺序优先实现：
  1. `遁地鼠`
  2. `巡徊猎手`
  3. `绿熔蜥（稚形）`
  4. `裂变幼岩`
  5. `蚀脊龙`
