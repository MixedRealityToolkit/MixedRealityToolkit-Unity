// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// The <see cref="StatefulInteractableColliderToggle"/> class is responsible for managing the state of the collider 
    /// associated with a <see cref="StatefulInteractable"/>.
    /// </summary>
    /// <remarks>
    /// This class will enables or disables the colliders associated with <see cref="StatefulInteractable"/> based on
    /// the enabled state of <see cref="StatefulInteractable"/>.
    /// </summary>
    [AddComponentMenu("MRTK/UX/Stateful Interactable Collider Toggle")]
    public class StatefulInteractableColliderToggle : MonoBehaviour
    {       
        [SerializeField]
        [Tooltip("The StatefulInteractable to enable or disable the collider based on the interactable's enabled state.")]
        private StatefulInteractable statefulInteractable;

        /// <summary>
        /// The <see cref="StatefulInteractable"/> to enable or disable the collider based on the interactable's enabled state.
        /// </summary>
        public StatefulInteractable StatefulInteractable
        {
            get => statefulInteractable;
            set
            {
                if (statefulInteractable != value)
                {
                    RemoveStatefulInteractableEventHandlers();
                    statefulInteractable = value;
                    AddStatefulInteractableEventHandlers();
                    UpdateCollider();
                }
            }
        }

        [SerializeField]
        [Tooltip("The RectTransformColliderFitter that automatically disables the interactable's colliders based visibility.")]
        private RectTransformColliderFitter colliderFitter;

        /// <summary>
        /// The <see cref="RectTransformColliderFitter"/> that automatically disables the interactable's colliders based visibility.
        /// </summary>
        /// <remarks>
        /// The <see cref="StatefulInteractableColliderToggle"/> class will change <see cref="RectTransformColliderFitter.CanToggleCollider"/>
        /// based on the enabled state of <see cref="StatefulInteractable"/>.
        /// </remarks>
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

        /// <summary>
        /// A Unity event function that is called when the script component has been enabled.
        /// </summary> 
        protected virtual void OnEnable()
        {
            AddStatefulInteractableEventHandlers();
            UpdateCollider();
        }

        /// <summary>
        /// A Unity event function that is called when the script component has been disabled.
        /// </summary> 
        protected virtual void OnDisable()
        {
            RemoveStatefulInteractableEventHandlers();
        }

        /// <summary>
        /// Add event handlers to the <see cref="StatefulInteractable"/> to enable or disable the collider based on the interactable's enabled state.
        /// </summary>
        private void AddStatefulInteractableEventHandlers()
        {
            if (statefulInteractable != null)
            {
                statefulInteractable.OnDisabled.AddListener(OnInteractableDisabled);
                statefulInteractable.OnEnabled.AddListener(OnInteractableEnabled);
            }
        }

        /// <summary>
        /// Remove event handlers from the <see cref="StatefulInteractable"/> that enable or disable the collider based on the interactable's enabled state.
        /// </summary>
        private void RemoveStatefulInteractableEventHandlers()
        {
            if (statefulInteractable != null)
            {
                statefulInteractable.OnDisabled.RemoveListener(OnInteractableDisabled);
                statefulInteractable.OnEnabled.RemoveListener(OnInteractableEnabled);
            }
        }

        /// <summary>
        /// Function called when the interactable has been disabled.
        /// </summary>
        private void OnInteractableDisabled() => UpdateCollider();

        /// <summary>
        /// Function called when the interactable has been enabled.
        /// </summary>
        private void OnInteractableEnabled() => UpdateCollider();

        /// <summary>
        /// Update the interactable's collider's and the filler's enablement based on the interactable's enabled state.
        /// </summary>
        private void UpdateCollider()
        {
            if (statefulInteractable != null && statefulInteractable.colliders != null)
            {
                int colliderCount = statefulInteractable.colliders.Count;
                for (int i = 0; i < colliderCount; i++)
                {
                    var collider = statefulInteractable.colliders[i];
                    collider.enabled = statefulInteractable.enabled;
                }

                if (colliderFitter != null)
                {
                    colliderFitter.CanToggleCollider = statefulInteractable.enabled;
                }
            }
        }
    }
}
