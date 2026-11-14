# Changelog

## [Unreleased]

## [3.1.0] - 2026-11-14

- 接入 OpenUPM 官方 tag 发布工作流，自动触发扫描并检查注册表发布结果。

- 统一 Cowart 包版本为 3.1.0。

- 为盒形尺寸与直线方向补齐独立框架分组，收起后不再残留未分组参数。

- 包内分组显式使用 Inspector Groups 框架样式；用户新增分组默认保持普通样式，所有绘制集中在 Inspector Groups。

- 使用类级 HideScript 声明隐藏 Script 行，移除专用 Inspector 中写死的排除规则。

- 修复选中对象在脚本继承关系变更、重编译后旧 Inspector 继续访问失效字段的问题；统一验证目标类型并刷新字段绑定。

- 声明 Inspector Groups 3.1.0 依赖；移除包内 Inspector 自定义样式，业务操作复用统一绘制。

- 统一菜单分类与中文名称，按选中对象的适用范围补充右键入口。

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
