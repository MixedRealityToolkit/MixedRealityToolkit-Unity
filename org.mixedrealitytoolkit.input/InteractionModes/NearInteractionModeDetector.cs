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
        /// Used to keep track of the previously detected colliders so that we can know which
        /// colliders stopped being detected and update their buttons front plate and rounded
        /// rect if they are labeled as dynamic (based on proximity).
        /// </summary>
        private List<Collider> previouslyDetectedColliders = new List<Collider>();

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
        /// Call OnProximityExited for all colliders that were previously detected but are no longer detected.  This
        /// effectively triggers OnProximityExited for all colliders that are no longer detected.
        /// </summary>
        private void UpdateProximityExited()
        {
            for (int i = 0; i < previouslyDetectedColliders.Count; i++)
            {
                Collider previouslyDetectedCollider = previouslyDetectedColliders[i];
                if (!DetectedColliders.Contains(previouslyDetectedCollider) &&
                    InteractionManager.TryGetInteractableForCollider(previouslyDetectedCollider, out IXRInteractable xrInteractable) &&
                    xrInteractable is IXRProximityInteractable xrProximityInteractable)
                {
                    foreach (XRBaseInteractable xrBaseInteractable in nearInteractables)
                    {
                        xrProximityInteractable.OnProximityExited(new ProximityExitedEventArgs(previouslyDetectedCollider, xrBaseInteractable));
                    }
                    previouslyDetectedColliders.Remove(previouslyDetectedCollider);
                }
            }
        }

        /// <summary>
        /// Call OnProximityEntered for all colliders that are currently detected but were not detected previously.  This
        /// effectively triggers OnProximityEntered for all colliders that are newly detected.
        /// </summary>
        private void UpdateProximityEntered()
        {
            foreach (Collider collider in DetectedColliders)
            {
                if (!previouslyDetectedColliders.Contains(collider) &&
                    InteractionManager.TryGetInteractableForCollider(collider, out IXRInteractable xrInteractable) &&
                    xrInteractable is IXRProximityInteractable xrProximityInteractable)
                {
                    foreach (XRBaseInteractable xrBaseInteractable in nearInteractables)
                    {
                        xrProximityInteractable.OnProximityEntered(new ProximityEnteredEventArgs(collider, xrBaseInteractable));
                    }
                    previouslyDetectedColliders.Add(collider);
                }
            }
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
