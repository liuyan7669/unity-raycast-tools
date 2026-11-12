# Changelog

## [2.0.0] - 2026-11-12

- 隔离射线测试的物理检测层，避免当前打开场景的碰撞体干扰命中数量。

### 破坏性变更

- `com.cowart.raycast-tools` 改为 `com.cowart.raycast-utilities`；更新业务项目的安装地址、程序集和源码引用。
- 完成包标识、目录、程序集、命名空间、脚本及文档改名，保持资源 GUID 和包独立性。

## [1.0.2] - 2026-11-08

- 统一 Cowart 包图标：重新设计为按功能类别配色的圆角图标（扇形射线 + 命中点），同步更新 `Icon.png` 与 `Icon@2x.png`。

## [1.0.1] - 2026-08-23

- 新增包专用图标，并将包内组件脚本的默认图标统一替换为该图标。

## [1.0.0] - 2026-08-16

### Added

- Added forward, back, left, and right straight raycasts.
- Added planar fan raycasts and volume raycasts composed of vertically stacked planar fans.
- Added Solid3D raycasts composed of vertically translated parallel planar fans.
- Added a mode-aware FanRaycaster Inspector that hides parameters unused by the selected mode.
- Added a unified `Cast()` API and `Action<RaycastResult>` completion event for all raycaster components.
- Added shared ClosestOnly and AllHits detection modes with unique, distance-sorted Collider results.
- Added forward box cast support.
- Added configurable Scene view previews with a green default color.
- Added an importable All Raycasters sample with a complete demonstration scene, high-contrast materials, and documented usage for every raycaster mode and public casting workflow.
