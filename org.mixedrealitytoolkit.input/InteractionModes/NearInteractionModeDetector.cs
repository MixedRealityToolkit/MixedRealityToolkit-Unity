// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// This interface is used to update the front plate and rounded rect of a pressable
    /// button if they are flagged as dynamic (based on proximity), it is needed to prevent
    /// a circular reference between MRTK Input and MRTK UX Core Scripts packages.
    /// </summary>
    public interface IPressableButtonForNearInteractionModeDetector
    {
        public void UpdateFrontPlateAndRoundedRectIfDynamic(bool enable);
    }

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
                        previouslyDetectedCollider.GetComponent<IPressableButtonForNearInteractionModeDetector>()?.UpdateFrontPlateAndRoundedRectIfDynamic(false);
                        previouslyDetectedColliders.Remove(previouslyDetectedCollider);
                    }
                }
                foreach (Collider collider in DetectedColliders)
                {
                    if (!previouslyDetectedColliders.Contains(collider))
                    {
                        collider.GetComponent<IPressableButtonForNearInteractionModeDetector>()?.UpdateFrontPlateAndRoundedRectIfDynamic(true);
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
