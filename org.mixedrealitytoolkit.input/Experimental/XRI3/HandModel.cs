// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR;

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

        [SerializeField, Tooltip("The prefab of the MRTK Controller to show that will be automatically instantitated by this behaviour.")]
        private Transform modelPrefab;

        /// <summary>
        /// The prefab of the model to show that will be automatically instantitated by this <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public Transform ModelPrefab => modelPrefab;

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
            }
        }
    }
}
