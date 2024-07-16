// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// This component instantiates a model prefab for hand interactors.
    /// </summary>
    /// <remarks>
    /// This does not control the visibility of the instantiated models, the prefab is always created.
    /// </remarks>
    public class HandModel : MonoBehaviour
    {
        #region Properties

        [SerializeField, Tooltip("The prefab of the MRTK Controller to show that will be automatically instantiated by this behavior.")]
        private Transform modelPrefab;

        /// <summary>
        /// The prefab of the model to show that will be automatically instantiated by this <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public Transform ModelPrefab
        {
            get => modelPrefab;
            set => modelPrefab = value;
        }

        [SerializeField, Tooltip("The transform that is used as the parent for the model prefab when it is instantiated.  Will be set to a new child GameObject if None.")]
        private Transform modelParent;

        /// <summary>
        /// The <see cref="Transform"/> that is used as the parent for the model prefab when it is instantiated.  Will be set to a new child <see cref="GameObject"/> if None.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public Transform ModelParent => modelParent;

        [SerializeField, Tooltip("The instance of the controller model in the scene.  This can be set to an existing object instead of using Model Prefab.")]
        private Transform model;

        /// <summary>
        /// The instance of the model in the scene.  This can be set to an existing object instead of using Model Prefab.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public Transform Model => model;

        #endregion Properties

        #region Associated hand select values

        [SerializeField, Tooltip("The XRNode associated with this Hand Controller. Expected to be XRNode.LeftHand or XRNode.RightHand.")]
        private XRNode handNode;

        /// <summary>
        /// The <see cref="XRNode"/> associated with this Hand Model.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public XRNode HandNode => handNode;

        [SerializeField, Tooltip("The XRInputButtonReader representing selection values to be used by the hand model prefab when implementing ISelectInputVisualizer.")]
        private XRInputButtonReader selectInput;

        /// <summary>
        /// The <see cref="XRInputButtonReader"/> representing selection values to be used by
        /// the hand model prefab when implementing <see cref="ISelectInputVisualizer"/>.
        /// </summary>
        public XRInputButtonReader SelectInput => selectInput;
        
        #endregion Associated hand select values

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary>
        protected virtual void Start()
        {
            if (!HandNode.Equals(XRNode.LeftHand) && !HandNode.Equals(XRNode.RightHand))
            {
                Debug.LogWarning("HandNode is not set to XRNode.LeftHand or XRNode.RightHand. HandNode is expected to be XRNode.LeftHand or XRNode.RightHand.");
            }

            // Instantiate the model prefab if it is set
            if (ModelPrefab != null)
            {
                model = Instantiate(ModelPrefab, ModelParent);

                Debug.Assert(selectInput != null, $"The Select Input reader for {handNode} is not set and will not be used with the instantiated hand model.");

                // Set the select input reader for the model if it implements ISelectInputVisualizer
                if (selectInput != null && model != null && model.TryGetComponent(out ISelectInputVisualizer selectInputVisualizer))
                {
                    selectInputVisualizer.SelectInput = selectInput;
                }
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            // Create empty container transform for the model if none specified.
            // This is not strictly necessary to create since this GameObject could be used
            // as the parent for the instantiated prefab, but doing so anyway for backwards compatibility.
            if (modelParent == null)
            {
                modelParent = new GameObject($"[{gameObject.name}] Model Parent").transform;
                modelParent.SetParent(transform, false);
                modelParent.localPosition = Vector3.zero;
                modelParent.localRotation = Quaternion.identity;
            }
        }
    }
}
