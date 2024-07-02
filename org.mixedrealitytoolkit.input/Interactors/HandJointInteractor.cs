// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// An abstract XRDirectInteractor that represents a near interaction
    /// driven by articulated hand data. Implementers should define <see cref="TryGetInteractionPoint"/>
    /// to specify the desired interaction point, using the cached reference to
    /// the hands aggregator subsystem.
    /// </summary>
    public abstract class HandJointInteractor :
        XRDirectInteractor,
        IHandedInteractor,
        IModeManagedInteractor,
        ITrackedInteractor
    {
        #region Serialized Fields
        [SerializeField, Tooltip("Holds a reference to the <see cref=\"TrackedPoseDriver\"/> associated to this interactor if it exists.")]
        private TrackedPoseDriver trackedPoseDriver = null;

        /// <summary>
        /// Holds a reference to the <see cref="TrackedPoseDriver"/> associated to this interactor if it exists.
        /// </summary>
        protected internal TrackedPoseDriver TrackedPoseDriver => trackedPoseDriver;

        [SerializeField]
        [Tooltip("The root management GameObject that interactor belongs to. T")]
        private GameObject modeManagedRoot = null;

        /// <summary>
        /// Returns the GameObject that this interactor belongs to. This GameObject is governed by the
        /// interaction mode manager and is assigned an interaction mode. This GameObject represents the group that this interactor belongs to.
        /// </summary>
        /// <remarks>
        /// This will default to the GameObject that this attached to a parent <see cref="TrackedPoseDriver"/>.
        /// </remarks>
        public GameObject ModeManagedRoot
        {
            get => modeManagedRoot;
            set => modeManagedRoot = value;
        }
        #endregion Serialized Fields

        #region HandJointInteractor

        /// <summary>
        /// Concrete implementations should override this function to specify the point
        /// at which the interaction occurs. This would be the tip of the index finger
        /// for a poke interactor, or some other computed position from other data sources.
        /// </summary>
        protected abstract bool TryGetInteractionPoint(out Pose pose);

        #endregion HandJointInteractor

        #region ITrackedInteractor
        /// <inheritdoc />
        public GameObject TrackedParent => trackedPoseDriver == null ? null : trackedPoseDriver.gameObject;
        #endregion ITrackedInteractor

        #region IHandedInteractor

        /// <inheritdoc/>
        Handedness IHandedInteractor.Handedness
        {
            get => base.handedness.ToHandedness();
        }

        #endregion IHandedInteractor

        #region XRBaseInteractor

        /// <summary>
        /// Used to keep track of whether the `TrackedPoseDriver` or controller (if using deprecated XRI) has an interaction point.
        /// </summary>
        private bool interactionPointTracked;

        /// <summary>
        /// Indicates whether this Interactor is in a state where it could hover.
        /// </summary>
        public override bool isHoverActive
        {
            // Only be available for hovering if the `TrackedPoseDriver` or controller (if using deprecated XRI) pose driver is tracked or we have joint data.
            get
            {
                bool result = base.isHoverActive;

#pragma warning disable CS0618 // xrController is obsolete
                if (forceDeprecatedInput)
                {
                    result &= (xrController.currentControllerState.inputTrackingState.HasPositionAndRotation() || interactionPointTracked);
                }
#pragma warning restore CS0618 // xrController is obsolete
                else if (trackedPoseDriver != null)
                {
                    result &= (trackedPoseDriver.GetInputTrackingState().HasPositionAndRotation() || interactionPointTracked);
                }
                else
                {
                    result &= interactionPointTracked;
                }

                return result;
            }
        }

        #endregion XRBaseInteractor

        #region XRBaseInputInteractor

        private static readonly ProfilerMarker ProcessInteractorPerfMarker =
            new ProfilerMarker("[MRTK] HandJointInteractor.ProcessInteractor");

        /// <summary>
        /// Unity's <see href="https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.4/api/UnityEngine.XR.Interaction.Toolkit.XRInteractionManager.html">XRInteractionManager</see> 
        /// or containing <see href="https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.4/api/UnityEngine.XR.Interaction.Toolkit.IXRInteractionGroup.html">IXRInteractionGroup</see> 
        /// calls this method to update the Interactor before interaction events occur. See Unity's documentation for more information.
        /// </summary>
        /// <param name="updatePhase">The update phase this is called during.</param>
        /// <remarks>
        /// Please see the <see href="https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.4/api/UnityEngine.XR.Interaction.Toolkit.XRInteractionManager.html">XRInteractionManager</see> documentation for more
        /// details on update order.
        /// </remarks>
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            using (ProcessInteractorPerfMarker.Auto())
            {
                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                {
                    // Obtain near interaction point, and set our interactor's
                    // position/rotation to the interaction point's pose.
                    interactionPointTracked = TryGetInteractionPoint(out Pose interactionPose);
                    if (interactionPointTracked)
                    {
                        transform.SetPositionAndRotation(interactionPose.position, interactionPose.rotation);
                    }
                    else
                    {
                        // If we don't have a joint pose, reset to whatever our parent `TrackedPoseDriver` pose is.
                        transform.localPosition = Vector3.zero;
                        transform.localRotation = Quaternion.identity;
                    }

                    // Ensure that the attachTransform tightly follows the interactor's transform
                    attachTransform.SetPositionAndRotation(transform.position, transform.rotation);
                }
            }
        }
        #endregion XRBaseInputInteractor

        #region IModeManagedInteractor
        /// <inheritdoc/>
        [Obsolete("This function is obsolete and will be removed in the next major release. Use ModeManagedRoot instead.")]
        public GameObject GetModeManagedController()
        {
            // Legacy controller-based interactors should return null, so the legacy controller-based logic in the
            // interaction mode manager is used instead.
#pragma warning disable CS0618 // Type or member is obsolete 
            if (forceDeprecatedInput)
            {
                return null;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return ModeManagedRoot;
        }
        #endregion IModeManagedInteractor

        #region Unity Event Functions
        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            // Try to get the TrackedPoseDriver component from the parent if it hasn't been set yet
            if (trackedPoseDriver == null) 
            {
                trackedPoseDriver = GetComponentInParent<TrackedPoseDriver>();
            }

            // If mode managed root is not defined, default to the tracked pose driver's game object
            if (modeManagedRoot == null && trackedPoseDriver != null)
            {
                modeManagedRoot = trackedPoseDriver.gameObject;
            }
        }
        #endregion Unity Event Functions
    }
}
