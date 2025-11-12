# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

* Added input action focus handling to disable controller/hand tracked state when the application goes out of focus. [PR #1039](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1039)
* Added ToInteractorHandedness extension for XRNode. [PR #1042](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1042)

### Changed

* Updated the MRTK Default Profile to use the Unity XR Hands subsystem by default instead of the Microsoft OpenXR Plugin subsystem. [PR #973](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/973)
* Updated the minimum editor version to 2022.3.6f1 [PR #1003](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1003)

### Removed

* Removed `ITrackedInteractor`, as it was supporting an unused codepath and there are better ways to get this data (like querying the attach transform). [PR #1044](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1044)
* Removed `FindObjectUtility`, as it was a backwards-compatibility layer for pre-2021.3.18. Since our min version is now 2022.3, we can just call the API directly. [PR #1056](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1056)

### Deprecated

* Deprecated `IHandedInteractor`, as its info is now queryable directly from `IXRInteractor`'s handedness property. [PR #1042](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1042)
* Deprecated `EnumFlagsAttribute` in favor of `Unity.XR.CoreUtils.GUI.FlagsPropertyAttribute`. [PR #1075](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1075)

## [4.0.0-development.pre.1] - 2024-07-09

### Added

* Added ITrackedInteractor to represent interactor with parent pose backed by a tracked input device.

### Changed

* Updated package com.unity.xr.interaction.toolkit to 3.0.4
* Updated InteractorHandednessExtensions.

### Removed

* Removed obsolete HandednessExtensions::IsRight method.
* Removed obsolete HandednessExtensions::IsLeft method.
* Removed obsolete HandsUtils::GetSubsystem method.
* Removed obsolete PlayspaceUtilities.ReferenceTransform field.
* Removed obsolete XRSubsystemHelpers::GetAllRunningSubsystemsNonAlloc method.

### Deprecated

* ControllerLookup marked as Obsolete.

## Unreleased 3.x

### Changed

* Updated tests to follow existing MRTK test patterns. [PR #1046](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1046)

### Added

* Added event `OnSpeechRecognitionKeywordChanged` to allow UI updates when the speech recognition keyword has changed. [PR #792](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/792/)

### Fixed

* Fixed broken project validation help link, for item 'MRTK3 profile may need to be assigned for the Standalone build target' (Issue #882) [PR #886](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/886)
* Fixed the "Is Interactable" convenience alias on StatefulInteractableEditor to allow multi-object editing in the Inspector to update all values. (Issue #573) [PR #943](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/943)
* Augment SerializableDictionary to allow temporary duplicates in Editor to prevent serialization errors. [PR #961](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/961)
* Fix an issue with the "Init Controllers" type lookup within InteractionModeManager.InitializeControllers() to find XRBaseControllers instead of XRControllers. [PR #961](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/961)

## [3.2.2] - 2024-09-18

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