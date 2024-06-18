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

## [3.3.0] - 2024-04-30

### Added

* Made bounds control overridable for custom translation, scaling and rotation logic using manipulation logic classes. [PR #722](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/722)

### Fixed

* Added null check and index check when hiding colliders on BoundsHandleInteractable. [PR #730](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/730)

## [3.2.0] - 2024-03-20

### Added

* ObjectManipulator's ManipulationLogic observes XRSocketInteractor, XRI v2.3.0. [PR #567](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/567)

### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. [PR #519](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/519)