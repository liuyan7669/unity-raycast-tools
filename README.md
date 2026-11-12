# Raycast Utilities

`com.cowart.raycast-utilities` 是面向 Unity 3D Physics 的可视化射线检测包，提供直线射线、三种扇形射线和方块投射。所有组件都以脚本所绑定物体的位置为默认起点，以物体自身本地 `+Z` 轴为前方，并通过统一的 `Cast()` 与 `Action<RaycastResult>` 事件返回结果。

- Package Manager 显示名：`Raycast Utilities`
- 包名：`com.cowart.raycast-utilities`
- Runtime 程序集：`Cowart.RaycastUtilities`
- Editor 程序集：`Cowart.RaycastUtilities.Editor`
- 命名空间：`Cowart.RaycastUtilities`
- 许可：MIT

## 功能

- 前、后、左、右直线射线，可通过 `RayDirections` 组合方向。
- Plane：单个水平扇面。
- Volume：在 Plane 基础上，从同一起点上下旋转多个完整扇面。
- Solid3D：复制 Plane，并沿物体自身本地 Y 轴上下平移多个平行扇面。
- 方块射线：沿物体自身本地 `+Z` 轴执行 `Physics.BoxCast`。
- `ClosestOnly`：只检查每条检测路径上最近且无遮挡的物体。
- `AllHits`：穿透并检查检测路径上的全部物体。
- 所有组件统一返回 `RaycastResult`，并同步触发 `Action<RaycastResult> CastCompleted`。
- Scene 视图实时预览，默认绿色，可修改颜色或设置为仅选中时显示。
- FanRaycaster Inspector 只显示当前模式实际生效的参数。
- 所有公开和受保护 API 均包含中文 XML 文档，Inspector 参数包含中文 Tooltip。
- 包含可通过 Package Manager 导入的完整场景示例。

## 环境要求

- Unity 2021.3 LTS 或更高版本。
- 使用 Unity 3D Physics：`UnityEngine.Physics`。
- 本包不调用 `Physics2D`，不适用于 2D Collider。
- Scene 预览依赖 Unity Scene 视图右上角的 Gizmos 开关。

`package.json` 中的 `"unity": "2021.3"` 表示最低 Unity 版本。当前自动验证基线是 Unity `2021.3.30f1`；更高 Unity 大版本仍应在实际项目中重新编译和验收。

## 安装

当前包版本为 `1.0.0`。正式项目建议固定 Tag，不要直接依赖持续变化的 `main` 分支。

### 通过 OpenUPM 安装（发布后）

包被 OpenUPM 收录后，在 Unity 的 `Edit > Project Settings > Package Manager` 中添加：

```text
Name: package.openupm.com
URL: https://package.openupm.com
Scope: com.cowart.raycast-utilities
```

然后通过 `Add package by name` 安装：

```text
com.cowart.raycast-utilities
```

当前 README 仅预留标准安装方式，不代表已经在 OpenUPM 实时可用。

### 通过 Git URL 安装

创建并推送 `1.0.0` Tag 后，在 Package Manager 中使用：

```text
https://github.com/liuyan7669/unity-raycast-utilities.git#2.0.0
```

开发期间如需跟踪主分支，可以临时使用不带 Tag 的 Git URL，但正式项目不建议这样做。

### 从本地磁盘安装

1. 克隆或解压本仓库。
2. 打开 **Window > Package Manager**。
3. 点击左上角 **+**，选择 **Add package from disk...**。
4. 选择仓库根目录下的 `package.json`。

也可以在目标项目的 `Packages\manifest.json` 中使用本地目录：

```json
"com.cowart.raycast-utilities": "file:E:/YourPackages/unity-raycast-utilities"
```

JSON 中的 Windows 路径建议使用正斜杠，避免反斜杠转义错误。

### 作为 Embedded Package 安装

把完整包目录放到项目的 `Packages\com.cowart.raycast-utilities`，并在 `Packages\manifest.json` 中添加：

```json
"com.cowart.raycast-utilities": "file:com.cowart.raycast-utilities"
```

计划使用的远端仓库地址是：

```text
https://github.com/liuyan7669/unity-raycast-utilities.git
```

## 导入 All Raycasters 示例

1. 打开 **Window > Package Manager**。
2. 选中 **Raycast Utilities**。
3. 展开 **Samples**。
4. 点击 **All Raycasters** 右侧的 **Import**。
5. 打开导入后的 `Raycast Utilities Demo.unity`。

Unity 2021.3 默认会将示例导入到：

