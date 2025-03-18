// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace MixedReality.Toolkit.Input
{
    public abstract class HandMeshVisualizer : MonoBehaviour, ISelectInputVisualizer
    {
        [SerializeField]
        [Tooltip("The XRNode on which this hand is located.")]
        private XRNode handNode = XRNode.LeftHand;

        /// <summary> The XRNode on which this hand is located. </summary>
        public XRNode HandNode { get => handNode; set => handNode = value; }

        [SerializeField]
        [Tooltip("When true, this visualizer will render rigged hands even on XR devices " +
                 "with transparent displays. When false, the rigged hands will only render " +
                 "on devices with opaque displays.")]
        private bool showHandsOnTransparentDisplays;

        /// <summary>
        /// When true, this visualizer will render rigged hands even on XR devices with transparent displays.
        /// When false, the rigged hands will only render on devices with opaque displays.
        /// Usually, it's recommended not to show hand visualization on transparent displays as it can
        /// distract from the user's real hands, and cause a "double image" effect that can be disconcerting.
        /// </summary>
        public bool ShowHandsOnTransparentDisplays
        {
            get => showHandsOnTransparentDisplays;
            set => showHandsOnTransparentDisplays = value;
        }

        [SerializeField]
        [Tooltip("Name of the shader property used to drive pinch-amount-based visual effects. " +
                 "Generally, maps to something like a glow or an outline color!")]
        private string pinchAmountMaterialProperty = "_PinchAmount";

        [SerializeField]
        [Tooltip("The input reader used when pinch selecting an interactable.")]
        private XRInputButtonReader selectInput = new XRInputButtonReader("Select");

        #region ISelectInputVisualizer implementation

        /// <summary>
        /// Input reader used when pinch selecting an interactable.
        /// </summary>
        public XRInputButtonReader SelectInput
        {
            get => selectInput;
            set => SetInputProperty(ref selectInput, value);
        }

        #endregion ISelectInputVisualizer implementation

        // The property block used to modify the pinch amount property on the material
        private MaterialPropertyBlock propertyBlock = null;

        // Scratch list for checking for the presence of display subsystems.
        private readonly List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();

        // The XRController that is used to determine the pinch strength (i.e., select value!)
        [Obsolete("This field has been deprecated in version 4.0.0 and will be removed in a future version. Use the SelectInput property instead.")]
        private XRBaseController controller;

        /// <summary>
        /// The list of button input readers used by this interactor. This interactor will automatically enable or disable direct actions
        /// if that mode is used during <see cref="OnEnable"/> and <see cref="OnDisable"/>.
        /// </summary>
        /// <seealso cref="XRInputButtonReader.EnableDirectActionIfModeUsed"/>
        /// <seealso cref="XRInputButtonReader.DisableDirectActionIfModeUsed"/>
        protected List<XRInputButtonReader> buttonReaders { get; } = new List<XRInputButtonReader>();

        /// <summary>
        /// Whether this visualizer currently has a loaded and visible hand mesh or not.
        /// </summary>
        protected internal bool IsRendering => HandRenderer != null && HandRenderer.enabled;

        /// <summary>
        /// The renderer for this visualizer, to use to visualize the pinch amount.
        /// </summary>
        protected abstract Renderer HandRenderer { get; }

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            buttonReaders.Add(selectInput);
        }

        /// <summary>
        /// A Unity event function that is called when the script component has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            buttonReaders.ForEach(reader => reader?.EnableDirectActionIfModeUsed());

            // Ensure hand is not visible until we can update position first time.
            HandRenderer.enabled = false;

            Debug.Assert(handNode == XRNode.LeftHand || handNode == XRNode.RightHand,
                $"HandVisualizer has an invalid XRNode ({handNode})!");
        }



        /// <summary>
        /// A Unity event function that is called when the script component has been disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            buttonReaders.ForEach(reader => reader?.DisableDirectActionIfModeUsed());

            // Disable the rigged hand renderer when this component is disabled
            HandRenderer.enabled = false;
        }

        /// <summary>
        /// Helper method for setting an input property.
        /// </summary>
        /// <param name="property">The <see langword="ref"/> to the field.</param>
        /// <param name="value">The new value being set.</param>
        /// <remarks>
        /// If the application is playing, this method will also enable or disable directly embedded input actions
        /// serialized by the input if that mode is used. It will also add or remove the input from the list of button inputs
        /// to automatically manage enabling and disabling direct actions with this behavior.
        /// </remarks>
        /// <seealso cref="buttonReaders"/>
        protected void SetInputProperty(ref XRInputButtonReader property, XRInputButtonReader value)
        {
            if (value == null)
            {
                Debug.LogError("Setting XRInputButtonReader property to null is disallowed and has therefore been ignored.");
                return;
            }

            if (Application.isPlaying && property != null)
            {
                buttonReaders?.Remove(property);
                property.DisableDirectActionIfModeUsed();
            }

            property = value;

            if (Application.isPlaying)
            {
                buttonReaders?.Add(property);
                if (isActiveAndEnabled)
                {
                    property.EnableDirectActionIfModeUsed();
                }
            }
        }

        protected virtual bool ShouldRenderHand()
        {
            if (displaySubsystems.Count == 0)
            {
                SubsystemManager.GetSubsystems(displaySubsystems);
            }

            // Are we running on an XR display and it happens to be transparent?
            // Probably shouldn't be showing rigged hands! (Users can
            // specify showHandsOnTransparentDisplays if they disagree.)
            if (displaySubsystems.Count > 0 &&
                displaySubsystems[0].running &&
                !displaySubsystems[0].displayOpaque &&
                !showHandsOnTransparentDisplays)
            {
                return false;
            }

            // All checks out!
            return true;
        }

        protected virtual void UpdateHandMaterial()
        {
            if (HandRenderer == null)
            {
                return;
            }

            // Update the hand material
            float pinchAmount = TryGetSelectionValue(out float selectionValue) ? Mathf.Pow(selectionValue, 2.0f) : 0;
            HandRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(pinchAmountMaterialProperty, pinchAmount);
            HandRenderer.SetPropertyBlock(propertyBlock);
        }

        /// <summary>
        /// Try to obtain the tracked devices selection value from the provided input reader.
        /// </summary>
        /// <remarks>
        /// For backwards compatibility, this method will also attempt to get the selection amount from a
        /// legacy XRI controller if the input reader is not set.
        /// </remaks>
        private bool TryGetSelectionValue(out float value)
        {
            if (selectInput != null && selectInput.TryReadValue(out value))
            {
                return true;
            }

            bool success = false;
            value = 0.0f;

#pragma warning disable CS0618 // XRBaseController is obsolete
            if (controller == null)
            {
                controller = GetComponentInParent<XRBaseController>();
            }
            if (controller != null)
            {
                value = controller.selectInteractionState.value;
                success = true;
            }
#pragma warning restore CS0618 // XRBaseController is obsolete

            return success;
        }
    }
}
