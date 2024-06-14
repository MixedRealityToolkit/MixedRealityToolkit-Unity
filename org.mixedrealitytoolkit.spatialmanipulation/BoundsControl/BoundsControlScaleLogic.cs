// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// This class defines the default manipulation logic for scaling handles of bounds control
    /// </summary>
    public class BoundsControlScaleLogic : ManipulationLogic<Vector3>
    {
        private BoundsControl boundsCont;
        private BoundsHandleInteractable currentHandle;
        private Vector3 initialGrabPoint;
        private MixedRealityTransform initialTransformOnGrabStart;
        private Vector3 diagonalDir;

        /// <inheritdoc />
        public override void Setup(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget)
        {
            base.Setup(interactors, interactable, currentTarget);
            currentHandle = interactable.transform.GetComponent<BoundsHandleInteractable>();
            boundsCont = currentHandle.BoundsControlRoot;
            initialGrabPoint = currentHandle.interactorsSelecting[0].GetAttachTransform(currentHandle).position;
            initialTransformOnGrabStart = new MixedRealityTransform(boundsCont.Target.transform);
            diagonalDir = (currentHandle.transform.position - boundsCont.OppositeCorner).normalized;
        }

        /// <inheritdoc />
        public override Vector3 Update(List<IXRSelectInteractor> interactors, IXRSelectInteractable interactable, MixedRealityTransform currentTarget, bool centeredAnchor)
        {
            base.Update(interactors, interactable, currentTarget, centeredAnchor);

            Vector3 anchorPoint = centeredAnchor ? boundsCont.Target.transform.TransformPoint(boundsCont.CurrentBounds.center) : boundsCont.OppositeCorner;
            Vector3 scaleFactor = boundsCont.Target.transform.localScale;
            Vector3 currentGrabPoint = currentHandle.interactorsSelecting[0].GetAttachTransform(currentHandle).position;

            if (boundsCont.ScaleBehavior == HandleScaleMode.Uniform)
            {
                float initialDist = Vector3.Dot(initialGrabPoint - anchorPoint, diagonalDir);
                float currentDist = Vector3.Dot(currentGrabPoint - anchorPoint, diagonalDir);
                float scaleFactorUniform = 1 + (currentDist - initialDist) / initialDist;
                scaleFactor = new Vector3(scaleFactorUniform, scaleFactorUniform, scaleFactorUniform);
            }
            else // non-uniform scaling
            {
                // get diff from center point of box
                Vector3 initialDist = boundsCont.Target.transform.InverseTransformVector(initialGrabPoint - anchorPoint);
                Vector3 currentDist = boundsCont.Target.transform.InverseTransformVector(currentGrabPoint - anchorPoint);
                Vector3 grabDiff = (currentDist - initialDist);

                scaleFactor = Vector3.one + grabDiff.Div(initialDist);
            }

            Vector3 newScale = initialTransformOnGrabStart.Scale.Mul(scaleFactor);
            return newScale;
        }
    }
}
