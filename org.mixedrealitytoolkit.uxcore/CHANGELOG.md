# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).


## [3.3.0-development] - 2024-06-24

### Added

* Added automatic update for the `See It Say It Label` when the `SpeechRecognitionKeyword` of a `StatefulInteractable` has changed. Added ability to change the pattern, from inspector or code. When Unity Localization package is installed, a `LocalizedString` is used as pattern.  [PR #792](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/792)

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