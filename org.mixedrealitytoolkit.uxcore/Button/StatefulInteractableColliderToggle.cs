// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using Unity.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// The 'InteractableColliderToggle' class is responsible for managing the state of the collider 
    /// associated with a StatefulInteractable in the Unity UI. It enables or disables the collider based on 
    /// the state of the associated 'StatefulInteractable' object.
    /// </summary>
    [AddComponentMenu("MRTK/UX/Stateful Interactable Collider Toggle")]
    public class StatefulInteractableColliderToggle : UIBehaviour
    {       
        [SerializeField]
        [Tooltip("The StatefulInteractable to enable or disable the collider based on the Interactable's state.")]
        private StatefulInteractable statefulInteractable;

        /// <summary>
        /// The StatefulInteractable to enable or disable the collider based on the Interactable's state.
        /// </summary>
        public StatefulInteractable StatefulInteractable
        {
            get => statefulInteractable;
            set => statefulInteractable = value;
        }
        
        [SerializeField]
        [Tooltip("The collider to enable or disable based on the Interactable's state.")]
        private BoxCollider thisCollider;

        /// <summary>
        /// The collider to enable or disable based on the Interactable's state.
        /// </summary>
        public BoxCollider ThisCollider
        {
            get => thisCollider;
            set
            {
                if (thisCollider != value)
                {
                    thisCollider = value;
                    UpdateCollider();
                }
            }
        }

        [SerializeField]
        private RectTransformColliderFitter colliderFitter;

        public RectTransformColliderFitter ColliderFitter
        {
            get => colliderFitter;
            set 
            {
                if (colliderFitter != value)
                {
                    colliderFitter = value;
                    UpdateCollider();
                }
            }
        }

        protected virtual void OnEnable()
        {
            if (statefulInteractable != null)
            {
                statefulInteractable.OnDisabled.AddListener(OnInteractableDisabled);
                statefulInteractable.OnEnabled.AddListener(OnInteractableEnabled);
            }

            UpdateCollider();
        }

        protected virtual void OnDisable()
        {
            if (statefulInteractable != null)
            {
                statefulInteractable.OnDisabled.RemoveListener(OnInteractableDisabled);
                statefulInteractable.OnEnabled.RemoveListener(OnInteractableEnabled);
            }
        }

        protected virtual void OnInteractableDisabled() => UpdateCollider();

        protected virtual void OnInteractableEnabled() => UpdateCollider();

        private void UpdateCollider()
        {
            if (thisCollider != null)
            {
                thisCollider.enabled = statefulInteractable.enabled;

                if (colliderFitter != null)
                {
                    colliderFitter.CanToggleCollider = statefulInteractable.enabled;
                }
            }
        }
    }
}
