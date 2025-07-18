# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Changed

* Updated ObjectManipulator and ObjectManipulatorTests to be compatible with renamed rigidbody properties in Unity 6.
* Updated the minimum editor version to 2022.3.6f1 [PR #1003](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1003)

## [4.0.0-development.pre.1] - 2024-07-09

### Added

* SolverTapToPlaceTestsForControllerlessRig Unity-tests.
* Ported SolverTapToPlaceTests so that they also test the new controllerless prefabs.
* Updated TapToPlace logic to handle both deprecated XRController and new controllerless actions.
* Updated HandConstraintPalmUp logic to handle both deprecated XRController and new controllerless actions.
* Updated Solver logic to handle both deprecated XRController and new controllerless actions.

### Changed

* Updated package com.unity.xr.interaction.toolkit to 3.0.4

## Unreleased

### Added

* Added different types of maintaining scale for bounds handles. [PR #976](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/976)

### Fixed

* Fixed tap to place `StartPlacement()` when called just after instantiation of the component. [PR #785](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/785)
* Fix null ref in SpatialManipulationReticle when multiple interactables are hovered. [PR #873](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/873)
* ConstantViewSize solver now retains the initial scale and aspect ratio [PR #719](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/719)
* Fixed Follow solver frequently logging "Look Rotation Viewing Vector Is Zero" [PR #895](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/895)

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