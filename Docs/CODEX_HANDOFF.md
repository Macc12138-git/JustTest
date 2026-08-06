# JustTest Codex 工作交接

更新时间：2026-08-06  
项目路径：`/Users/user/Desktop/JustTest/JustTest`  
Unity版本：`2022.3.34f1`  

## 1. 交接目标

本文件用于让新的 Codex 在不重新梳理整个项目的情况下接手当前工作。

当前项目是一款以《死亡细胞》式高速横版战斗为参考的 2D Roguelike 战斗原型。现阶段已经完成移动、翻滚、三武器、技能、QTE、敌人、战斗平台、波次、对象池、HUD、结算和战斗反馈等核心闭环。

当前优先级已经从继续增加战斗节奏内容，调整为：

```text
优化角色表现
→ 引入美少女角色形象
→ 使用 PSD 美术源文件和 Unity 2D 骨骼动画
→ 实现角色外观与武器表现的灵活替换
```

战斗节奏调优、统计分析和局外系统暂时后置。

## 2. 新 Codex 接手时先做什么

1. 阅读本文件和 `Docs/开发阶段计划.md`。
2. 如果仓库根目录存在 `.codegraph/`，理解和定位代码时必须先使用 `codegraph explore`。
3. 如果需要调用 Unity Editor，必须先读取 `Application.dataPath`，确认连接的是本项目而不是同时打开的 CastleClasher2。
4. 不要进入 Play Mode。运行时玩法验证由用户执行；Codex只做编译、EditMode测试和静态资源校验。

正确的 Unity 项目标识：

```text
Application.dataPath=/Users/user/Desktop/JustTest/JustTest/Assets
Application.productName=JustTest
```

主要开发场景：

```text
Assets/Game/Scenes/CombatSandbox.unity
```

## 3. 已完成任务

### 3.1 工程基线

- Unity 2022.3 LTS工程已建立。
- 已建立 `JustTest.Game.Runtime` 和 `JustTest.Game.Tests.EditMode` 程序集。
- `CombatSandbox` 是当前战斗开发源场景。
- 第一版只支持键盘和旧版 `UnityEngine.Input`。
- 参数主要由 ScriptableObject配置，不将调试参数硬编码在行为代码中。

### 3.2 玩家移动

- 地面移动、快速转向和空中控制。
- 跳跃、可变跳高、土狼时间和跳跃缓存。
- 单向平台下穿。
- 翻滚、翻滚方向、冷却和无敌窗口。
- 摄像机跟随、角色重生和朝向提示。
- 用户已多次进行实际操作验证，反馈移动流畅。

主要路径：

- `Assets/Game/Runtime/Player`
- `Assets/Game/Runtime/Input`
- `Assets/Game/Data/Player`

### 3.3 通用战斗内核

- Hitbox、Hurtbox和单次攻击命中去重。
- 攻击 Windup、Active、Recovery三阶段时间线。
- 生命、伤害、受击硬直、击退、无敌和死亡。
- 失衡、击飞、眩晕等状态。
- 命中停顿、受击闪白、攻击后坐力、镜头震动和冲击粒子。
- 战斗逻辑与动画表现分离，攻击生效时间由代码和配置决定。

主要路径：

- `Assets/Game/Runtime/Combat`
- `Assets/Game/Runtime/Combat/Feedback`
- `Assets/Game/Data/Combat`

### 3.4 三武器与QTE

- 最多携带三把武器，不限制重复武器类型。
- 未携带武器时默认提供初级单手剑。
- 战斗中不再掉落武器，武器只在开局带入。
- 同一时间只有一把当前使用武器。
- 单手剑、双匕首、重锤均有普通连击、技能和QTE配置。
- 武器技能与武器绑定，消耗角色能量。
- 能量支持攻击命中回复和自动回复。
- QTE触发条件由目标状态决定：

| 武器 | QTE条件 |
|---|---|
| 单手剑 | 敌人失衡，例如摔倒或击退 |
| 双匕首 | 敌人被击飞 |
| 重锤 | 敌人眩晕 |

- 只有非当前武器能够成为QTE候选。
- 多把备用武器同时满足条件时同时高亮。
- 选择一把后取消其他候选，直到下一次条件触发。
- QTE执行后，所选武器成为当前武器。
- QTE没有处决效果。

主要路径：

- `Assets/Game/Runtime/Weapons`
- `Assets/Game/Data/Weapons`

### 3.5 战斗内HUD和结算

- 玩家生命、能量和技能状态显示。
- 三个武器槽、当前武器和QTE候选显示。
- 战斗胜利/失败结果界面。
- 支持战斗快速重开流程。

主要路径：

- `Assets/Game/Runtime/UI`
- `Assets/Game/Runtime/Run/CombatRunController.cs`

### 3.6 敌人与战斗平台

