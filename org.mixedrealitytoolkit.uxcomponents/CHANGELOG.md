# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

* Added touch support for the NonNativeKeyboard. [PR #655](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/655)

### Fixed

* Changed PressableButton_Custom_Cylinder using Default-Material to MRTK_Standard_White instead. [PR #740](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/740)

### Changed

* Updated the minimum editor version to 2022.3.6f1 [PR #1003](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1003)

## [4.0.0-development.pre.1] - 2024-07-09

### Changed

* Updated package com.unity.xr.interaction.toolkit to 3.0.4

## [3.3.0] - 2024-06-13

### Added

* Added proximity-hover dynamic Frontplates functionality (Moved from Experimental to Release). [PR #712](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/712)
* Added unified font atlas and updated corresponding fonts and their materials. [PR #700](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/700)

### Fixed

* Reduced package description to support for UPM package publishing in the Unity Asset Store.

## [3.2.0] - 2024-03-20

### Added

* Added Empty, Action, Action with Checkbox, and ToggleSwitch Experimental buttons prefabs [PR #649](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/649)
* Added corresponding MenuItems for use in Editor Tools [PR #649](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/649)
* Added CanvasUITearsheetDynamicFrontplate scene [PR #649](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/649)
* Added SimpleButton + demo scene [PR #635](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/635)

### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. [PR #519](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/519)
* Fixed Bug #643 - Experimental SimpleEmptyButton and SimpleActionButton prefabs have missing "See It Say It Label" GameObject reference [PR #646](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/646)
* Auto disable button's colliders when `StatefulInteractable` is disabled [PR #626](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/626)
