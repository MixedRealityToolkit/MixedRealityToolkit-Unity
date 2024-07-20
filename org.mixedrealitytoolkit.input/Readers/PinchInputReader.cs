// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A class that reads pinch selection input and values from <see cref="InputActionProperty"/>. If no action is set or if an action is not bound to a control,
    /// the selection state will be driven by the <see cref="MixedReality.Toolkit.Subsystems.IHandsAggregatorSubsystem"/> subsystem's pinch amount.
    /// </summary>
    /// <remarks>
    /// When using this class, ensure that the <see cref="MixedReality.Toolkit.Subsystems.IHandsAggregatorSubsystem"/> is available and enabled.
    /// This is a workaround for device's without interaction profiles for hands. Once universal hand interaction profiles are available,
    /// this class will be removed.
    /// </remarks>
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_XRInputDeviceButtonReader)]
    public class PinchInputReader : MonoBehaviour, IXRInputButtonReader
    {
        /// <summary>
        /// The state of the pinch input reader when <see cref="InputActionProperty"/> is not set or not bound to a control.
        /// </summary>
        private struct FallbackState
        {
            public bool hasPinchData;
            public bool isPerformed;
            public bool wasPerformedThisFrame;
            public bool wasCompletedThisFrame;
            public float value;
        }

        #region Serialized Fields

        [SerializeField, Tooltip("The XRNode associated with this Hand Controller. Expected to be XRNode.LeftHand or XRNode.RightHand.")]
        private XRNode handNode;

        /// <summary>
        /// The XRNode associated with this Hand Controller.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public XRNode HandNode => handNode;

        [SerializeField, Tooltip("The Input System action to use for selecting an Interactable. If not defined or if a controller is not attached to this property, the selection will be driven by the IHandsAggregatorSubsystem subsystem's pinch amount.")]
        private InputActionProperty selectAction = new InputActionProperty(new InputAction("Select", type: InputActionType.Button));

        /// <summary>
        /// The Input System action to use for selecting an Interactable. If not defined or if a controller is not attached
        /// to the action property, the selection will be driven by the <see cref="MixedReality.Toolkit.Subsystems.IHandsAggregatorSubsystem"/>
        /// subsystem's pinch amount.
        /// </summary>
        /// <remarks>
        /// Must be an action with a button-like interaction where phase equals performed when pressed.
        /// Typically a <see cref="ButtonControl"/> Control or a Value type action with a Press or Sector interaction.
        /// </remarks>
        /// <seealso cref="SelectActionValue"/>
        public InputActionProperty SelectAction
        {
            get => selectAction;
            set => SetInputActionProperty(ref selectAction, value);
        }

        [SerializeField, Tooltip("The Input System action to read values for selecting an Interactable. If not defined or if a controller is not attached to this property, the selection value will be driven by the IHandsAggregatorSubsystem subsystem's pinch amount.")]
        private InputActionProperty selectActionValue = new InputActionProperty(new InputAction("Select Value", expectedControlType: "Axis"));

        /// <summary>
        /// The Input System action to read values for selecting an Interactable. If not defined or if a controller is not attached
        /// to the action property, the selection will be driven by the <see cref="MixedReality.Toolkit.Subsystems.IHandsAggregatorSubsystem"/>
        /// subsystem's pinch amount.
        /// </summary>
        /// <remarks>
        /// Must be an <see cref="AxisControl"/> Control or <see cref="Vector2Control"/> Control.
        /// Optional, Unity uses <see cref="SelectAction"/> when not set.
        /// </summary>
        /// </remarks>
        /// <seealso cref="selectAction"/>
        public InputActionProperty SelectActionValue
        {
            get => selectActionValue;
            set => SetInputActionProperty(ref selectActionValue, value);
        }

        [SerializeField, Tooltip("The tracked pose driver used to determine if the select actions should be utilized or if selection should fallback to join positions from XRSubsystemHelpers.HandsAggregator.")]
        private TrackedPoseDriver trackedPoseDriver = null;

        /// <summary>
        /// The <see cref="TrackedPoseDriver"/> used to determine if the select actions should be utilized or if selection
        /// should fallback to join positions from XRSubsystemHelpers.HandsAggregator.
        /// </summary>
        public TrackedPoseDriver TrackedPoseDriver
        {
            get => trackedPoseDriver;
            set => trackedPoseDriver = value;
        }

        #endregion Serialized Fields

        #region Private Fields

        private FallbackState m_fallbackState;
        private bool m_firstUpdate = true;
        private bool m_isSelectionActionValidCache = false;
        private bool m_isSelectionActionValueValidCache = false; 

        private static readonly ProfilerMarker UpdatePinchSelectionPerfMarker =
            new ProfilerMarker("[MRTK] PinchInputReader.UpdatePinchSelection");

        #endregion Private Fields

        #region Unity Event Functions

        /// <summary>
        /// A Unity function event that is triggered when this behaviour is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            selectAction.EnableDirectAction();
            selectActionValue.EnableDirectAction();

            // Read current input values when becoming enabled,
            // but wait until after the input update so the input is read at a consistent time
            m_firstUpdate = true;

            // reset fallback state
            m_fallbackState = default;
        }

        /// <summary>
        /// A Unity function event that is triggered when this behaviour is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            selectAction.DisableDirectAction();
            selectActionValue.DisableDirectAction();
        }

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        protected virtual void Update()
        {
            if (m_firstUpdate)
            {
                UpdateActionValidCaches();
                m_firstUpdate = false;                
            }

            // Workaround for missing select actions on devices without interaction profiles
            // for hands, such as Varjo and Quest. Should be removed once we have universal
            // hand interaction profile(s) across vendors.
            if (!m_isSelectionActionValidCache || !m_isSelectionActionValueValidCache || GetIsPolyfillDevicePose())
            {
                UpdatePinchSelection();
            }
        }

        #endregion Unity Event Functions

        #region IXRInputButtonReader

        /// <inheritdoc />
        public bool ReadIsPerformed()
        {
            if (m_isSelectionActionValidCache && !GetIsPolyfillDevicePose())
            {
                var action = selectAction.action;
                var phase = action.phase;
                return phase == InputActionPhase.Performed || (phase != InputActionPhase.Disabled && action.WasPerformedThisFrame());
            }
            else
            {
                return m_fallbackState.isPerformed;
            }
        }

        /// <inheritdoc />
        public bool ReadWasPerformedThisFrame()
        {
            if (m_isSelectionActionValidCache && !GetIsPolyfillDevicePose())
            {
                return selectAction.action.WasPerformedThisFrame();
            }
            else
            {
                return m_fallbackState.wasPerformedThisFrame;
            }
        }

        /// <inheritdoc />
        public bool ReadWasCompletedThisFrame()
        {
            if (m_isSelectionActionValidCache && !GetIsPolyfillDevicePose())
            {
                return selectAction.action.WasCompletedThisFrame();
            }
            else
            {
                return m_fallbackState.wasCompletedThisFrame;
            }
        }

        /// <inheritdoc />
        public float ReadValue()
        {
            if (m_isSelectionActionValueValidCache && !GetIsPolyfillDevicePose())
            {
                return selectActionValue.action.ReadValue<float>();
            }
            else
            {
                return m_fallbackState.value;
            }
        }

        /// <inheritdoc />
        public bool TryReadValue(out float value)
        {
            if (m_isSelectionActionValueValidCache && !GetIsPolyfillDevicePose())
            {
                var action = selectActionValue.action;
                value = action.ReadValue<float>();
                return action.IsInProgress();
            }
            else
            {
                value = m_fallbackState.value;
                return m_fallbackState.hasPinchData;
            }
        }

        #endregion IXRInputButtonReader

        #region Private Functions
        /// <summary>
        /// Workaround for missing select actions on devices without interaction profiles for hands, such as Varjo and Quest.
        /// </summary>
        /// <remarks>
        /// This class should be removed once we have universal hand interaction profile(s) across vendors.
        /// </remarks>
        private void UpdatePinchSelection()
        {
            using (UpdatePinchSelectionPerfMarker.Auto())
            {
                // If we still don't have an aggregator, then don't update selects.
                if (XRSubsystemHelpers.HandsAggregator == null)
                {
                    return;
                }

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
                    bool isPinched = pinchAmount >= (m_fallbackState.isPerformed ? 0.9f : 1.0f);

                    m_fallbackState.wasPerformedThisFrame = isPinched && !m_fallbackState.isPerformed;
                    m_fallbackState.wasCompletedThisFrame = !isPinched && m_fallbackState.isPerformed;
                    m_fallbackState.isPerformed = isPinched;
                    m_fallbackState.value = pinchAmount;
                    m_fallbackState.hasPinchData = true;
                }
                else
                {
                    // If we didn't get pinch data, reset the fallback state.
                    m_fallbackState = default;
                }  
            }
        }

        /// <summary>
        /// Update the cached "is valid" states of the selection action and selection action value.
        /// </summary>
        private void UpdateActionValidCaches()
        {
            m_isSelectionActionValidCache = IsSelectionActionValid();
            m_isSelectionActionValueValidCache = IsSelectionActionValueValid();
        }

        /// <summary>
        /// Get if the selection action is attached to a control and the hand is being tracked. If not, the selection state is
        /// considered "polyfilled" and the HandsAggregator subsystem should be used to determine selection state.
        /// </summary>
        private bool IsSelectionActionValid()
        {
            return IsActionValid(selectAction.action);
        }

        /// <summary>
        /// Get if the selection action value is attached to a control and the hand is being tracked. If not, the selection state is
        /// considered "polyfilled" and the HandsAggregator subsystem should be used to determine selection value.
        /// </summary>
        private bool IsSelectionActionValueValid()
        {
            return IsActionValid(selectActionValue.action);
        }

        /// <summary>
        /// Get if the action value is attached to a control and the hand is being tracked. If not, the selection state is
        /// considered "polyfilled" and the HandsAggregator subsystem should be used to determine selection state and value.
        /// </summary>
        /// <remarks>
        /// We need to consider the fact that the action can be bound to a control, but the control may not be active even
        /// if the tracking state is valid. So we need to check if there's an active control before using the action.
        /// If there is no active control, this component will fallback to using the HandsAggregator subsystem to determine
        /// selection press and value.
        /// </remarks>
        private bool IsActionValid(InputAction action)
        {
            return action != null && action.HasAnyControls();
        }

        /// <summary>
        /// Check if the device pose is a polyfill device pose. If polyfill device pose is true,
        /// this means the pose is being driven by the HandsAggregator subsystem.
        /// </summary>
        private bool GetIsPolyfillDevicePose()
        {
            return trackedPoseDriver != null && trackedPoseDriver.GetIsPolyfillDevicePose();
        }

        /// <summary>
        /// Apply and enable the new action property if the application is running and this component is enabled.
        /// </summary>
        private void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
            {
                property.DisableDirectAction();
            }

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
            {
                property.EnableDirectAction();
            }
        }
        #endregion Private Functions
    }
}
