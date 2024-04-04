// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;


namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// A simple proxy interactor which will select and hover things on MRTK's behalf, for canvas input.
    /// </summary>
    [AddComponentMenu("MRTK/UX/Canvas Proxy Interactor")]
    public class CanvasProxyInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, IProxyInteractor, IModeManagedInteractor
    {
        /// <summary>
        /// The hash set containing a collection of valid interactable targets for this this interactor.
        /// </summary>
        protected HashSet<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable> validTargets = new HashSet<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable>();

        /// <summary>
        /// The last target selected using the <see cref="StartSelect(IXRSelectInteractable)"/> method. This value will
        /// be cleared when <see cref="EndSelect"/> is called.
        /// </summary>
        protected UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable manualSelectTarget;

        // We set this flag whenever we're cancelling an interaction. This will suppress
        // events (like OnClicked) on any StatefulInteractable.
        private bool isCancellingInteraction = false;

        /// <inheritdoc />
        public void StartHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable target)
        {
            StartHover(target, target.colliders[0].transform.position);
        }

        /// <inheritdoc />
        public void StartHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable target, Vector3 worldPosition)
        {
            if (target != null && target.IsHoverableBy(this))
            {
                transform.position = worldPosition;
                validTargets.Add(target);
            }
        }

        /// <inheritdoc />
        public void EndHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable target)
        {
            if (target != null)
            {
                validTargets.Remove(target);
            }
        }

        /// <inheritdoc />
        public void StartSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable target)
        {
            StartSelect(target, target.colliders[0].transform.position);
        }

        /// <inheritdoc />
        public void StartSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable target, Vector3 worldPosition)
        {
            if (interactionManager.IsRegistered(target) && target.IsSelectableBy(this))
            {
                // If we're already selecting something, end selection but suppress events.
                if (manualSelectTarget != null)
                {
                    isCancellingInteraction = true;
                    EndManualInteraction();
                    isCancellingInteraction = false;
                }

                transform.position = worldPosition;
                manualSelectTarget = target;
                StartManualInteraction(target);
            }
        }

        /// <inheritdoc />
        public void UpdateSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable, Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        /// <inheritdoc />
        public void EndSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable target, bool suppressEvents = false)
        {
            if (manualSelectTarget == target)
            {
                manualSelectTarget = null;

                if (suppressEvents)
                {
                    isCancellingInteraction = true;
                }

                EndManualInteraction();

                if (suppressEvents)
                {
                    isCancellingInteraction = false;
                }
            }
        }
        
        /// <inheritdoc />
        public override void GetValidTargets(List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable> targets)
        {
            targets.Clear();
            targets.AddRange(validTargets);
        }

        /// <inheritdoc />
        public override bool CanSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable)
        {
            return base.CanSelect(interactable) && interactable == manualSelectTarget;
        }

        /// <inheritdoc />
        public override bool isSelectActive
        {
            get
            {
                // If the base interactor class doesn't want to select, for
                // some reason. Shouldn't happen in most cases.
                if (!base.isSelectActive)
                    return false;

                // We use Start/EndManualInteraction to select our target.
                if (isPerformingManualInteraction)
                    return true;

                // No other way to select.
                return false;
            }
        }

        /// <inheritdoc />
        // We combine the base hoverActive with our flag for whether we're suppressing events.
        // Our interactors use isHoverActive = false to indicate interaction cancellation.
        public override bool isHoverActive => base.isHoverActive && !isCancellingInteraction;

        /// <inheritdoc />
        public GameObject GetModeManagedController() => gameObject;
    }
}