- 普通近战敌人。
- 远程敌人、投射物对象池和激光式攻击提示线。
- 敌人不再进行无限制跨场景激进追击。
- 玩家进入战斗平台后关闭边界并开始波次。
- 战斗平台不可设计为可下穿的单向平台。
- 敌人采用观察、站位、攻击、恢复和受控中断流程。
- 敌人之间取消实体阻挡，使用平台水平站位槽控制拥挤。
- 敌人生成使用对象池，场景和Prefab引用均由Inspector提供。

主要路径：

- `Assets/Game/Runtime/Enemies`
- `Assets/Game/Runtime/Run/CombatPlatformController2D.cs`
- `Assets/Game/Runtime/Run/CombatPositionSlotAllocator.cs`
- `Assets/Game/Data/Run/DefaultCombatEncounter.asset`

### 3.7 精英敌人

精英敌人是当前最新完成批次，作为 `CombatSandbox` 第4波单体敌人出现。

属性与攻击：

| 项目 | 数值 |
|---|---|
| 生命 | 260 |
| 快速斩 | 12伤害，0.18秒抬手，0.12秒生效，0.35秒恢复 |
| 延迟重击 | 25伤害，0.65秒抬手，0.16秒生效，0.70秒恢复 |
| 突进斩 | 18伤害，0.50秒抬手，0.28秒生效，0.55秒恢复 |

行为规则：

1. 玩家处于攻击恢复阶段且近距离时优先快速斩。
2. 玩家位于3.2至6.0距离且突进可用时使用突进斩。
3. 玩家近距离停留约0.7秒且重击可用时使用延迟重击。
4. 最大被动时间达到后尝试靠近并快速斩。
5. 玩家翻滚、攻击抬手或攻击生效阶段时以观察为主。

其他特性：

- 三种攻击拥有独立颜色和动作配置。
- 重击和突进在抬手阶段闪白预警。
- 突进只在攻击 Active阶段移动。
- 突进Motor逐物理帧限制停止X坐标，不会越过平台站位边界。
- 支持现有受击、失衡、击飞、眩晕和QTE系统。
- 复用共享攻击权、敌人池、反馈和战斗平台。

用户已经完成实际玩法测试，并确认功能正常。

## 4. 当前表现层结构

当前角色和敌人仍以白盒Sprite和程序化姿势为主。

主要组件：

- `Assets/Game/Runtime/Presentation/CharacterVisualRig2D.cs`
- `Assets/Game/Runtime/Presentation/PlayerWhiteboxMotionPresenter2D.cs`
- `Assets/Game/Runtime/Presentation/EnemyWhiteboxMotionPresenter2D.cs`
- `Assets/Game/Runtime/Presentation/RangedEnemyWhiteboxMotionPresenter2D.cs`
- `Assets/Game/Runtime/Presentation/EliteEnemyWhiteboxMotionPresenter2D.cs`
- `Assets/Game/Runtime/Presentation/CombatMotionProfile.cs`

`CharacterVisualRig2D`已经将以下内容拆开：

- 整体朝向根节点。
- 身体表现根节点。
- 主手武器轴与武器显示。
- 副手武器轴与武器显示。
- 基础动作姿势与受击反馈叠加。

这意味着后续可以替换模型表现层，而不需要重做移动、碰撞、攻击判定和QTE。

## 5. 当前进行中的设计事项

用户希望优先优化角色动作和实体表现，战斗节奏后续再做。

已确认方向：

- 主打原创美少女角色形象。
- 项目组目前没有美术人员。
- 使用PSD作为可编辑美术源文件。
- Unity侧使用分层PSB或透明PNG配合2D骨骼动画。
- 重点是灵活替换角色外观与武器表现。
- 第一版采用Unity 2D Animation，后续可视需求升级至Spine。

当前 `Packages/manifest.json` 尚未安装：

- `com.unity.2d.animation`
- `com.unity.2d.psdimporter`

安装包和改动工程前，先与用户确认最终参考图和美术方向。

## 6. 已确认的美术风格概念

方向：高速战斗美少女 + 哥特魔导武装。

角色定位：

- 成年女性佣兵或猎手。
- 6.5至7头身，纤细但有运动感。
- 银灰色高马尾，形成清晰的移动轮廓。
- 象牙白短款战斗外套，内侧绯红。
- 深灰战斗服、轻型护甲和高筒战斗靴。
- 腰部魔导核心作为技能能量视觉来源。
- 不依赖暴露服装，通过脸部、发型、动作和剪裁塑造美少女形象。

推荐色彩：

| 用途 | 色彩 |
|---|---|
| 主轮廓 | 象牙白、炭黑 |
| 角色识别色 | 绯红 |
| 魔导能量 | 青绿 |
| 单手剑效果 | 金白 |
| 双匕首效果 | 青蓝 |
| 重锤效果 | 赤红、琥珀 |

