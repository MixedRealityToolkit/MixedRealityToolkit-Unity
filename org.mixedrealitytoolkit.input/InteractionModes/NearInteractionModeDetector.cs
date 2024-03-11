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
        private readonly HashSet<IXRProximityInteractable> previouslyDetectedInteractables = new();

        /// <summary>
        /// Stores the currently detected interactables (updated every frame).
        /// </summary>
        private readonly List<IXRProximityInteractable> currentlyDetectedInteractables = new();

        /// <summary>
        /// Stores the interactables that are no longer detected (updated every frame).
        /// </summary>
        private readonly List<IXRProximityInteractable> noLongerDetectedInteractables = new();

        /// <inheritdoc />
        public override bool IsModeDetected()
        {
            bool result = base.IsModeDetected() || IsNearInteractorSelecting();
            if (result)
            {
                UpdateCurrentlyDetectedInteractables();
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
            noLongerDetectedInteractables.Clear();

            foreach (IXRProximityInteractable previouslyDetectedInteractable in previouslyDetectedInteractables)
            {
                if (!currentlyDetectedInteractables.Contains(previouslyDetectedInteractable))
                {
                    noLongerDetectedInteractables.Add(previouslyDetectedInteractable);
                }
            }

            foreach (IXRProximityInteractable noLongerDetectedInteractable in noLongerDetectedInteractables)
            {
                noLongerDetectedInteractable.OnProximityExited(new ProximityHoverExitedEventArgs(noLongerDetectedInteractable));
                currentlyDetectedInteractables.Remove(noLongerDetectedInteractable);
            }
        }

        /// <summary>
        /// Call OnProximityEntered for all interactables that are currently detected but were not detected previously.
        /// </summary>
        private void UpdateProximityEntered()
        {
            foreach (IXRProximityInteractable currentlyDetectedInteractable in currentlyDetectedInteractables)
            {
                if (previouslyDetectedInteractables.Add(currentlyDetectedInteractable))
                {
                    currentlyDetectedInteractable.OnProximityEntered(new ProximityHoverEnteredEventArgs(currentlyDetectedInteractable));
                }
            }
        }

        /// <summary>
        /// Returns a hashset of all unique interactables that are currently detected.
        /// </summary>
        /// <returns>Hashset with unique interactables currently detected</returns>
        private void UpdateCurrentlyDetectedInteractables()
        {
            currentlyDetectedInteractables.Clear();

            foreach (Collider collider in DetectedColliders)
            {
                if (InteractionManager.TryGetInteractableForCollider(collider, out IXRInteractable xrInteractable) &&
                    xrInteractable is IXRProximityInteractable xrProximityInteractable &&
                    !currentlyDetectedInteractables.Contains(xrProximityInteractable))
                {
                    currentlyDetectedInteractables.Add(xrProximityInteractable);
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
