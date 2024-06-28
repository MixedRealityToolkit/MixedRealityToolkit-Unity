// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Serialization;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    internal class FlatScreenModeDetector : MonoBehaviour, IInteractionModeDetector
    {
        [SerializeField]
        [Tooltip("The interaction mode to be set when the interactor is hovering over an interactable.")]
        private InteractionMode flatScreenInteractionMode;

        [SerializeField]
        [FormerlySerializedAs("controllers")]
        [Tooltip("List of XR Base interactor groups that this interaction mode detector has jurisdiction over. Interaction modes will be set on all specified groups.")]
        private List<GameObject> interactorGroups;

        public InteractionMode ModeOnDetection => flatScreenInteractionMode;

        [Obsolete("Deprecated, please use MixedReality.Toolkit.Input.TrackedPoseDriverLookup instead.")]
        protected ControllerLookup controllerLookup = null;

        protected TrackedPoseDriverLookup trackedPoseDriverLookup = null;

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        protected void Awake()
        {
#pragma warning disable CS0618 // ControllerLookup is obsolete
            controllerLookup = ComponentCache<ControllerLookup>.FindFirstActiveInstance();
#pragma warning restore CS0618 // ControllerLookup is obsolete

            trackedPoseDriverLookup = ComponentCache<TrackedPoseDriverLookup>.FindFirstActiveInstance();
        }

        /// <inheritdoc /> 
        [Obsolete("This function is obsolete and will be removed in a future version. Please use GetInteractorGroups instead.")]
        public List<GameObject> GetControllers() => GetInteractorGroups();

        /// <inheritdoc /> 
        public List<GameObject> GetInteractorGroups() => interactorGroups;

        public bool IsModeDetected()
        {
            // Flat screen mode is only active if the Left and Right Hands aren't being tracked
            #pragma warning disable CS0618 // Type or member is obsolete
            if (controllerLookup != null)
            {
                return !controllerLookup.LeftHandController.currentControllerState.inputTrackingState.HasPositionAndRotation() && !controllerLookup.RightHandController.currentControllerState.inputTrackingState.HasPositionAndRotation();
            }
            #pragma warning restore CS0618
            else if (trackedPoseDriverLookup != null)
            {
                return !trackedPoseDriverLookup.LeftHandTrackedPoseDriver.GetInputTrackingState().HasPositionAndRotation() &&
                    !trackedPoseDriverLookup.RightHandTrackedPoseDriver.GetInputTrackingState().HasPositionAndRotation();
            }
            else
            {
                Debug.LogWarning("Neither controllerLookup nor trackedPoseDriverLookup are set, unable to detect mode.");
                return false;
            }
        }
    }
}
