using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// The Orienter class implements IOrienter.
    /// </summary>
    /// <remarks>
    /// It derives from MonoBehaviour only to facilitate assigning it
    /// in the Inspector. 
    /// Alternatively, it could be implemented as a singleton service. 
    /// There are pros and cons in either direction. The MonoBehaviour assigned in inspector
    /// was chosen to make explicit the dependency, rather than a dependency hidden by a
    /// static get internally.
    /// </remarks>
    public class Orienter : MonoBehaviour, IOrienter
    {
        #region Private members
        /// <summary>
        /// An object whose rotation needs to be computed, and the weight of its rotation.
        /// </summary>
        protected struct WeightedRotation
        {
            public IOrientable orientable;
            public float weight;
            public Quaternion rotation;

            public FragmentId FragmentId => orientable.FragmentId;
        }

        /// <summary>
        /// Registered orientables.
        /// </summary>
        private readonly List<IOrientable> orientables = new List<IOrientable>();

        /// <summary>
        /// Orientables in the currently processing fragment.
        /// </summary>
        protected readonly List<WeightedRotation> actives = new List<WeightedRotation>();

        /// <summary>
        /// Backing store for AlignmentManager property. If null, will use global AlignmentManager.
        /// </summary>
        private IAlignmentManager alignmentManager = null;
        #endregion Private members

        #region Unity overloads
        private void Start()
        {
            WorldLockingManager.GetInstance().FragmentManager.RegisterForRefitNotifications(OnRefit);
        }

        private void OnDestroy()
        {
            WorldLockingManager.GetInstance().FragmentManager.UnregisterForRefitNotifications(OnRefit);
        }

        #endregion Unity overloads

        #region Public APIs
        /// <inheritdocs />
        public IAlignmentManager AlignmentManager 
        { 
            get
            {
                return alignmentManager == null 
                    ? WorldLockingManager.GetInstance().AlignmentManager 
                    : alignmentManager;
            }
            set 
            { 
                alignmentManager = value; 
            } 
        }

        /// <inheritdocs />
        public void Register(IOrientable orientable)
        {
            int idx = orientables.FindIndex(o => o == orientable);
            if (idx < 0)
            {
                orientables.Add(orientable);
            }
        }

        /// <inheritdocs />
        public void Unregister(IOrientable orientable)
        {
            orientables.Remove(orientable);
        }

        /// <inheritdocs />
        public void Reorient(FragmentId fragmentId, IAlignmentManager mgr)
        {
            Debug.Assert(mgr == AlignmentManager);
            if (!InitRotations(fragmentId))
            {
                return;
            }
            if (!ComputeRotations())
            {
                return;
            }
            if (!SetRotations(mgr))
            {
                return;
            }
        }

        #endregion Public APIs

        #region Private implementations

        /// <summary>
        /// Adjust to refit operations. 
        /// </summary>
        /// <param name="mainId">The new combined fragment.</param>
        /// <param name="absorbedIds">Id's of other fragments being merged into mainId.</param>
        /// <remarks>
        /// This callback occurs *after* the refit operation. As part of the refit, 
        /// positions of the managed SpacePinOrientables may have changed, and therefore
        /// their implied orientations must be re-calculated.
        /// Note that there is an apparent race condition, as there is no order guarantee on
        /// the order of refit notifications, and the AlignmentManager also relies on the
        /// refit notification to adjust after refit operations. 
        /// However, both the Orienter and the AlignmentManager rely only on the positions
        /// having been set, which was accomplished during the refit and before the refit
        /// notification. So it really doesn't matter whether the Orienter.OnRefit or the 
        /// AlignmentManager.OnRefit is called first.
        /// </remarks>
        private void OnRefit(FragmentId mainId, FragmentId[] absorbedIds)
        {
            Reorient(mainId, AlignmentManager);
            AlignmentManager.SendAlignmentAnchors();
        }

        /// <summary>
        /// Collect all orientables in the current fragment for processing.
        /// </summary>
        /// <param name="fragmentId"></param>
        /// <returns></returns>
        private bool InitRotations(FragmentId fragmentId)
        {
            actives.Clear();
            for (int i = 0; i < orientables.Count; ++i)
            {
                if (orientables[i].FragmentId == fragmentId)
                {
                    actives.Add(
                        new WeightedRotation()
                        {
                            orientable = orientables[i],
                            weight = 0.0f,
                            /// Default rotation is current rotation. The inverse of the model rotation will
                            /// cancel out the model rotation when the coordinate system rotation is computed, 
                            /// using the unmodified locked rotation.
                            rotation = orientables[i].LockedRotation * Quaternion.Inverse(orientables[i].ModelRotation)
                        }
                    );
                }
            }
            return actives.Count > 0;
        }

        /// <summary>
        /// Compute rotations by pairs, weighting by distance and averaging for each orientable.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ComputeRotations()
        {
            for (int i = 0; i < actives.Count; ++i)
            {
                for (int j = i + 1; j < actives.Count; ++j)
                {
                    WeightedRotation wrotNew = ComputeRotation(actives[i].orientable, actives[j].orientable);
                    if (wrotNew.weight > 0)
                    {
                        WeightedRotation wrot = actives[i];
                        wrot = AverageRotation(wrot, wrotNew);
                        actives[i] = wrot;
                        wrot = actives[j];
                        wrot = AverageRotation(wrot, wrotNew);
                        actives[j] = wrot;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Compute the rotation that aligns a and b correctly in pinned space.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected virtual WeightedRotation ComputeRotation(IOrientable a, IOrientable b)
        {
            Vector3 lockedAtoB = b.LockedPosition - a.LockedPosition;
            lockedAtoB.y = 0.0f;
            lockedAtoB.Normalize();

            Vector3 virtualAtoB = b.ModelPosition - a.ModelPosition;
            virtualAtoB.y = 0.0f;
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
        /// Compute a new weighted rotation representing the two input weighted rotations.
        /// </summary>
        /// <param name="accum">The accumulator rotation.</param>
        /// <param name="add">The rotation to add in.</param>
        /// <returns>A new aggregate weighted rotation.</returns>
        protected WeightedRotation AverageRotation(WeightedRotation accum, WeightedRotation add)
        {
            float interp = add.weight / (accum.weight + add.weight);

            Quaternion combinedRot = Quaternion.Slerp(accum.rotation, add.rotation, interp);
            combinedRot.Normalize();

            float combinedWeight = accum.weight + add.weight;

            return new WeightedRotation()
            {
                orientable = accum.orientable,
                rotation = combinedRot,
                weight = combinedWeight
            };
        }

        /// <summary>
        /// Apply the computed rotations to the orientables.
        /// </summary>
        /// <param name="mgr">The alignment manager.</param>
        /// <returns>True on success.</returns>
        private bool SetRotations(IAlignmentManager mgr)
        {
            for (int i = 0; i < actives.Count; ++i)
            {
                actives[i].orientable.PushRotation(mgr, actives[i].rotation);
            }
            return true;
        }

        #endregion Private implementations
    }
}