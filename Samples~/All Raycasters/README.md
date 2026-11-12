# All Raycasters

导入 Sample 后打开 `Raycast Utilities Demo.unity`。不进入 Play Mode 时，Scene 视图会直接显示默认绿色预览线；进入 Play Mode 后，左上角操作面板可以执行所有示例接口。

## 场景内容

- `Straight Raycaster`：启用前、后、左、右四个方向，演示物体自身本地坐标轴方向。
- `Fan Plane Raycaster`：一个水平扇面。
- `Fan Volume Raycaster`：从同一起点上下旋转多个完整扇面。
- `Fan Solid3D Raycaster`：沿物体自身 Y 轴上下平移多个平行扇面。
- `Box Raycaster`：从物体位置沿自身 +Z 执行方块投射。
- 每一组射线和目标物体使用独立物理层，避免不同演示区域互相命中。

## 运行方式

1. 打开场景后进入 Play Mode。
2. 默认每 0.5 秒调用全部组件的统一 `Cast()`。
3. 点击“切换全部 ClosestOnly / AllHits”，比较最近无遮挡命中和穿透全部命中的差异。
4. 点击三个便捷接口按钮，分别查看直线、扇形和方块的旧接口使用方式。
5. Scene 视图绿色线表示编辑器预览；进入 Play Mode 后，黄色线表示统一结果中的实际命中路径。

## 示例脚本覆盖的接口

- 公共接口：`Distance`、`DetectionMode`、`LayerMask`、`QueryTriggerInteraction`、`PreviewColor`、`PreviewOnlyWhenSelected`、`Cast()`、`CastCompleted`。
- 直线射线：`Directions`、单方向 `Cast(...)`、`CastForward/Back/Left/Right`、`CastAll(...)`、`GetWorldDirection(...)`。
- 扇形射线：三种 `Mode`、各模式参数、`GetWorldDirections(...)`、`GetWorldSamples(...)`、`CastAll(...)`。
- 方块射线：`HalfExtents`、`ScaleWithTransform`、`WorldHalfExtents`、`Cast(out RaycastHit)`、`CastNonAlloc(...)`。

新业务代码优先采用统一无参数 `Cast()` 与 `Action<RaycastResult>` 事件。便捷接口用于只关心单次特定检测或希望复用调用方结果容器的场景。
