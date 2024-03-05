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
        /// Used to keep track of the previously detected colliders+interactrables duples (CID) so that we can know which
        /// CID stopped being detected and update their buttons front plate RawImage.
        /// </summary>
        private HashSet<(Collider, IXRProximityInteractable)> previouslyDetectedColliderInteractableDuples = new HashSet<(Collider, IXRProximityInteractable)>();

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
            List<(Collider, IXRProximityInteractable)> currentlyDetectedCID = GetCurrentlyDetectedColliderInteractablesDuples();
            List<(Collider, IXRProximityInteractable)> noLongerDetectedCIDs = new List<(Collider, IXRProximityInteractable)>();

            foreach ((Collider collider, IXRProximityInteractable interactable) previouslyDetectedCID in previouslyDetectedColliderInteractableDuples)
            {
                if (currentlyDetectedCID.Contains(previouslyDetectedCID))
                {
                    noLongerDetectedCIDs.Add(previouslyDetectedCID);
                }
            }

            foreach ((Collider collider, IXRProximityInteractable interactable) noLongerDetectedCID in noLongerDetectedCIDs)
            {
                noLongerDetectedCID.interactable.OnProximityExited(new ProximityHoverExitedEventArgs(noLongerDetectedCID.collider, noLongerDetectedCID.interactable));
                currentlyDetectedCID.Remove(noLongerDetectedCID);
            }
        }

        /// <summary>
        /// Call OnProximityEntered for all colliders that are currently detected but were not detected previously.
        /// </summary>
        private void UpdateProximityEntered()
        {
            List<(Collider, IXRProximityInteractable)> currentlyDetectedCIDs = GetCurrentlyDetectedColliderInteractablesDuples();
            foreach ((Collider collider, IXRProximityInteractable interactable) colliderInteractableDuple in currentlyDetectedCIDs)
            {
                if (previouslyDetectedColliderInteractableDuples.Add(colliderInteractableDuple))
                {
                    colliderInteractableDuple.interactable.OnProximityEntered(new ProximityHoverEnteredEventArgs(colliderInteractableDuple.collider, colliderInteractableDuple.interactable));
                }
            }
        }

        /// <summary>
        /// Returns a hashset of all unique collider-interactable duples that are currently detected.
        /// </summary>
        /// <returns>Hashset with unique collider-interactable duples currently detected</returns>
        private List<(Collider, IXRProximityInteractable)> GetCurrentlyDetectedColliderInteractablesDuples()
        {
            List<(Collider, IXRProximityInteractable)> result = new List<(Collider, IXRProximityInteractable)>();

            foreach (Collider collider in DetectedColliders)
            {
                if (InteractionManager.TryGetInteractableForCollider(collider, out IXRInteractable xrInteractable) &&
                    xrInteractable is IXRProximityInteractable xrProximityInteractable &&
                    !result.Contains((collider, xrProximityInteractable)))
                {
                    {
                        result.Add((collider, xrProximityInteractable));
                    }
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
