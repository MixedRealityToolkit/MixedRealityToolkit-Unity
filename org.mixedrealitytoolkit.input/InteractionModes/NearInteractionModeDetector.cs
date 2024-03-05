// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A <see cref="ProximityDetector"/> that will check if any near interactor is 
    /// selecting an interactable. If a near interactor is selecting an interactable,
    /// the specified <see cref="ProximityDetector.ModeOnDetection"/> will be marked
    /// as being detected.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Near Interaction Mode Detector")]
    public class NearInteractionModeDetector : ProximityDetector
    {
        /// <summary>
        /// The set of near interactables that belongs to near interaction
        /// </summary>
        [SerializeField]
        [Tooltip("The set of near interactables that belongs to near interaction")]
        private List<XRBaseInteractable> nearInteractables;

        /// <summary>
        /// Used to keep track of the previously detected interactrables so that we can know which
        /// interactable stopped being detected and update their buttons front plate RawImage.
        /// </summary>
        private HashSet<IXRProximityInteractable> previouslyDetectedColliderInteractableDuples = new();

        /// <inheritdoc />
        public override bool IsModeDetected()
        {
            bool result = base.IsModeDetected() || IsNearInteractorSelecting();
            if (result)
            {
                UpdateProximityExited();
                UpdateProximityEntered();
            }
            return result;
        }

        /// <summary>
        /// Calls OnProximityExited for all interactables that were previously ProximityHover detected but are no longer detected.
        /// </summary>
        private void UpdateProximityExited()
        {
            List<IXRProximityInteractable> currentlyDetectedInteractable = GetCurrentlyDetectedColliderInteractablesDuples();
            List<IXRProximityInteractable> noLongerDetectedInteractables = new();

            foreach (IXRProximityInteractable previouslyDetectedInteractable in previouslyDetectedColliderInteractableDuples)
            {
                if (!currentlyDetectedInteractable.Contains(previouslyDetectedInteractable))
                {
                    noLongerDetectedInteractables.Add(previouslyDetectedInteractable);
                }
            }

            foreach (IXRProximityInteractable noLongerDetectedInteractable in noLongerDetectedInteractables)
            {
                noLongerDetectedInteractable.OnProximityExited(new ProximityHoverExitedEventArgs(noLongerDetectedInteractable));
                currentlyDetectedInteractable.Remove(noLongerDetectedInteractable);
            }
        }

        /// <summary>
        /// Call OnProximityEntered for all colliders that are currently detected but were not detected previously.
        /// </summary>
        private void UpdateProximityEntered()
        {
            List<IXRProximityInteractable> currentlyDetectedInteractables = GetCurrentlyDetectedColliderInteractablesDuples();
            foreach (IXRProximityInteractable colliderInteractableDuple in currentlyDetectedInteractables)
            {
                if (previouslyDetectedColliderInteractableDuples.Add(colliderInteractableDuple))
                {
                    colliderInteractableDuple.OnProximityEntered(new ProximityHoverEnteredEventArgs(colliderInteractableDuple));
                }
            }
        }

        /// <summary>
        /// Returns a hashset of all unique interactables that are currently detected.
        /// </summary>
        /// <returns>Hashset with unique interactables currently detected</returns>
        private List<IXRProximityInteractable> GetCurrentlyDetectedColliderInteractablesDuples()
        {
            List<IXRProximityInteractable> result = new();

            foreach (Collider collider in DetectedColliders)
            {
                if (InteractionManager.TryGetInteractableForCollider(collider, out IXRInteractable xrInteractable) &&
                    xrInteractable is IXRProximityInteractable xrProximityInteractable &&
                    !result.Contains(xrProximityInteractable))
                {
                    result.Add(xrProximityInteractable);
                }
            }

            return result;
        }

        /// <summary>
        /// Indicates whether any near interactable has interactors selecting.
        /// </summary>
        /// <returns>True if an interactable has interactors selecting, false otherwise.</returns>
        private bool IsNearInteractorSelecting()
        {
            foreach (XRBaseInteractable nearInteractable in nearInteractables)
            {
                if (nearInteractable.interactorsSelecting.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
