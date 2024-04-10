// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Derived class which supports computing implicit rotations in full 3-DOF (6-DOF w/ position).
    /// </summary>
    /// <remarks>
    /// Whereas the base Orienter class uses the simplifying assumption of only adjusting yaw, that
    /// is rotation about the gravity vector Y-axis, the OrienterThreeBody computes an arbitrary 3-DOF
    /// rotation to align modeling space with the supplied physical reference points.
    /// Since at least three non-collinear points are necessary to compute such a rotation, until they
    /// are available, it falls back on a simplified computation. To summarize:
    /// 1) Zero points - identity transform
    /// 2) One point - position alignment only (identity rotation)
    /// 3) All points collinear - yaw and pitch about the line, but no roll about the line.
    /// 4) Non-collinear - 3-DOF alignment.
    /// </remarks>
    public class OrienterThreeBody : Orienter
    {

        /// <summary>
        /// Override to compute rotations unconstrained as a rotation about the gravity vector, Y-axis.
        /// </summary>
        /// <returns>True on success.</returns>
        /// <remarks>
        /// It takes at least 3 non-collinear points to imply a rotation.
        /// If there are fewer than that, this reverts back to the behavior of
        /// computing a rotation which pitches to align points but doesn't introduce roll.
        /// </remarks>
        protected override bool ComputeRotations()
        {
            bool haveNonZero = false;
            for (int i = 0; i < actives.Count; ++i)
            {
                for (int j = i + 1; j < actives.Count; ++j)
                {
                    for (int k = j + 1; k < actives.Count; ++k)
                    {
                        WeightedRotation wrotNew = ComputeRotation(actives[i].orientable, actives[j].orientable, actives[k].orientable);
                        if (wrotNew.weight > 0)
                        {
                            haveNonZero = true;
                            WeightedRotation wrot = actives[i];
                            wrot = AverageRotation(wrot, wrotNew);
                            actives[i] = wrot;
                            wrot = actives[j];
                            wrot = AverageRotation(wrot, wrotNew);
                            actives[j] = wrot;
                            wrot = actives[k];
                            wrot = AverageRotation(wrot, wrotNew);
                            actives[k] = wrot;
                        }
                    }
                }
            }
            if (!haveNonZero)
            {
                // This can happen if there aren't enough points, or they are all collinear.
                return base.ComputeRotations();
            }
            return true;
        }

        /// <summary>
        /// Compute yaw and pitch to align virtual line with physical.
        /// </summary>
        /// <param name="a">First point</param>
        /// <param name="b">Second point</param>
        /// <returns>Computed rotation weighted by inverse distance between points.</returns>
        protected override WeightedRotation ComputeRotation(IOrientable a, IOrientable b)
        {
            Vector3 lockedAtoB = b.LockedPosition - a.LockedPosition;
            lockedAtoB.Normalize();

            Vector3 virtualAtoB = b.ModelPosition - a.ModelPosition;
            virtualAtoB.Normalize();

            Quaternion rotVirtualFromLocked = Quaternion.FromToRotation(virtualAtoB, lockedAtoB);
            rotVirtualFromLocked.Normalize();

            float weight = (a.ModelPosition - b.ModelPosition).sqrMagnitude;
            float minDistSq = 0.0f;
            weight = weight > minDistSq ? 1.0f / weight : 1.0f;

            return new WeightedRotation()
            {
                orientable = null,
                rotation = rotVirtualFromLocked,
                weight = weight
            };
        }

        /// <summary>
        /// Compute 3-DOF rotation to align virtual to physical space.
        /// </summary>
        /// <param name="a">First point</param>
        /// <param name="b">Second point</param>
        /// <param name="c">Third point</param>
        /// <returns>Alignment rotation weighted by suitability of points.</returns>
        private WeightedRotation ComputeRotation(IOrientable a, IOrientable b, IOrientable c)
        {
            Vector3 lockedA = a.LockedPosition;
            Vector3 lockedB = b.LockedPosition;
            Vector3 lockedC = c.LockedPosition;

            Vector3 lockedBtoA = lockedA - lockedB;
            Vector3 lockedBtoC = lockedC - lockedB;

            float weight = ComputeWeight(a.ModelPosition, b.ModelPosition, c.ModelPosition);

            Quaternion rotVirtualFromLocked = Quaternion.identity;

            if (weight > 0)
            {
                Vector3 virtualBtoA = a.ModelPosition - b.ModelPosition;
                Vector3 virtualBtoC = c.ModelPosition - b.ModelPosition;

                // First compute a rotation aligning the virtual line A-B to the locked line A-B.
                Quaternion rotationFirst = Quaternion.FromToRotation(virtualBtoA, lockedBtoA);

                // Now compute a roll about that line which aligns the third virtual point C to locked C.
                virtualBtoC = rotationFirst * virtualBtoC;

                Vector3 dir = lockedBtoA;
                dir.Normalize();
                Vector3 up = Vector3.Cross(lockedBtoC, dir);
                up.Normalize();
                Vector3 right = Vector3.Cross(dir, up);

                float sinRads = Vector3.Dot(virtualBtoC, up);
                float cosRads = Vector3.Dot(virtualBtoC, right);

                float rotRads = Mathf.Atan2(sinRads, cosRads);

                Quaternion rotationSecond = Quaternion.AngleAxis(Mathf.Rad2Deg * rotRads, dir);

                rotVirtualFromLocked = rotationSecond * rotationFirst;

                rotVirtualFromLocked.Normalize();
            }

            return new WeightedRotation()
            {
                orientable = null,
                rotation = rotVirtualFromLocked,
                weight = weight
            };

        }

        /// <summary>
        /// Compute a weight reflecting the suitability of the input points for computing a 3-DOF rotation.
        /// </summary>
        /// <param name="modelA">The first point.</param>
        /// <param name="modelB">The second point.</param>
        /// <param name="modelC">The third point.</param>
        /// <returns>A weight in [0..1]. An unsuitable triplet will have weight == 0.</returns>
        /// <remarks>
        /// Preferred triplets of points will be near each other and have no acute angles.
        /// The heuristic for the weight tries to penalize triplets with either
        /// longer edges or more acute angles.
        /// The weight itself has no absolute meaning. But a more suitable triplet should have
        /// a greater weight than a less suitable triplet.
        /// </remarks>

        private float ComputeWeight(Vector3 modelA, Vector3 modelB, Vector3 modelC)
        {
            Vector3 vBA = modelA - modelB;
            Vector3 vCB = modelB - modelC;
            Vector3 vAC = modelC - modelA;
            float minDist = 0.01f; // a centimeter, really should be much further apart to be provide satisfactory results (like 10s of meters).
            float minEdgeLength = Mathf.Min(Mathf.Min(vBA.magnitude, vCB.magnitude), vAC.magnitude);
            if (minEdgeLength < minDist)
            {
                return 0.0f;
            }
            float maxEdgeLength = Mathf.Max(Mathf.Max(vBA.magnitude, vCB.magnitude), vAC.magnitude);
            float crossProd = Vector3.Cross(vBA.normalized, vCB.normalized).magnitude / (vBA.magnitude * vCB.magnitude);

            return crossProd / maxEdgeLength;
        }
    }
}