```text
Assets\Samples\Raycast Utilities\1.0.0\All Raycasters
```

不进入 Play Mode 时，Scene 视图会显示默认绿色预览线。进入 Play Mode 后，左上角操作面板可以：

- 执行全部组件的统一 `Cast()`。
- 切换 `ClosestOnly` 和 `AllHits`。
- 调用直线射线的单方向与批量便捷接口。
- 读取三种扇形的方向、起点和原始命中。
- 调用方块射线的最近命中与 NonAlloc 接口。

示例使用深蓝灰地板、暖橙目标方块、青蓝起点球和近白标题文字，便于区分射线起点、目标和场景背景。

## 快速开始

把任意射线组件挂到 GameObject。脚本所在物体的 `Transform.position` 是默认检测起点，物体自身本地 `+Z` 是前方。

```csharp
using Cowart.RaycastUtilities;
using UnityEngine;

public sealed class RaycastUsageExample : MonoBehaviour
{
    [SerializeField] private RaycasterBase raycaster;

    private void OnEnable()
    {
        raycaster.CastCompleted += HandleCastCompleted;
    }

    private void OnDisable()
    {
        raycaster.CastCompleted -= HandleCastCompleted;
    }

    private void FixedUpdate()
    {
        RaycastResult result = raycaster.Cast();

        if (result.TryGetClosestHit(out RaycastSampleHit closestHit))
        {
            Debug.Log("最近命中：" + closestHit.Hit.collider.name);
        }
    }

    private void HandleCastCompleted(RaycastResult result)
    {
        foreach (RaycastSampleHit sampleHit in result.Hits)
        {
            Debug.Log("事件命中：" + sampleHit.Hit.collider.name);
        }
    }
}
```

`Cast()` 的直接返回值与 `CastCompleted` 事件参数是同一个 `RaycastResult` 对象。事件会在检测完成后同步触发；对象禁用或销毁前应对称取消订阅。

## 统一返回结果

所有射线组件都继承 `RaycasterBase`，并使用相同的 `RaycastResult`：

| 成员 | 类型 | 说明 |
|---|---|---|
| `Source` | `RaycasterBase` | 发起本次检测的组件。 |
| `DetectionMode` | `RaycastDetectionMode` | 本次实际使用的检测方式。 |
| `Hits` | `List<RaycastSampleHit>` | 按 Collider 去重并按距离从近到远排列的结果。 |
| `HasHit` | `bool` | 是否至少命中一个有效 Collider。 |
| `HitCount` | `int` | 去重后的命中 Collider 数量。 |
| `TryGetClosestHit(...)` | `bool` | 安全获取距离最近的命中。 |

每个 `RaycastSampleHit` 包含：

| 成员 | 说明 |
|---|---|
| `Origin` | 该条采样射线的世界空间起点。 |
| `Direction` | 该条采样射线的世界空间方向。 |
| `Hit` | Unity 返回的 `RaycastHit`。 |

同一个 Collider 被多条扇形采样射线命中时，统一 `Cast()` 结果只保留距离最近的一项。如果业务需要保留每条采样射线产生的原始重复命中，请使用对应组件的 `CastAll(List<...>)` 便捷接口。

## 最近命中与穿透检测

| 模式 | 行为 |
|---|---|
| `ClosestOnly` | 每条直线或扇形采样线只取最近且无遮挡的命中；方块投射只取最近命中。 |
| `AllHits` | 直线和扇形采样线使用穿透检测；方块投射收集扫过路径中的全部命中。 |

```csharp
raycaster.DetectionMode = RaycastDetectionMode.ClosestOnly;
RaycastResult closestResult = raycaster.Cast();

raycaster.DetectionMode = RaycastDetectionMode.AllHits;
RaycastResult allHitsResult = raycaster.Cast();
```

`RaycastResult.Hits` 是当前结果对象拥有的可修改列表。业务需要长期保存原始结果时，不要在其他调用方共享并直接修改同一个列表。

## 直线射线

将 `StraightRaycaster` 挂到物体，通过 `Directions` 选择一个或多个方向：

| 方向 | 物体自身坐标轴 |
|---|---|
| `Forward` | 本地 `+Z` |
| `Back` | 本地 `-Z` |
| `Left` | 以自身前方为参考的本地 `-X` |
| `Right` | 以自身前方为参考的本地 `+X` |
| `All` | 同时检测前、后、左、右 |

