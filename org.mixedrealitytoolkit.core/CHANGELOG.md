# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.2.1-development] - 2024-04-24

### Fixed

* Fixed missing [CanEditMultipleObject] attributes as per Bug 573 [PR #698](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/698)

## [3.2.0] - 2024-03-20

### Added

* StabilizedRay constructor with explicit position and direction half life values. [PR #625](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/625)
* Added IsProximityHovered property of type TimedFlag to detect when a button starts being hovered or on interactor proximity and when it stops being hovered or on proximity of any interactor. [PR #611](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/611)
* Adding ProximityHover events (Entered & Exited) to PressableButton class. [PR #611](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/611)


### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. [PR #519](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/519)
* Fix warning and event triggered on disabled StatefulInteractable after changing speech settings [PR #591](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/591) [PR #608](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/608)