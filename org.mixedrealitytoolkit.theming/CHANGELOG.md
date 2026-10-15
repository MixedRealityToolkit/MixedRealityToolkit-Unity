# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

* Added serializable theme item data types for booleans. [PR #1130](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1130)
* Added built-in binders for skybox materials and behaviour enabled states. [PR #1130](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1130)
* Added edit mode tests for validating custom theming types and their dependencies. [PR #1133](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1133)
* Added runtime performance benchmark and regression tests for theming hot paths. [PR #1133](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1133)

### Changed

* Optimized `Theme.TryGetItemData<T>` from an $O(N)$ linear list scan to an $O(1)$ hash table lookup with automatic cache invalidation on deserialization, inspector modifications, and enable events. [PR #1133](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1133)
* Optimized `BaseThemeBinder` by caching `UnityAction<ChangeEvent<Theme>>` delegates, eliminating heap allocations during repeated subscribe/unsubscribe cycles. [PR #1133](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1133)
* Optimized `ThemeDataSource` to route programmatic subscribers through a Zero-GC dynamic listener list, avoiding delegate allocations while preserving inspector `UnityEvent` invocations. [PR #1133](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1133)

## [1.0.0-pre.1] - 2026-05-20

### Added

* Added the new MRTK UX Theming package (`org.mixedrealitytoolkit.theming`) for runtime theme switching across MRTK canvas UI. [PR #1119](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1119)
  * Added `ThemeDataSource`, `Theme`, `ThemeDefinition`, and `ThemeBinding` as the core theming data model and binding workflow.
  * Added serializable theme item data types for colors, floats, font icon sets, materials, rect offsets, sprites, TextMeshPro font assets, and Vector3 values.
  * Added built-in binders for common Unity and MRTK UI properties, including graphic color/material, renderer material, image and sprite renderer sprite/color, layout group padding, rounded rectangle mask radius, transform local scale, TMP font assets, font icon sets, and StateVisualizer tint colors.
  * Added editor tooling for creating and configuring themes, theme data sources, theme bindings, and font icon set maps in the Unity Inspector.
  * Added package documentation, screenshots, license, assembly definitions, sample theme assets, and a default MRTK font icon set map.