```csharp
using System.Collections.Generic;
using Cowart.RaycastUtilities;
using UnityEngine;

public sealed class StraightRayExample : MonoBehaviour
{
    [SerializeField] private StraightRaycaster raycaster;
    private readonly List<DirectionalRaycastHit> hits =
        new List<DirectionalRaycastHit>();

    private void FixedUpdate()
    {
        if (raycaster.CastForward(out RaycastHit forwardHit))
        {
            Debug.Log("前方最近命中：" + forwardHit.collider.name);
        }

        int count = raycaster.CastAll(hits);
        for (int i = 0; i < count; i++)
        {
            Debug.Log(hits[i].Direction + " 命中 " + hits[i].Hit.collider.name);
        }
    }
}
```

`CastForward/Back/Left/Right` 和带方向参数的 `Cast(...)` 始终只取该方向最近命中，不读取 `DetectionMode`，也不会触发 `CastCompleted`。新业务代码需要统一结果时优先使用无参数 `Cast()`。

## 扇形射线

`FanRaycaster.Mode` 支持三种模式：

| 模式 | 起点 | 方向和形状 | 专用参数 |
|---|---|---|---|
| `Plane` | 组件物体位置 | 在物体本地 XZ 平面围绕本地 Y 轴展开一个扇面 | 无 |
| `Volume` | 所有扇面共用组件物体位置 | 在 Plane 基础上绕本地 X 轴上下旋转多个完整扇面 | `VerticalAngle`、`VerticalPlaneCount` |
| `Solid3D` | 各层起点沿本地 Y 轴平移 | 复制多个方向完全相同、互相平行的 Plane 扇面 | `VerticalHeight`、`VerticalPlaneCount` |

公共水平参数：

- `Angle`：每个扇面的水平完整张角。
- `PlaneRayCount`：每个扇面从左到右的射线数量。

Volume 的 `VerticalAngle` 是最下层到最上层的完整垂直张角。Solid3D 的 `VerticalHeight` 是最下层起点到最上层起点的总高度。

```csharp
using System.Collections.Generic;
using Cowart.RaycastUtilities;
using UnityEngine;

public sealed class FanRayExample : MonoBehaviour
{
    [SerializeField] private FanRaycaster raycaster;
    private readonly List<RaycastSample> samples = new List<RaycastSample>();
    private readonly List<RaycastSampleHit> rawHits = new List<RaycastSampleHit>();

    private void FixedUpdate()
    {
        raycaster.GetWorldSamples(samples);
        raycaster.CastAll(rawHits);
    }
}
```

`GetWorldDirections(...)` 只返回方向。Solid3D 的平行扇面会产生重复方向，因此需要区分各层起点时必须使用 `GetWorldSamples(...)` 并读取 `RaycastSample.Origin`。

`CastAll(List<RaycastSampleHit>)` 返回每条采样射线的原始命中，同一 Collider 可能出现多次，且结果不保证全局距离排序。需要去重排序时使用统一无参数 `Cast()`。

## 方块射线

`BoxRaycaster` 的起始方块中心是脚本所绑定物体的位置，沿自身本地 `+Z` 轴投射。

- `HalfExtents`：本地半尺寸。
- `ScaleWithTransform`：是否乘以物体的 `lossyScale`。
- `WorldHalfExtents`：最终参与检测的世界半尺寸。

```csharp
using Cowart.RaycastUtilities;
using UnityEngine;

public sealed class BoxRayExample : MonoBehaviour
{
    [SerializeField] private BoxRaycaster raycaster;
    private readonly RaycastHit[] hits = new RaycastHit[16];

    private void FixedUpdate()
    {
        if (raycaster.Cast(out RaycastHit closestHit))
        {
            Debug.Log("方块最近命中：" + closestHit.collider.name);
        }

        int count = raycaster.CastNonAlloc(hits);
        Debug.Log("NonAlloc 写入数量：" + count);
    }
}
```

`Cast(out RaycastHit)` 始终只取最近命中。`CastNonAlloc(...)` 直接使用 Unity 的 `Physics.BoxCastNonAlloc`，不按 `DetectionMode` 筛选、不统一去重排序，也不会触发完成事件。

## 公共设置

| Inspector / 属性 | 说明 |
|---|---|
| `Distance` | 最大检测距离，小于 0 的值会限制为 0。 |
| `Detection Mode` / `DetectionMode` | 最近无遮挡命中或穿透全部命中。 |
| `Layer Mask` / `LayerMask` | 参与检测的物理层。 |
| `Query Trigger Interaction` | 是否检测 Trigger；UseGlobal 遵循 Unity 全局物理设置。 |
| `Preview Color` / `PreviewColor` | Scene 预览颜色，默认绿色。 |
| `Preview Only When Selected` | 是否仅在选中组件物体时绘制预览。 |
| `Origin` | 默认世界起点，即组件物体位置。 |
| `Forward` | 默认世界前方，即组件物体自身本地 `+Z`。 |

