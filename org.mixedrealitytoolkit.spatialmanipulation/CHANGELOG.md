# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

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
