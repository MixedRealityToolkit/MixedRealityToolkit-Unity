// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    internal class FlatScreenModeDetector : MonoBehaviour, IInteractionModeDetector
    {
        [SerializeField]
        [Tooltip("The interaction mode to be set when the interactor is hovering over an interactable.")]
        private InteractionMode flatScreenInteractionMode;

        [SerializeField]
        [Tooltip("List of XR Base Controllers that this interaction mode detector has jurisdiction over. Interaction modes will be set on all specified controllers.")]
        private List<GameObject> controllers;

        public InteractionMode ModeOnDetection => flatScreenInteractionMode;

        [Obsolete("Deprecated, please use MixedReality.Toolkit.Input.TrackedPoseDriverLookup instead.")]
        protected ControllerLookup controllerLookup = null;

        protected TrackedPoseDriverLookup trackedPoseDriverLookup = null;

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        protected void Awake()
        {
            controllerLookup = ComponentCache<ControllerLookup>.FindFirstActiveInstance();
            trackedPoseDriverLookup = ComponentCache<TrackedPoseDriverLookup>.FindFirstActiveInstance();
        }

        /// <inheritdoc />
        public List<GameObject> GetControllers() => controllers;

        public bool IsModeDetected()
        {
            // Flat screen mode is only active if the Left and Right Hand Controllers aren't being tracked
            #pragma warning disable CS0618 // Type or member is obsolete
            if (controllerLookup != null)
            {
                return !controllerLookup.LeftHandController.currentControllerState.inputTrackingState.HasPositionAndRotation() && !controllerLookup.RightHandController.currentControllerState.inputTrackingState.HasPositionAndRotation();
            }
            #pragma warning restore CS0618
            else if (trackedPoseDriverLookup != null &&
                     trackedPoseDriverLookup.LeftHandTrackedPoseDriver != null &&
                     trackedPoseDriverLookup.RightHandTrackedPoseDriver != null &&
                     trackedPoseDriverLookup.LeftHandTrackedPoseDriver.trackingStateInput != null &&
                     trackedPoseDriverLookup.RightHandTrackedPoseDriver.trackingStateInput != null &&
                     trackedPoseDriverLookup.LeftHandTrackedPoseDriver.trackingStateInput.action != null &&
                     trackedPoseDriverLookup.RightHandTrackedPoseDriver.trackingStateInput.action != null)
            {
                InputTrackingState leftHandInputTrackingState = (InputTrackingState)trackedPoseDriverLookup.LeftHandTrackedPoseDriver.trackingStateInput.action.ReadValue<int>();
                InputTrackingState rightHandInputTrackingState = (InputTrackingState)trackedPoseDriverLookup.RightHandTrackedPoseDriver.trackingStateInput.action.ReadValue<int>();
                return !leftHandInputTrackingState.HasPositionAndRotation() && !rightHandInputTrackingState.HasPositionAndRotation();
            }
            else
            {
                Debug.LogWarning("Neither controllerLookup nor trackedPoseDriverLookup are set, unable to detect mode.");
                return false;
            }
        }
    }
}
