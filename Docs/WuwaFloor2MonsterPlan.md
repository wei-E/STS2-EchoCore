# 鸣潮二层怪开发规划

- 日期：2026-06-05
- 范围：EchoCore 二层普通怪、二层精英、配套自定义 Power / 卡牌状态
- 目标仓库：
  - EchoCore：`E:\Code\sts2mod-dev\mods\EchoCore`
  - 原版源码参考：`E:\Code\sts2mod-dev\sts2-source-code`
  - 美术与机制参考：`E:\Code\sts2mod-dev\美术资源\ww_monster_icons_nanoka_3.4.3`

## 目标

在现有一层鸣潮小怪基础上，补一批更适合二层的机制怪。

设计目标：

- 保留鸣潮怪物辨识度，不做单纯加数值换皮。
- 二层普通怪至少各自带一个“必须读”的机制点。
- 第一批普通怪机制控制在中复杂度，避免同时引入过多新系统。
- 让 `蚀脊龙` 承担第一只真正的二层精英 showcase。

## 当前已实现的一层生态

目前 EchoCore 已有 4 只基础怪：

- `先锋幼岩`
- `惊蛰猎手`
- `碎獠猪`
- `冷凝棱镜`

它们更偏一层的基础读法：

- 前排撞脸
- 后排易伤压制
- 简单进攻
- 死亡增益

二层则需要开始加入：

- 击杀顺序判断
- 死亡替换 / 分裂
- 潜地与延迟爆发
- 蓄势成长
- 对玩家手牌区施压

## 推荐首批二层怪名单

普通怪：

- `裂变幼岩`
- `遁地鼠`
- `巡徊猎手`
- `绿熔蜥（稚形）`

精英：

- `蚀脊龙`

## 总体分层原则

普通怪：

- 每只怪 `2-4` 个 Move
- 最多只引入 `1` 个专属自定义 Power
- 组合恶心程度高于单体恶心程度

精英怪：

- 可以引入玩家侧 Debuff Power
- 可以引入卡牌状态标记
- 可以出现更明显的阶段切换或半血强化

## 一、裂变幼岩

建议类名：

- `WuwaFissionJunrock`

建议 `CustomID`：

- `ECHO_CORE_MONSTER_FISSION_JUNROCK`

定位：

- 二层普通怪 / 强怪池
- 死亡替换怪
- 负责把“清掉一只前排”变成“战斗并没有立刻结束”

### 核心机制

本体死亡时不会直接结束，而是分裂为两只小体。

推荐结构：

- 本体 `裂变幼岩` 死亡时：
  - 若没有 `FissionSplitPower`，则移除自身并生成 `2` 只 `幼裂变幼岩`
  - 生成的小体最大生命为本体的 `50%`
- 小体 `幼裂变幼岩` 死亡时：
  - 对玩家造成其 `MaxHp 20%` 的可格挡伤害
- 小体不可再次分裂

不建议第一版写成：

- 同一次死亡同时“分裂 + 对玩家造成爆炸伤害”

原因：

- 信息量太大，读感不清楚
- 普通池里会抢走其他怪的存在感
- 数值很容易超标

### 推荐实现方案

建议拆为两只模型：

- `WuwaFissionJunrock`
- `WuwaFissionJunrockShard`

并引入 1 个自定义 Power：

- `FissionSplitPower`
  - 作用：标记“是否还允许分裂”
  - 本体携带：`CanSplit = true`
  - 分裂出来的小体携带：`CanSplit = false`

另外可选引入 1 个轻量死亡伤害 Power：

- `FissionBurstOnDeathPower`
  - 作用：死亡时对玩家造成按自身最大生命计算的可格挡伤害
  - 只给小体用

如果不想一开始就做死亡 Power，也可以把伤害逻辑直接写在小体 `OnDeath` 里。

### Move 建议

本体：

- `JAGGED_SLAM`
  - Intent：`SingleAttackIntent`
  - 效果：造成中等伤害
- `UNSTABLE_HUM`
  - Intent：`BuffIntent`
  - 效果：获得少量力量或格挡
- `CRACKED_RUSH`
  - Intent：`MultiAttackIntent`
  - 效果：较低多段伤害

