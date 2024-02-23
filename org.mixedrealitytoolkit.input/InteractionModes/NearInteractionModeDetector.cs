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
        [SerializeField]
        [Tooltip("The set of near interactors that belongs to near interaction")]
        private List<XRBaseInteractor> nearInteractors;

        /// <summary>
        /// Used to keep track of the previously detected colliders so that we can know which
        /// colliders stopped being detected and update their buttons front plate and rounded
        /// rect if they are labeled as dynamic (based on proximity).
        /// </summary>
        List<Collider> previouslyDetectedColliders = new List<Collider>();

        /// <inheritdoc />
        public override bool IsModeDetected()
        {
            bool result = base.IsModeDetected() || IsNearInteractorSelecting();
            if (result)
            {
                for (int i = 0; i < previouslyDetectedColliders.Count; i++)
                {
                    Collider previouslyDetectedCollider = previouslyDetectedColliders[i];
                    if (!DetectedColliders.Contains(previouslyDetectedCollider) && previouslyDetectedCollider != null)
                    {
                        IXRProximityInteractable nearInteractionMode = previouslyDetectedCollider.GetComponent<IXRProximityInteractable>();
                        if (nearInteractionMode != null)
                        {
                            foreach (XRBaseInteractor xrBaseInteractor in nearInteractors)
                            {
                                previouslyDetectedCollider.GetComponent<IXRProximityInteractable>().OnProximityExited(new ProximityExitedEventArgs(previouslyDetectedCollider, xrBaseInteractor));
                            }
                        }
                        previouslyDetectedColliders.Remove(previouslyDetectedCollider);
                    }
                }
                foreach (Collider collider in DetectedColliders)
                {
                    if (!previouslyDetectedColliders.Contains(collider))
                    {
                        IXRProximityInteractable nearInteractionMode = collider.GetComponent<IXRProximityInteractable>();
                        if (nearInteractionMode != null)
                        {
                            foreach (XRBaseInteractor xrBaseInteractor in nearInteractors)
                            {
                                collider.GetComponent<IXRProximityInteractable>().OnProximityEntered(new ProximityEnteredEventArgs(collider, xrBaseInteractor));
                            }
                        }
                        previouslyDetectedColliders.Add(collider);
                    }
                }
            }
            return result;
        }

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
