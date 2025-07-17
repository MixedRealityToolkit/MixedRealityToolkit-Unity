# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.3.0-development] - 2024-04-26

### Added

* Added proximity-hover dynamic Frontplates functionality (Moved from Experimental to Release). [PR #712](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/712)
* Added unified font atlas and updated corresponding fonts and their materials. [PR #700](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/700)

### Fixed

* Reduced package description to support for UPM package publishing in the Unity Asset Store.
* Changed PressableButton_Custom_Cylinder using Default-Material to MRTK_Standard_White instead. [PR #740](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/740)

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