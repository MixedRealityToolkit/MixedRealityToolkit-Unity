# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.4.0] - 2025-11-12

### Added

* Added different types of maintaining scale for bounds handles. [PR #976](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/976)
* Added InitializeToTargetForward field/property to RadialView to position a GameObject in front of its target (like at the center of the view) when it becomes enabled. [PR #655](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/655)

### Fixed

* ConstantViewSize solver now retains the initial scale and aspect ratio [PR #719](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/719)
* Fixed Follow solver frequently logging "Look Rotation Viewing Vector Is Zero" [PR #895](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/895)
* Fixed issues with HandConstraint hand tracking events not being fired (OnFirstHandDetected/OnHandActivate/OnLastHandLost/OnHandDeactivate) when SolverHandler TrackedHand is set to Right or Left (not Both) [PR #956](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/956)
* Updated dependencies to match Unity Asset Store packages. [PR #1054](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1054)
  * org.mixedrealitytoolkit.core 3.2.2
* Fixed `Constraint Manager` not properly highlighting custom constraints via the "Go to component" button. [PR #1078](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1078)

## [3.3.1] - 2024-08-29

### Fixed

* Fixed tap to place `StartPlacement()` when called just after instantiation of the component. [PR #785](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/785)
* Fix null ref in SpatialManipulationReticle when multiple interactables are hovered. [PR #873](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/873)

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