渲染风格：

- 高清二次元赛璐璐。
- 两至三级明暗和清晰描边。
- 少量手绘纹理，不使用复杂渐变。
- 可在后期增加轻微像素化或颗粒后处理。
- 场景降低饱和度，角色和武器效果保持高识别度。

武器表现：

| 武器 | 动作气质 | 表现 |
|---|---|---|
| 单手剑 | 稳定、干净 | 金白弧形剑光 |
| 双匕首 | 前倾、高频 | 青蓝短促残影和交叉斩 |
| 重锤 | 低重心、蓄力 | 赤红闪光、地面冲击和碎屑 |

## 7. 推荐的PSD与骨骼方案

美术源文件建议保存在Unity `Assets`目录外，避免Unity重复导入源PSD。Unity运行资源使用分层PSB或透明PNG。

推荐目录：

```text
ArtSource/Characters/Heroine_A/Heroine_A.psd
Assets/Game/Art/Characters/Heroine_A/Heroine_A.psb
Assets/Game/Art/Characters/Heroine_A/SpriteLibraries/
Assets/Game/Art/Characters/Heroine_A/Animations/
Assets/Game/Prefabs/Presentation/HeroineModel.prefab
Assets/Game/Data/Presentation/Characters/
Assets/Game/Data/Presentation/Weapons/
```

推荐PSD图层：

```text
Hair_Back
Cape_Back
Arm_Back_Upper
Arm_Back_Lower
Hand_Back
Leg_Back_Upper
Leg_Back_Lower
Foot_Back
Torso
Pelvis
Head
Face_Base
Eyes
Mouth
Hair_Front
Arm_Front_Upper
Arm_Front_Lower
Hand_Front
Leg_Front_Upper
Leg_Front_Lower
Foot_Front
Cape_Front
```

身体PSD中不能烘焙武器、剑光、受击闪光或粒子。

推荐骨架预算：

- 18至24根骨骼。
- 20至30个Sprite部件。
- 一张2048或4096图集。
- 1至3个正常Draw Call。
- 头发2至3段骨骼，披肩2段骨骼。
- 首版不使用实时布料物理。

## 8. 推荐的新表现层边界

建议保持现有物理和玩法根节点不变：

```text
PlayerRoot
├── Rigidbody2D
├── Collider2D
├── Movement / Roll / Attack / Health
├── AttackAnchor
└── VisualRoot
    └── HeroineModelView2D
        ├── Animator
        ├── SkeletonRoot
        ├── MainHandSocket
        ├── OffHandSocket
        ├── WeaponTrailSocket
        ├── BodyEffectSocket
        └── FootEffectSocket
```

建议新增但尚未实现的类型：

- `CharacterModelView2D`
- `CharacterAnimationPresenter2D`
- `CharacterAppearanceDefinition`
- `WeaponPresentationDefinition`
- `WeaponVisual2D`
- `AttackAnimationBinding`

运行时仍需遵守现有规则：所有依赖由Inspector提供。模型和武器使用带类型Prefab进行 `Instantiate`，不能通过 `GetComponent`或场景遍历获取脚本。

角色外观建议通过 Sprite Library切换。武器使用独立Prefab和手部插槽，不放在身体皮肤中。

## 9. 动画驱动原则

绝对不能通过 Animation Event直接决定伤害、状态或QTE是否生效。

攻击动画由现有战斗时间线驱动：

```text
AttackPhase.Windup
→ AttackPhase.Active
→ AttackPhase.Recovery
```

建议每个 `AttackAnimationBinding`保存：

- 对应的 `AttackDefinition`。
- 对应的 AnimationClip或Animator状态。
- Windup结束归一化时间。
- Active结束归一化时间。
- 可选的武器轨迹与特效配置。

Presenter根据 AttackRunner的 `Phase`和 `PhaseProgress`采样动画。替换动作素材时，攻击判定时间不会发生漂移。

移动动画可自由播放：

- Idle
- Run
- Jump
- Fall
- Land

受战斗逻辑控制的动作：

- Roll
- Hurt
- Knockback
- Airborne
- Stunned
- Dead
- 普通连击
- Skill
- QTE

## 10. 未完成任务与优先级

### P0：美少女角色表现竖切

当前最优先，尚未开始代码或资源实施。

1. 用户收集并确认约15张风格参考图。
2. 产出三张女主轮廓与服装方向草案。
3. 确认最终色板、发型、服装、比例和武器造型。
4. 创建第一版完整角色概念图和侧视拆分设定。
5. 创建可编辑PSD与Unity用分层PSB/PNG。
6. 安装Unity 2D Animation和PSD Importer。
7. 创建玩家共享骨架、权重和模型Prefab。
8. 创建基础移动、翻滚、受击和死亡动作。
9. 接入单手剑三段连击、技能和QTE。
10. 保留白盒表现开关用于对照验证。