小体：

- `SHARD_PECK`
  - Intent：`SingleAttackIntent`
  - 效果：低伤害
- `SHARD_SCRAPE`
  - Intent：`MultiAttackIntent`
  - 效果：低多段伤害

### 数值方向

本体：

- HP：`58-66`
- 普攻：`11`
- 多段：`5x2`

小体：

- HP：本体 `50%`
- 普攻：`6`
- 多段：`3x2`
- 死亡伤害：`自身 MaxHp 的 20%`

### 推荐遭遇

- `裂变幼岩 + 先锋幼岩`
- `裂变幼岩 + 冷凝棱镜`
- `绿熔蜥（稚形） + 裂变幼岩`

## 二、遁地鼠

建议类名：

- `WuwaExcarat`

建议 `CustomID`：

- `ECHO_CORE_MONSTER_EXCARAT`

定位：

- 二层普通怪 / 弱怪池与强怪池都可放
- 群体型潜地骚扰怪
- 负责制造“轮流潜地、轮流突袭”的节奏压力

### 核心机制

以原版 `Tunneler` 为原型，但差异不放在单体爆发，而放在群体协同：

- 血量更低
- 单次伤害更低
- 更适合 `2-3` 只同时出现
- 同场多只时尽量不要同步钻地

不建议做成“钻更久”：

- 拖回合感会更重
- 玩家主观上更像卡流程，而不是有互动

更推荐做成：

- 一只先咬
- 一只先钻
- 下一轮交错突袭

### 推荐实现方案

建议直接参考：

- `Tunneler.cs`

第一版尽量复用原版逻辑：

- `BurrowedPower`
- `GainBlock`
- 潜地后下一回合攻击

差异化手段：

- 数值比 `Tunneler` 低
- 遭遇编排强制错峰
- 单体 Move 更短更频繁

### Move 建议

- `NIP_MOVE`
  - Intent：`SingleAttackIntent`
  - 效果：低伤害
- `BURROW_MOVE`
  - Intent：`BuffIntent + DefendIntent`
  - 效果：获得中量格挡并进入潜地
- `AMBUSH_MOVE`
  - Intent：`SingleAttackIntent`
  - 效果：中等伤害

### 数值方向

- HP：`34-40`
- 啃咬：`8`
- 潜袭：`13`
- 钻地格挡：`16-20`

### 遭遇侧特殊约束

为避免多只完全同步，建议增加遭遇级开场参数：

- 左侧遁地鼠：首轮偏 `NIP_MOVE`
- 右侧遁地鼠：首轮偏 `BURROW_MOVE`

如果后续做三只版本：

- 只允许其中一只首轮直接潜地

### 推荐遭遇

- `2x 遁地鼠`
- `遁地鼠 + 先锋幼岩`
- `2x 遁地鼠 + 冷凝棱镜`

## 三、巡徊猎手

建议类名：

- `WuwaAeroPredator`

建议 `CustomID`：

- `ECHO_CORE_MONSTER_AERO_PREDATOR`

定位：

- 二层后排骚扰怪
- 与 `惊蛰猎手` 区分开，不走“易伤处决”，而走“回旋骚扰”

### 核心机制

将鸣潮里的回旋镖手感转译为：

- 普攻
- 施压 debuff
- 延迟回旋追击

它的威胁不在一发大伤，而在于：

- 多段
- 连续回合持续施压
- 和前排怪搭配后很烦

### 推荐实现方案

不建议新建复杂 Power。

第一版用状态机就够：

- `风旋投掷`
- `割风压制`
- `回旋追猎`

如果想强化“延迟回旋”的识别度，可以做一个非常轻量的自 Buff：

- `ReturningBoomerangPower`
  - 作用：下一回合优先使用回旋攻击

但第一版完全可以只靠状态机实现，不强依赖新 Power。

### Move 建议

- `GALE_THROW`
  - Intent：`SingleAttackIntent`
  - 效果：造成中等伤害
- `CUTTING_GUST`
  - Intent：`DebuffIntent`
  - 效果：施加 `1 Weak` 或 `1 Frail`
- `RETURNING_HUNT`
  - Intent：`MultiAttackIntent`
  - 效果：`5x3` 或 `6x2`
  - 限制：优先在 `CUTTING_GUST` 后使用

