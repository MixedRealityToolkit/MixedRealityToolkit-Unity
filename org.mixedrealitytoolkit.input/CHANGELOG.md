# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

* Added support for Unity's com.unity.cloud.gltfast and com.unity.cloud.ktx packages when loading controller models. [PR #631](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/631)

### Fixed

* Fixed controller model fallback visualization becoming stuck visible when hands became tracked after initialization. [PR #984](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/984)

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