第一批只做玩家模型和单手剑，不要一次制作所有角色和敌人。

### P1：武器表现扩展

P0验证后执行：

- 创建 `WeaponPresentationDefinition`。
- 单手剑使用主手插槽。
- 双匕首使用主手和副手插槽。
- 重锤使用主手插槽和副手2D IK握持点。
- 补充双匕首和重锤普通攻击、技能和QTE动画。
- QTE执行完成并切换当前武器时同步替换武器Prefab和动作集合。

### P2：敌人实体表现

- 普通近战敌人模型和动画。
- 远程敌人模型、射击和激光瞄准表现。
- 精英敌人快速斩、重击和突进动画。
- 敌人池回收时重置Animator、模型姿势和武器显示。

### P3：战斗节奏与数据

用户已明确后置：

- 波次出场间隔调优。
- 敌人组合和共享攻击权调优。
- 精英攻击频率和恢复窗口调优。
- 战斗耗时、受伤、闪避、QTE和武器伤害统计。

### P4：外部系统

战斗表现和节奏稳定后再考虑：

- 程序化关卡路线。
- 战斗奖励与恢复物。
- 局内升级选择。
- 局外成长、商店和存档。
- 正式音频、剧情和内容生产管线。

## 11. 工程实现约束

以下约束来自用户，必须继续遵守：

- 默认使用中文沟通，中文文件保持UTF-8。
- 可调参数使用 `[SerializeField] private`，不要把字段全部声明为 `public`。
- 尽量少使用静态类；需要全局服务时优先考虑明确生命周期的单例。
- Runtime禁止使用 `AddComponent`。
- Runtime禁止使用 `GetComponent`和 `TryGetComponent`获取依赖。
- Runtime禁止使用 `GameObject.Find`、`FindObjectOfType`和其他场景遍历方法。
- 场景资源使用Inspector引用。
- 动态对象使用绑定Prefab和带类型 `Instantiate`。
- 不要让正式角色Collider跟随动画骨骼变化。
- 角色和敌人的玩法根节点与表现模型必须分离。
- 武器、技能、攻击和QTE保持配置驱动。
- 不要把正式战斗规则放进Animator Controller或Animation Event。
- 不要修改无关文件或回退用户已有修改。
- 不要进入Play Mode；把玩法验证清单交给用户。

## 12. 最近验证结果

精英敌人批次完成后的验证：

- Unity脚本编译：0错误，0警告。
- EditMode测试：145通过，0失败，0跳过。
- 精英Prefab缺失脚本：0。
- 精英Prefab关键Inspector引用：完整。
- `DefaultEliteEnemy.asset`：有效。
- `EliteEnemyArchetype.asset`：有效，初始容量1，最大容量1。
- `EliteEnemyDefinition.asset`：Enemy阵营，260生命。
- `DefaultCombatEncounter.asset`：有效，共4波。
- 第4波：精英敌人×1。
- `CombatSandbox`对象池：包含普通近战、远程和精英三种原型。
- 新精英Runtime代码未发现禁用的查找、动态挂组件API。
- `git diff --check`通过。
- 未进入Play Mode。
- 用户随后完成实际玩法验证，确认测试正常。

## 13. 已知风险与注意事项

- 当前开发计划文档的后续顺序已经部分过时，本文件的当前优先级更高。
- 项目尚未安装Unity 2D Animation与PSD Importer，不能假定对应类型已经可用。
- 项目没有美术人员，第一批美术资产应选择易于分层和骨骼变形的造型，避免复杂披风、透明纱、密集花纹和大量挂件。
- AI生成的整张角色图不能直接视为可用骨骼资产，需要重新拆分、补齐关节遮挡和统一轮廓。
- 相同骨架换装只适用于比例接近的角色。体型差异明显时应使用独立模型Prefab。
- ArtStation、Pixiv和Pinterest只用于参考，不能直接把作者作品放入游戏。
- 购买第三方素材时必须检查商用、修改、再分发和AI相关授权。

## 14. 下一位 Codex 推荐执行顺序

在用户提供或确认参考图后：

1. 整理参考图中的共同视觉特征，不立即写代码。
2. 输出三套原创女主方向草案和固定色板。
3. 让用户选定一个方向。
4. 制作角色侧视标准姿势和身体部件拆分表。
5. 明确PSD/PSB图层、骨骼、插槽和资源预算。
6. 再制定Unity包安装和表现层代码修改方案。
7. 获得用户明确许可后才修改Packages、代码、Prefab和场景。

不要直接跳到制作全部武器和敌人。先完成“玩家模型 + 基础移动 + 翻滚 + 单手剑”的纵向切片。
