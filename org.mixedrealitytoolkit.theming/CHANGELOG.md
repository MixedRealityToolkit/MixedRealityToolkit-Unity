# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

* Added serializable theme item data types for booleans. [PR #1130](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1130)
* Added built-in binders for skybox materials and behaviour enabled states. [PR #1130](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1130)

## [1.0.0-pre.1] - 2026-05-20

### Added

* Added the new MRTK UX Theming package (`org.mixedrealitytoolkit.theming`) for runtime theme switching across MRTK canvas UI. [PR #1119](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1119)
  * Added `ThemeDataSource`, `Theme`, `ThemeDefinition`, and `ThemeBinding` as the core theming data model and binding workflow.
  * Added serializable theme item data types for colors, floats, font icon sets, materials, rect offsets, sprites, TextMeshPro font assets, and Vector3 values.
  * Added built-in binders for common Unity and MRTK UI properties, including graphic color/material, renderer material, image and sprite renderer sprite/color, layout group padding, rounded rectangle mask radius, transform local scale, TMP font assets, font icon sets, and StateVisualizer tint colors.
  * Added editor tooling for creating and configuring themes, theme data sources, theme bindings, and font icon set maps in the Unity Inspector.
  * Added package documentation, screenshots, license, assembly definitions, sample theme assets, and a default MRTK font icon set map.
