# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.3.0] - 2025-11-12

### Added

* Added a project validation rule to ensure the Unity XR Hands subsystem is enabled in the OpenXR settings when the corresponding MRTK subsystem is enabled. [PR #973](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/973)
* Added support for Unity's com.unity.cloud.gltfast and com.unity.cloud.ktx packages when loading controller models. [PR #631](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/631)
* Added hand tracking permission for AndroidXR. [PR #982](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/982)
* Added toggle for frame rate independent smoothing in camera simulation. [PR #1011](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1011)
* Added implementation for the synthesized TriggerButton, accounting for animation smoothing. [PR #1043](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1043)
* Added a "squeeze" alias for the grip states, to account for broader input action mapping support. [PR #1043](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1043)

### Changed

* Updated hands subsystem names for clarity. [PR #995](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/995)
* Remapped the synthetic hands config to read the float "select value" action instead of the bool "select" action, since it's read as a float. [PR #1043](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1043)
* Updated tests to follow existing MRTK test patterns. [PR #1046](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1046)

### Fixed

* Fixed controller model fallback visualization becoming stuck visible when hands became tracked after initialization. [PR #984](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/984)
* Updated dependencies to match Unity Asset Store packages. [PR #1054](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1054)
  * com.microsoft.mrtk.graphicstools.unity 0.8.0
  * org.mixedrealitytoolkit.core 3.2.2

### Changed


## [3.2.2] - 2024-09-18

### Fixed

* InputSimulator execution order so that it executes before InteractionManager.
* Ensure all relevant interactor types show up in InteractionModeManager's dropdowns. [PR #872](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/872)

## [3.2.1] - 2024-06-12

### Fixed

* Fixed missing [CanEditMultipleObject] attributes as per Bug 573 [PR #698](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/698)
* Add logic to account for a bound but untracked interaction profile. [PR #704](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/704)
* Reduced package description to support for UPM package publishing in the Unity Asset Store.
* Ensures the simulated input sources hold their state (including gestures) when their toggle state is locked on. [PR #705](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/705)

## [3.2.0] - 2024-03-27

### Added

* Added an alternative Line of Sight (LOS), with angular offset, hand ray pose source. [PR #625](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/625)
* Added IsProximityHovered property of type TimedFlag to detect when a button starts being hovered or on interactor proximity and when it stops being hovered or on proximity of any interactor. [PR #611](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/611)

### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. [PR #519](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/519)
* Fix the fallback controllers being backwards [PR #636](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/636)
* Fix empty SpeechRecognitionKeyword breaking all speech keyword system [PR #612](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/612) [PR #614](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/614)
