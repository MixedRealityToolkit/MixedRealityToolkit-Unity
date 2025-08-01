// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// An interactable for the handles of a <see cref="BoundsControl"/>.
    /// Scale handles subclass this to implement custom occlusion + reorientation logic.
    /// </summary>
    [AddComponentMenu("MRTK/Spatial Manipulation/Bounds Handle Interactable")]
    public class BoundsHandleInteractable : StatefulInteractable, ISnapInteractable, ISerializationCallbackReceiver
    {
        private BoundsControl boundsControlRoot;

        /// <summary>
        /// Reference to the BoundsControl that is associated with this handle.
        /// </summary>
        public BoundsControl BoundsControlRoot
        {
            get
            {
                if (boundsControlRoot == null)
                {
                    boundsControlRoot = transform.GetComponentInParent<BoundsControl>();
                }
                return boundsControlRoot;
            }
            set
            {
                boundsControlRoot = value;
            }
        }

        #region Bounds Handle Scaling

        [SerializeField]
        [Tooltip("How should the handle scale be maintained?")]
        private ScaleMaintainType scaleMaintainType = ScaleMaintainType.GlobalSize;

        // Properties applicable for advanced scale maintenance
        private float targetParentScale = 1f;

        [SerializeField]
        [Tooltip("Target lossy scale for the handle. Set value only applicable if ScaleAdjustType is Advanced.")]
        private float targetLossyScale = 2f;

        [SerializeField]
        [Tooltip("Minimum lossy scale for the handle. Only applicable if ScaleAdjustType is Advanced.")]
        private float minLossyScale = 1f;

        [SerializeField]
        [Tooltip("Maximum lossy scale for the handle. Only applicable if ScaleAdjustType is Advanced.")]
        private float maxLossyScale = 4f;

        #region Handling Obsolete Properties

        // A temporary variable used to migrate instances of BoundsHandleInteractable to use the scaleMaintainType property
        // instead of the serialized field maintainGlobalSize.
        // TODO: Remove this after some time to ensure users have successfully migrated.
        [SerializeField, HideInInspector]
        private bool migratedSuccessfully = false;

        [SerializeField, HideInInspector]
        private bool maintainGlobalSize = true;

        /// <summary>
        /// Should the handle maintain its global size, even as the object changes size?
        /// </summary>
        [Obsolete("This property has been deprecated in version 3.4.0. Use ScaleMaintainType instead.")]
        public bool MaintainGlobalSize
        {
            get => scaleMaintainType == ScaleMaintainType.GlobalSize;
            set => scaleMaintainType = value ? ScaleMaintainType.GlobalSize : ScaleMaintainType.FixedScale;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            // Only update the scaleMaintainType if it hasn't been set and the old property was not migrated yet
            if (!migratedSuccessfully && scaleMaintainType == ScaleMaintainType.GlobalSize)
            {
                scaleMaintainType = maintainGlobalSize ? ScaleMaintainType.GlobalSize : ScaleMaintainType.FixedScale;
                migratedSuccessfully = true;
            }
        }

        #endregion Handling Obsolete Properties

        #endregion Bounds Handle Scaling

        #region ISnapInteractable

        /// <inheritdoc />
        public Transform HandleTransform => transform;

        #endregion

        /// <summary>
        /// Is this handle currently occluded or hidden? Some handles
        /// are designed to occlude themselves in certain bounding box orientations and perspectives.
        /// </summary>
        /// <remarks>
        /// The "setter" for this is effectively processed in Update(), so that multiple per-frame
        /// calls to IsOccluded = true/false will not incur unnecessary expense.
        /// </remarks>
        public virtual bool IsOccluded { get; set; }

        /// <summary>
        /// The vector/direction along which the bounds should be flattened.
        /// Set by the box visuals script; it controls which handles are hidden
        /// when the bounds are flattened to a 2D/slate shape. Has no effect
        /// if/when IsFlattened is false!
        /// </summary>
        public Vector3 FlattenVector { get; set; }

        /// <summary>
        /// Whether the parent bounds is flattened or not. If true,
        /// FlattenVector is used to determine which axis to flatten along
        /// (and, accordingly, which handles to hide!)
        /// </summary>
        public bool IsFlattened { get; set; }

        [SerializeField]
        [Tooltip("The type of handle. Affects what the BoundsControl does when this handle is grabbed.")]
        private HandleType handleType;

        /// <summary>
        /// This handle's handle type.
        /// </summary>
        public HandleType HandleType { get => handleType; set => handleType = value; }

        private MeshRenderer handleRenderer;

        private bool wasOccludedLastFrame = false;

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            // Handles are never selected by poking.
            DisableInteractorType(typeof(IPokeInteractor));

            handleRenderer = GetComponentInChildren<MeshRenderer>();
        }

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary>
        public void Start()
        {
            if (scaleMaintainType != ScaleMaintainType.Advanced)
            {
                // Record initial values at Start(), so that we
                // capture the bounds sizing, etc.
                targetParentScale = transform.parent.lossyScale.MaxComponent();
                targetLossyScale = transform.localScale.MaxComponent();
            }
        }

        /// <summary>
        /// A Unity event function that is called every frame after normal update functions, if this object is enabled.
        /// </summary>
        protected virtual void LateUpdate()
        {
            // Do our IsOccluded "setter" in Update so we don't do this multiple times a frame.
            if (IsOccluded != wasOccludedLastFrame)
            {
                wasOccludedLastFrame = IsOccluded;
                if (handleRenderer != null)
                {
                    handleRenderer.enabled = !IsOccluded;
                }
                colliders[0].enabled = !IsOccluded;
            }

            // Maintain the aspect ratio/proportion of the handles based on scaleMaintainType.
            UpdateLocalScale();
        }

        protected virtual void UpdateLocalScale()
        {
            transform.localScale = Vector3.one;

            switch (scaleMaintainType)
            {
                case ScaleMaintainType.GlobalSize:
                    transform.localScale = GetLocalScale(targetLossyScale);
                    break;

                case ScaleMaintainType.FixedScale:
                    transform.localScale = GetLocalScale(targetLossyScale);

                    // If we don't want to maintain the overall *size*, we scale
                    // by the maximum component of the box so that the handles grow/shrink
                    // with the overall box manipulation.
                    if (targetParentScale != 0)
                    {
                        transform.localScale = transform.localScale * (transform.parent.lossyScale.MaxComponent() / targetParentScale);
                    }
                    break;

                case ScaleMaintainType.Advanced:
                    // Find the local scale that would result in the target lossy
                    // scale (the desired scale if the parent scale is 1)
                    Vector3 targetScale = GetLocalScale(targetLossyScale);
                    Vector3 minScale = GetLocalScale(minLossyScale);
                    Vector3 maxScale = GetLocalScale(maxLossyScale);

                    // We scale by the maximum component of the box so that 
                    // the handles grow/shrink with the overall box manipulation.
                    transform.localScale = targetScale * (transform.parent.lossyScale.MaxComponent() / targetParentScale);

                    // If this scale is greater than our desired lossy scale then clamp it to the max lossy scale
                    if (transform.lossyScale.MaxComponent() > maxLossyScale)
                    {
                        transform.localScale = maxScale;
                    }
                    // If this scale is less than our desired lossy scale then clamp it to the min lossy scale
                    else if (transform.lossyScale.MinComponent() < minLossyScale)
                    {
                        transform.localScale = minScale;
                    }
                    break;

                default:
                    break;
            }
        }

        // Returns the local scale this transform needs to have in order to have the desired lossy scale
        protected Vector3 GetLocalScale(float lossyScale) => new(
            transform.lossyScale.x == 0 ? transform.localScale.x : (lossyScale / transform.lossyScale.x),
            transform.lossyScale.y == 0 ? transform.localScale.y : (lossyScale / transform.lossyScale.y),
            transform.lossyScale.z == 0 ? transform.localScale.z : (lossyScale / transform.lossyScale.z)
            );


        /// <summary>
        /// Sets <see cref="IsOccluded"/> to true, and forces handling of occlusion immediately.
        /// </summary>
        internal void ForceOcclusion()
        {
            if (!wasOccludedLastFrame)
            {
                IsOccluded = true;
                wasOccludedLastFrame = true;
                if (handleRenderer != null)
                {
                    handleRenderer.enabled = false;
                }
                if (colliders.Count > 0 && colliders[0] != null)
                {
                    colliders[0].enabled = false;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            BoundsControlRoot.OnHandleSelectEntered(this, args);
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            BoundsControlRoot.OnHandleSelectExited(this, args);
        }
    }
}
