# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [4.0.0-development.pre.1] - 2024-06-18

### Changed

* Updated package com.unity.xr.interaction.toolkit to 3.0.3

### Removed

* Removed LegacyDialog/Dialog files.
* Removed LegacyDialog/DialogButton files.
* Removed LegacyDialog/DialogButtonContext files.
* Removed LegacyDialog/DialogButtonHelpers files.
* Removed LegacyDialog/DialogButtonTypes files.
* Removed LegacyDialog/DialogProperty files.
* Removed LegacyDialog/DialogShell files.
* Removed LegacyDialog/DialogState files.
* Removed LegacyDialog/README files.
* Removed obsolete Slider.SliderValue fields.

## [3.2.1] - 2024-04-23

### Fixed

* Fixed missing [CanEditMultipleObject] attributes as per Bug 573 [PR #698](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/698)
* Reduced package description to support for UPM package publishing in the Unity Asset Store.

## [3.2.0] - 2024-03-20

### Added

* Added Experimental Buttons with dynamic Frontplate and demo CanvasUITearsheetDynamicFrontplate scene. [PR #649](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/649)

### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. [PR #519](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/519)
* Fixed some buttons in nonnative keyboard not having sound [PR #648](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/648)