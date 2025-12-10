// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Basic implementation of a <see cref="IInteractionModeDetector"/>,
    /// which reports the specified hover and select modes whenever the associated
    /// interactor has a valid hover or select target.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Interaction Detector")]
    public class InteractionDetector : MonoBehaviour, IInteractionModeDetector
    {
        [SerializeField]
        [Tooltip("The interactor to listen to.")]
        private XRBaseInteractor interactor;

        /// <summary>
        /// The interactor to listen to.
        /// </summary>
        public XRBaseInteractor Interactor
        {
            get => interactor;
            set => interactor = value;
        }

        [SerializeField]
        [Tooltip("Should this detector set a mode when the specified interactor has a hover target?")]
        private bool detectHover;

        /// <summary>
        /// Should this detector set a mode when the specified interactor has a hover target?
        /// </summary>
        public bool DetectHover
        {
            get => detectHover;
            set => detectHover = value;
        }

        [SerializeField]
        [FormerlySerializedAs("farHoverMode")]
        [Tooltip("The interaction mode to be set when the interactor is hovering over an interactable.")]
        private InteractionMode modeOnHover;

        /// <summary>
        /// The interaction mode to be set when the interactor is hovering over an interactable.
        /// </summary>
        public InteractionMode ModeOnHover
        {
            get => modeOnHover;
            set => modeOnHover = value;
        }

        [SerializeField]
        [Tooltip("Should this detector set a mode when the specified interactor has a selection?")]
        private bool detectSelect;

        /// <summary>
        /// Should this detector set a mode when the specified interactor has a selection?
        /// </summary>
        public bool DetectSelect
        {
            get => detectSelect;
            set => detectSelect = value;
        }

        [SerializeField]
        [FormerlySerializedAs("farSelectMode")]
        [Tooltip("The interaction mode to be set when the interactor is selecting an interactable.")]
        private InteractionMode modeOnSelect;

        /// <summary>
        /// The interaction mode to be set when the interactor is selecting an interactable.
        /// </summary>
        public InteractionMode ModeOnSelect
        {
            get => modeOnSelect;
            set => modeOnSelect = value;
        }

        /// <inheritdoc />
        public InteractionMode ModeOnDetection => interactor.hasSelection ? modeOnSelect : modeOnHover;

        [SerializeField]
        [FormerlySerializedAs("Controllers")]
        [FormerlySerializedAs("controllers")]
        [Tooltip("List of GameObjects which represent the interactor groups that this interaction mode detector has jurisdiction over. Interaction modes will be set on all specified groups.")]
        private List<GameObject> interactorGroups;

        /// <inheritdoc />
        [Obsolete("This function has been deprecated in version 4.0.0 and will be removed in a future version. Please use GetInteractorGroups instead.")]
        public List<GameObject> GetControllers() => GetInteractorGroups();

        /// <inheritdoc />
        public List<GameObject> GetInteractorGroups() => interactorGroups;

        /// <inheritdoc />
        public bool IsModeDetected()
        {
            bool isDetected = (interactor.hasHover && detectHover) || (interactor.hasSelection && detectSelect);

            if (interactor is XRRayInteractor rayInteractor)
            {
                isDetected |= rayInteractor.TryGetUIModel(out TrackedDeviceModel model) && ((model.currentRaycast.isValid && detectHover) || (model.select && detectSelect));
            }
            else if (interactor is NearFarInteractor nearFarInteractor)
            {
                isDetected |= nearFarInteractor.TryGetUIModel(out TrackedDeviceModel model) && ((model.currentRaycast.isValid && detectHover) || (model.select && detectSelect));
            }

            return isDetected;
        }
    }
}
