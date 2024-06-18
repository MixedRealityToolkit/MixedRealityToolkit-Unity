# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [4.0.0-development.pre.1] - 2024-06-18

### Added

* MRTK3 controllerless prefabs.
* Added ObsoleteHandInteractionExamples scene that still uses deprecated MRTK3 rig.
* Added XRI3+ exclusive unity-tests.
* Added new TrackedPoseDriver lookup.

### Changed

* Updated package com.unity.xr.interaction.toolkit to 3.0.3
* Updated Unity Editor to 2022.3.7f1
* Updated all scenes to consume new MRTK3 controllerless prefabs.
* Updated existing unity-tests.
* Updated code to support new XRI3 patterns.

### Deprecated

* MRTK3 controller-based prefabs marked as obsolete and deprecated.
* Controller lookup deprecated.

### Removed

* Removed obsolete methods.
* Removed legacy dialog*.

## [3.2.2] - 2024-06-13

### Fixed

* Fixed UPM package validation so that it ignores errors caused when the test runner is not part of the MRTK publisher account. [PR #775](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/775/)

## [3.2.1] - 2024-04-24

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