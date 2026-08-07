# 玩家角色 PSB 分层约定

该目录中的 `PlayerModelVerticalSlice.prefab` 是技术验证模型。正式角色尚未定稿前，
可以按下列稳定层级制作 PSB，并逐层替换当前临时 Sprite，不需要改玩法根节点、碰撞体
或战斗时间线。

## 文档设置

- 文件格式：PSB。
- 建议画布：2048 x 2048，透明背景。
- 角色朝向：默认朝右。
- 原点：角色脚底中心。
- 图层名称使用 ASCII，避免重导入后绑定名称发生变化。
- 不合并需要独立摆动或换装的部位。

## 推荐图层

```text
FX_Front
Weapon_Main
Hand_Front
Arm_Front_Lower
Arm_Front_Upper
Hair_Front
Face
Head
Torso_Accent
Torso
Skirt
Leg_Front_Upper
Leg_Front_Lower
Foot_Front
Leg_Back_Upper
Leg_Back_Lower
Foot_Back
Arm_Back_Upper
Arm_Back_Lower
Hand_Back
Cape
Hair_Back
FX_Back
```

## 骨骼与插槽

当前技术模型使用 20 个左右的 Transform 骨骼。正式 PSB 导入后，应保持以下插槽语义：

- `MainHandSocket`：主手武器挂点。
- `WeaponFeedbackPivot`：命中后坐力叠加层，不由动画关键帧控制。
- `BodyEffectSocket`：身体状态和受击特效挂点，后续补充。
- `FootEffectSocket`：落地和移动特效挂点，后续补充。

武器不得绘制进身体图层。武器 Sprite 与尺寸由
`WeaponPresentationDefinition` 配置，以便在不重做角色动画的情况下替换武器。

## Unity 导入检查

1. Texture Type 选择 `Sprite (2D and UI)`。
2. Sprite Mode 选择 `Multiple`，Import Mode 选择 `Individual Sprites (Mosaic)`。
3. Character Rig 保留图层层级和名称。
4. 重导入后检查骨骼权重、排序顺序、主手插槽和脚底原点。
5. 动画事件不得造成伤害、施加状态或触发 QTE；这些规则仍由现有代码时间线负责。