### 数值方向

- HP：`48-56`
- 普攻：`10`
- 回旋：`5x3`
- Debuff：`1 Weak` 或 `1 Frail`

### 推荐遭遇

- `巡徊猎手 + 碎獠猪`
- `巡徊猎手 + 先锋幼岩`
- `巡徊猎手 + 裂变幼岩`

## 四、绿熔蜥（稚形）

建议类名：

- `WuwaBabyViridblazeSaurian`

建议 `CustomID`：

- `ECHO_CORE_MONSTER_BABY_VIRIDBLAZE_SAURIAN`

定位：

- 二层成长型前排
- 拖回合越久威胁越高

### 核心机制

使用一个轻量成长机制：

- 每次行动后获得 `1 Heat`
- 当 `Heat >= 3` 时，下一个攻击获得强化并清空 `Heat`

这是最适合二层的“看得懂但不能放着不管”的成长怪。

### 推荐实现方案

引入 1 个自定义 Power：

- `SaurianHeatPower`
  - 记录当前层数
  - 层数到阈值后，为下一次攻击提供强化判定
  - 结算强化攻击后清零

如果想进一步压低实现复杂度，也可以先不做可视化层数特效，只做数值和图标。

### Move 建议

- `SCORCH_BITE`
  - Intent：`SingleAttackIntent`
  - 效果：基础攻击
- `HARDENED_HIDE`
  - Intent：`DefendIntent + BuffIntent`
  - 效果：获得格挡，并获得 `1 Heat`
- `HEATWAVE_POUNCE`
  - Intent：`SingleAttackIntent`
  - 效果：
    - 常态：中等伤害
    - `Heat >= 3`：高伤害，并施加 `1 Vulnerable` 或 `1 Frail`

### 数值方向

- HP：`60-68`
- 基础攻击：`11`
- 强化攻击：`18`
- 格挡：`10`

### 推荐遭遇

- `绿熔蜥（稚形） + 裂变幼岩`
- `绿熔蜥（稚形） + 先锋幼岩`
- `绿熔蜥（稚形） + 巡徊猎手`

## 五、蚀脊龙

建议类名：

- `WuwaDreadmane`

建议 `CustomID`：

- `ECHO_CORE_ELITE_DREADMANE`

定位：

- 二层精英
- 高血量
- 半血后更凶
- 通过玩家手牌区施压来制造“这场战斗不一样”的感觉

### 核心机制

核心卖点不是单纯高伤，而是 `震慑`：

- 玩家抽牌后，随机 `2` 张牌获得 `震慑`
- `震慑` 效果：
  - 费用 `+1`
  - 获得 `保留`
  - 打出后：所有敌人获得 `1 Strength`

同时，`蚀脊龙` 在半血以下进入 `暴躁态`：

- 攻击提高
- 更倾向重击或多段
- `震慑` 压力变强

### 为什么适合做精英

因为它会同时对两个层面施压：

- 血量和伤害层面的常规战斗压力
- 手牌与资源规划层面的特殊机制压力

这已经超出普通怪范畴，更适合作为二层精英的辨识点。

### 推荐实现方案

建议拆成两个系统：

玩家侧 Power：

- `StaggerOnDrawPower`
  - 每回合或每次抽牌事件中，给随机手牌附加 `震慑`
  - 为防止过强，建议限制：
    - 每回合最多触发一次
    - 或者只处理本次新抽到的牌

卡牌侧 Modifier / 状态：

- `EchoCoreStaggeredCardModifier`
  - 费用 `+1`
  - 获得 `Retain`
  - 打出后触发“所有敌人获得 1 Strength”
  - 视觉特效可复用原版女王的卡牌封印特效

怪物自身可选再配一个阶段 Power：

- `DreadmaneEnragedPower`
  - 作用：标记半血以下已进入暴躁态
  - 防止重复切阶段

### Move 建议

- `RAVAGE_BITE`
  - Intent：`SingleAttackIntent`
  - 效果：稳定高伤
- `HOWL_OF_DREAD`
  - Intent：`BuffIntent + DebuffIntent`
  - 效果：给予玩家 `StaggerOnDrawPower`
