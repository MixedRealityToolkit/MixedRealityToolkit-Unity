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
        /// The set of near interactors that belongs to near interaction
        /// </summary>
        [SerializeField]
        [Tooltip("The set of near interactors that belongs to near interaction")]
        private List<XRBaseInteractor> nearInteractors;

        /// <summary>
        /// Keeps track of the previously detected interactables so that we can know which
        /// interactable stopped being detected and trigger corresponding event.
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
                if (noLongerDetectedInteractable != null)
                {
                    noLongerDetectedInteractable.OnProximityExited(new ProximityExitedEventArgs(this));
                }
                previouslyDetectedInteractables.Remove(noLongerDetectedInteractable);
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
                    currentlyDetectedInteractable.OnProximityEntered(new ProximityEnteredEventArgs(this));
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
        /// Indicates if there is an interactor with selection.
        /// </summary>
        /// <returns>True if an interactor has selection, false otherwise.</returns>
        private bool IsNearInteractorSelecting()
        {
            foreach (XRBaseInteractor nearInteractor in nearInteractors)
            {
                if (nearInteractor.hasSelection)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