## 编辑器预览

预览只用于 Scene 视图配置，不会在 Game 视图或最终 Player 中生成运行时线条。

- 未显示预览时，先检查 Scene 视图右上角的 Gizmos 开关。
- 默认颜色是绿色，可通过 Inspector 或 `PreviewColor` 修改。
- `PreviewOnlyWhenSelected=true` 时，只有选中组件物体才显示。
- 扇形模式切换后，自定义 Inspector 只显示当前模式真正生效的垂直属性。
- 方块射线显示起始方块、结束方块和扫掠边线。

## 测试

开发工程需要安装 Unity Test Framework，并在项目 `Packages\manifest.json` 的 `testables` 中加入：

```json
"com.cowart.raycast-utilities"
```

然后在 **Window > General > Test Runner > EditMode** 中运行 `Cowart.RaycastUtilities.Editor.Tests`。

当前自动验证基线：

- Unity `2021.3.30f1`。
- Runtime、Editor、Sample 和 Tests 程序集编译无错误。
- EditMode 测试 `11/11` 通过。
- Package Manager 能导入 `All Raycasters` Sample。
- 示例场景能打开，五个控制器引用完整。
- Plane、Volume、Solid3D 和方块射线在 `ClosestOnly`、`AllHits` 下均能命中预设目标。

这些结果属于本地 Unity 自动验证，不等于所有 Unity 版本、渲染管线、目标设备或业务场景都已验收。

## 包目录结构

```text
com.cowart.raycast-utilities/
  .gitattributes
  package.json
  README.md
  CHANGELOG.md
  LICENSE.md
  Runtime/
    Cowart.RaycastUtilities.asmdef
    RaycasterBase.cs
    RaycastResult.cs
    RaycastDetectionMode.cs
    RaycastSample.cs
    RaycastSampleHit.cs
    RayDirections.cs
    DirectionalRaycastHit.cs
    StraightRaycaster.cs
    FanRaycaster.cs
    BoxRaycaster.cs
  Editor/
    Cowart.RaycastUtilities.Editor.asmdef
    FanRaycasterEditor.cs
  Samples~/
    All Raycasters/
      Cowart.RaycastUtilities.Samples.asmdef
      RaycastAllFeaturesExample.cs
      Raycast Utilities Demo.unity
      README.md
      Materials/
        Ground.mat
        Target.mat
        Ray Origin.mat
  Tests/
    Editor/
      Cowart.RaycastUtilities.Editor.Tests.asmdef
      StraightRaycasterTests.cs
      FanRaycasterTests.cs
      BoxRaycasterTests.cs
      FanRaycasterEditorTests.cs
      UnifiedRaycasterTests.cs
```

Unity 的 `.meta` 文件属于包资产引用的一部分，必须与对应脚本、程序集、场景和材质一同提交。不要只复制上面目录树中显示的源码文件。

## 升级与卸载

- 升级前查看 `CHANGELOG.md`，并备份包含射线组件的场景和 Prefab。
- 不要同时保留 Assets 下的旧源码和 UPM 包，否则可能产生重复类型或程序集冲突。
- 卸载时移除项目 `manifest.json` 中的依赖，并移除对应本地包目录或 Git URL。
- 已导入到 `Assets\Samples` 的示例属于项目资产，卸载包不会自动删除；不再需要时可单独移除。
- 业务程序集仍引用 `Cowart.RaycastUtilities` API 时，卸载后产生编译错误属于预期行为，应先移除业务引用。

## License

MIT，详见 [LICENSE.md](LICENSE.md)。

## 改名迁移（2.0.0）

- 包标识：`com.cowart.raycast-tools` → `com.cowart.raycast-utilities`。
- Git 仓库：`https://github.com/liuyan7669/unity-raycast-tools.git` → `https://github.com/liuyan7669/unity-raycast-utilities.git`；原有提交和旧 tag 保留。
- 程序集、命名空间、示例和测试已同步使用新名称。业务项目需更新 manifest、asmdef 和 using；旧包与新包不可同时安装。
- 资源 .meta GUID 保留；已改命名空间的运行时类型提供 MovedFrom 迁移标记。已有源码仍需按新命名空间修改。
- 示例中的可选集成依赖保持可选，核心包不新增其他 Cowart 包依赖。
