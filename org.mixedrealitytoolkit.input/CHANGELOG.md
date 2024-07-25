# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

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

## [3.2.2-development] - 2024-06-24

### Fixed

* InputSimulator execution order so that it executes before InteractionManager.
* Ensure all relevant interactor types show up in InteractionModeManager's dropdowns. [PR #872](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/872)

## [3.2.1] - 2024-04-23

### Fixed

* Fixed missing [CanEditMultipleObject] attributes as per Bug 573 [PR #698](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/698)
* Add logic to account for a bound but untracked interaction profile. [PR #704](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/704)
* Reduced package description to support for UPM package publishing in the Unity Asset Store.
* Ensures the simulated input sources hold their state (including gestures) when their toggle state is locked on. [PR #705](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/705)

## [3.2.0] - 2024-03-20

### Added

* Added an alternative Line of Sight (LOS), with angular offset, hand ray pose source. [PR #625](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/625)
* Added IsProximityHovered property of type TimedFlag to detect when a button starts being hovered or on interactor proximity and when it stops being hovered or on proximity of any interactor. [PR #611](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/611)

### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. [PR #519](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/519)
* Fix the fallback controllers being backwards [PR #636](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/636)
* Fix empty SpeechRecognitionKeyword breaking all speech keyword system [PR #612](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/612) [PR #614](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/pull/614)
