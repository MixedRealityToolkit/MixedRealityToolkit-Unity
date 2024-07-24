// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Subsystems;
using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A Unity <see cref="ActionBasedController"/> for binding to hand input.
    /// </summary>
    /// <remarks>
    /// This is able to support variable pinch select through the use of <see cref="HandsAggregatorSubsystem"/>.
    /// </remarks>
    [AddComponentMenu("MRTK/Input/XR Controller (Articulated Hand)")]
    [Obsolete]
    public class ArticulatedHandController : ActionBasedController
    {
        #region Associated hand select values

        [SerializeField, Tooltip("The XRNode associated with this Hand Controller. Expected to be XRNode.LeftHand or XRNode.RightHand.")]
        private XRNode handNode;

        /// <summary>
        /// The XRNode associated with this Hand Controller.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public XRNode HandNode => handNode;

        /// <summary>
        /// Is the hand ready to select? Typically, this
        /// represents whether the hand is in a pinching pose,
        /// within the FOV set by the aggregator config.
        /// </summary>
        public bool PinchSelectReady => (currentControllerState is ArticulatedHandControllerState handControllerState) && handControllerState.PinchSelectReady;

        #endregion Associated hand select values

        #region Properties

        #endregion Properties

        private bool pinchedLastFrame = false;
        private bool isTrackingStatePolyfilled = false;

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            // Awake() override to prevent the base class
            // from using the base controller state instead of our
            // derived state. TODO: Brought up with Unity, may be
            // resolved in future XRI update.

            base.Awake();

            currentControllerState = new ArticulatedHandControllerState();
        }

        private static readonly ProfilerMarker UpdateTrackingInputPerfMarker =
            new ProfilerMarker("[MRTK] ArticulatedHandController.UpdateTrackingInput");

        /// <inheritdoc />
        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);

            using (UpdateTrackingInputPerfMarker.Auto())
            {
                if (controllerState == null)
                    return;

                // If we still don't have an aggregator, then don't update selects.
                if (XRSubsystemHelpers.HandsAggregator == null) { return; }

                bool gotPinchData = XRSubsystemHelpers.HandsAggregator.TryGetPinchProgress(
                    handNode,
                    out bool isPinchReady,
                    out bool isPinching,
                    out float pinchAmount
                );

                // If we got pinch data, write it into our select interaction state.
                if (gotPinchData)
                {
                    // Workaround for missing select actions on devices without interaction profiles
                    // for hands, such as Varjo and Quest. Should be removed once we have universal
                    // hand interaction profile(s) across vendors.

                    // Debounce the polyfill pinch action value.
                    bool isPinched = pinchAmount >= (pinchedLastFrame ? 0.9f : 1.0f);

                    // Inject our own polyfilled state into the Select state if no other control is bound.
                    if (!selectAction.action.HasAnyControls() || isTrackingStatePolyfilled)
                    {
                        controllerState.selectInteractionState.active = isPinched;
                        controllerState.selectInteractionState.activatedThisFrame = isPinched && !pinchedLastFrame;
                        controllerState.selectInteractionState.deactivatedThisFrame = !isPinched && pinchedLastFrame;
                    }

                    if (!selectActionValue.action.HasAnyControls() || isTrackingStatePolyfilled)
                    {
                        controllerState.selectInteractionState.value = pinchAmount;
                    }

                    // Also make sure we update the UI press state.
                    if (!uiPressAction.action.HasAnyControls() || isTrackingStatePolyfilled)
                    {
                        controllerState.uiPressInteractionState.active = isPinched;
                        controllerState.uiPressInteractionState.activatedThisFrame = isPinched && !pinchedLastFrame;
                        controllerState.uiPressInteractionState.deactivatedThisFrame = !isPinched && pinchedLastFrame;
                    }

                    if (!uiPressActionValue.action.HasAnyControls() || isTrackingStatePolyfilled)
                    {
                        controllerState.uiPressInteractionState.value = pinchAmount;
                    }

                    pinchedLastFrame = isPinched;
                }

                // Cast to expose hand state.
                if (controllerState is ArticulatedHandControllerState handControllerState)
                {
                    handControllerState.PinchSelectReady = isPinchReady;
                }
            }
        }

        /// <inheritdoc />
        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);

            // In case the position input action is not provided, we will try to polyfill it with the device position.
            // Should be removed once we have universal hand interaction profile(s) across vendors.

            // Check the tracking state here to account for a bound but untracked interaction profile.
            // This could show up on runtimes where a controller is disconnected, hand tracking spins up,
            // but the interaction profile is not cleared. This is allowed, per-spec: "The runtime may
            // return the last-known interaction profile in the event that no controllers are active."
            // Also check that the action was updated this frame by an active control, if is wasn't use polyfill pose.
            if ((!positionAction.action.HasAnyControls() || controllerState.inputTrackingState == InputTrackingState.None || positionAction.action.activeControl == null)
                && TryGetPolyfillDevicePose(out Pose devicePose))
            {
                controllerState.position = devicePose.position;
                controllerState.rotation = devicePose.rotation;
                
                // Polyfill the tracking state, too.
                controllerState.inputTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
                isTrackingStatePolyfilled = true;
            }
            else
            {
                isTrackingStatePolyfilled = false;
            }
        }

        private static readonly Quaternion rightPalmOffset = Quaternion.Inverse(
            new Quaternion(
                Mathf.Sqrt(0.125f),
                Mathf.Sqrt(0.125f),
                -Mathf.Sqrt(1.5f) / 2.0f,
                Mathf.Sqrt(1.5f) / 2.0f));
        private static readonly Quaternion leftPalmOffset = Quaternion.Inverse(
            new Quaternion(
                Mathf.Sqrt(0.125f),
                -Mathf.Sqrt(0.125f),
                Mathf.Sqrt(1.5f) / 2.0f,
                Mathf.Sqrt(1.5f) / 2.0f));

        // Workaround for missing device pose on devices without interaction profiles
        // for hands, such as Varjo and Quest. Should be removed once we have universal
        // hand interaction profile(s) across vendors.
        private bool TryGetPolyfillDevicePose(out Pose devicePose)
        {
            bool poseRetrieved = false;
            Handedness handedness = HandNode.ToHandedness();

            // palmPose retrieved in global space.
            if (XRSubsystemHelpers.HandsAggregator != null &&
                XRSubsystemHelpers.HandsAggregator.TryGetJoint(TrackedHandJoint.Palm, HandNode, out HandJointPose palmPose))
            {
                // XRControllers work in OpenXR scene-origin-space, so we need to transform
                // our global palm pose back into scene-origin-space.
                devicePose = PlayspaceUtilities.InverseTransformPose(palmPose.Pose);

                switch (handedness)
                {
                    case Handedness.Left:
                        devicePose.rotation *= leftPalmOffset;
                        poseRetrieved = true;
                        break;
                    case Handedness.Right:
                        devicePose.rotation *= rightPalmOffset;
                        poseRetrieved = true;
                        break;
                    default:
                        Debug.LogError("No polyfill available for device with handedness " + handedness);
                        devicePose = Pose.identity;
                        poseRetrieved = false;
                        break;
                };
            }
            else
            {
                devicePose = Pose.identity;
            }

            return poseRetrieved;
        }
    }
}