- `FRENZIED_TEAR`
  - Intent：`MultiAttackIntent`
  - 效果：多段攻击
- `BLOODSCENT_RUSH`
  - Intent：`SingleAttackIntent`
  - 效果：半血以下优先使用的高伤冲锋

### 阶段切换建议

- 半血以上：
  - `RAVAGE_BITE`
  - `HOWL_OF_DREAD`
  - `FRENZIED_TEAR`
- 半血以下：
  - 获得 `DreadmaneEnragedPower`
  - 下一轮优先 `BLOODSCENT_RUSH`
  - 后续提升攻击 Move 权重

### 数值方向

- HP：`130-145`
- 普攻：`16`
- 多段：`7x3`
- 冲锋：`24`

### 实现风险

这只是本批里复杂度最高的一只。

主要风险不在怪物状态机，而在：

- 抽牌事件挂接
- 卡牌状态持久化与显示
- 费用调整和保留共存时的兼容性
- 打出后给敌方全体加力量的结算时机

因此不建议和普通怪并行开工。

## 推荐实现顺序

1. `遁地鼠`
2. `巡徊猎手`
3. `绿熔蜥（稚形）`
4. `裂变幼岩`
5. `蚀脊龙`

理由：

- `遁地鼠` 最接近原版成熟模板，最快起量
- `巡徊猎手` 和 `绿熔蜥` 都属于中复杂度，不依赖太多新系统
- `裂变幼岩` 需要处理死亡替换 / 召唤链路
- `蚀脊龙` 涉及玩家手牌状态，单独作为一个阶段做最稳

## 推荐遭遇分层

二层弱怪池：

- `2x 遁地鼠`
- `巡徊猎手`
- `绿熔蜥（稚形）`

二层强怪池：

- `遁地鼠 + 先锋幼岩`
- `巡徊猎手 + 碎獠猪`
- `绿熔蜥（稚形） + 裂变幼岩`
- `裂变幼岩 + 冷凝棱镜`
- `巡徊猎手 + 裂变幼岩 + 先锋幼岩`

二层精英池：

- `蚀脊龙`

第一版不建议：

- `蚀脊龙 + 小怪`

先把精英主机制跑顺，再考虑带从属单位版本。

## 代码结构建议

普通怪：

- `Scripts/Monsters/Wuwa/WuwaFissionJunrock.cs`
- `Scripts/Monsters/Wuwa/WuwaFissionJunrockShard.cs`
- `Scripts/Monsters/Wuwa/WuwaExcarat.cs`
- `Scripts/Monsters/Wuwa/WuwaAeroPredator.cs`
- `Scripts/Monsters/Wuwa/WuwaBabyViridblazeSaurian.cs`

精英：

- `Scripts/Monsters/Wuwa/WuwaDreadmane.cs`

Power：

- `Scripts/Powers/FissionSplitPower.cs`
- `Scripts/Powers/SaurianHeatPower.cs`
- `Scripts/Powers/StaggerOnDrawPower.cs`
- `Scripts/Powers/DreadmaneEnragedPower.cs`

卡牌状态：

- `Scripts/Cards/Modifiers/EchoCoreStaggeredCardModifier.cs`

遭遇：

- `Scripts/Encounters/Wuwa/Act2/`

推荐按二层单独分目录，避免和当前一层文件混在一起。

## 实现边界

第一版明确不做：

- 元素抗性系统
- 复杂部位破坏
- 真正的即时弹反
- 多阶段精英带召唤物
- 裂变幼岩“无限分裂”

第一版成功标准：

- 二层怪一眼能看出和一层怪不同
- 至少有两只普通怪会迫使玩家改变击杀顺序
- `蚀脊龙` 的 `震慑` 能正常作用于手牌且不会导致规则错乱

## 推荐下一步

1. 先实现 `遁地鼠`
2. 同时补 2 组二层弱 / 强遭遇，尽快进游戏验证节奏
3. 再做 `巡徊猎手`
4. `绿熔蜥（稚形）`
5. `裂变幼岩`
6. 最后单开 `蚀脊龙`

这样可以先把二层普通怪生态立住，再处理真正重机制的精英。
