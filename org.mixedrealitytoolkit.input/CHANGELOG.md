# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

* Added a project validation rule to ensure the Unity XR Hands subsystem is enabled in the OpenXR settings when the corresponding MRTK subsystem is enabled. [PR #973](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/973)
* Added support for Unity's com.unity.cloud.gltfast and com.unity.cloud.ktx packages when loading controller models. [PR #631](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/631)
* Added toggle for frame rate independent smoothing in camera simulation. [PR #1011](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1011)
* Added implementation for the synthesized TriggerButton, accounting for animation smoothing. [PR #1043](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1043)
* Added a "squeeze" alias for the grip states, to account for broader input action mapping support. [PR #1043](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1043)
* Added support for XR_MSFT_hand_tracking_mesh and XR_ANDROID_hand_mesh on compatible runtimes. [PR #993](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/993)

### Fixed

* Fixed controller model fallback visualization becoming stuck visible when hands became tracked after initialization. [PR #984](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/984)

### Changed

* Updated the minimum editor version to 2022.3.6f1 [PR #1003](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1003)
* Updated tests to follow existing MRTK test patterns. [PR #1046](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1046)
* Remapped the synthetic hands config to read the float "select value" action instead of the bool "select" action, since it's read as a float. [PR #1043](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1043)
* Split out mappings for "airtap" and "grab", as well as mapping other bespoke interaction profile actions (like those provided by the Hand Interaction Profile). [PR #1040](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1040)
* Updated tests to follow existing MRTK test patterns. [PR #1046](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1046)

### Removed

* Removed HandNode property and field from HandModel, as it was largely unused. [PR #1045](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1045)

### Deprecated

* Deprecated IHandedInteractor across the interactor implementations, as its info is now queryable directly from IXRInteractor's handedness property. [PR #1042](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/1042)

## [4.0.0-development.pre.1] - 2024-07-16

### Added

* SpatialMouseInputTestsForControllerlessRig Unity-tests.
* BasicInputTestsForControllerlessRig Unity-tests.
* Ported BasicInputTests so that they also test the new controllerless prefabs.
* Ported FuzzyGazeInteractorTests so that they also test the new controllerless prefabs.
* MRTKRayInteractorVisualsTestsForControllerlessRig Unity-tests.
* InteractionModeManagerTestsForControllerlessRig Unity-tests.
* FuzzyGazeInteractorTestsForControllerlessRig Unity-tests.
* TrackedPoseDriverLookup as the XRI3+ equivalent of ControllerLookup.
* TrackedPoseDriverWithFallback as the XRI3+ equivalent of ActionBasedControllerWithFallbacks.
* Controllerless version of MRTK XR Rig prefab.
* Controllerless version of MRTK LeftHand Controller prefab.
* Controllerless version of MRTK RightHand Controller prefab.
* Controllerless version of MRTK Gaze Controller prefab.
* Controllerless version of MRTK Interaction Manager prefab.
* Added ModeManagerdRoot field to interactors to hold a reference to parent GameObject.

### Changed

* Updated package com.unity.xr.interaction.toolkit to 3.0.4
* Updated BaseRuntimeInputTests logic to handle both deprecated XRController and new controllerless actions.
* Updated GazePinchInteractor logic to handle both deprecated XRController and new controllerless actions.
* Updated PokeInteractor logic to handle both deprecated XRController and new controllerless actions.
* Updated MRTKRayInteractor logic to handle both deprecated XRController and new controllerless actions.
* Updated FlatScreenModeDetector logic to handle both deprecated XRController and new controllerless actions.
* Updated ObjectManipulator so to not rely on obsolete XRI controllers.
* Moved the Gaze Interactor TrackedPoseDriver to parent GameObject so that all controller prefabs have the same structure.
* Moved HandModel script from Experimental\XRI3 to Controllers\
* Renamed MRTK XR Rig prefab as Obsolete MRTK XR Rig.
* Renamed MRTK LeftHand Controller prefab as Obsolete MRTK LeftHand Controller.
* Renamed MRTK RightHand Controller prefab as Obsolete MRTK RightHand Controller.
* Renamed MRTK Gaze Controller prefab as Obsolete MRTK Gaze Controller.
* Renamed MRTK Interaction Manager prefab as Obsolete MRTK Interaction Manager.
* Added ITrackedInteractor interface to GazePinchInteractor class.
* Added ITrackedInteractor interface to HandJointInteractor class.
* Added ITrackedInteractor interface to PokeInteractor class.
* Added ITrackedInteractor interface to MRTKRayInteractor class.
* Updated new controllerless rig to use HandPoseDrive and PinchInputReader to support devices without a Hand Interaction profile.

### Deprecated

* ActionBasedControllerWithFallbacks marked as Obsolete.
* ArticulatedHandController marked as Obsolete.

### Removed

* Removed obsolete ArticulatedHandController.HandsAggregatorSubsystem field.
* Removed obsolete MRTKRayInteractor.HandsAggregatorSubsystem field.
* Removed obsolete ControllerSimulationSettings.InputActionReference field.
* Removed obsolete SyntheticsHandsSubsystem::GetNeutralPose method.
* Removed obsolete SyntheticsHandsSubsystem::SetNeutralPose method.
* Removed obsolete SyntheticsHandsSubsystem::GetSelectionPose method.
* Removed obsolete SyntheticsHandsSubsystem::SetSelectionPose method.
* Removed obsolete SyntheticsHandsSubsystem::GetNeutralPose method.
* Removed obsolete SyntheticsHandsSubsystem::SetNeutralPose method.
* Removed obsolete FollowJoint.migratedSuccessfully field.
* Removed obsolete FollowJoint.hand field.
* Removed obsolete FollowJoint.Joint field.
* Removed obsolete FollowJoint::OnAfterDeserialize method.
* Removed obsolete HandBasedPoseSource.HandsAggregator field.
* Removed obsolete ControllerVisualizer.HandsAggregator field.
* Removed no longer needed Experimental\XRI3 folder

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
