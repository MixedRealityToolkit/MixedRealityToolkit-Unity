# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.3.0-development] - 2024-09-12

### Changed

* StateVisualizer: Modified access modifiers of State, stateContainers and UpdateStateValue to protected internal to allow adding states through subclassing. [PR #926](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/926)

### Fixed

* Fixed an issue when selecting a PressableButton in Editor scene view causing error spam. (Issue #772) [PR #943](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/943)

## [3.2.2-development] - 2024-08-29

### Changed

* Package patch version update to allow UPM publishing

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