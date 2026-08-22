# Changelog

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
