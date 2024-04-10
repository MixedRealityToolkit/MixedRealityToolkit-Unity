// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_WSA && !UNITY_2020_1_OR_NEWER
#define WLT_ENABLE_LEGACY_WSA
#endif

using UnityEngine;
#if WLT_ENABLE_LEGACY_WSA
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
#endif // WLT_ENABLE_LEGACY_WSA

#pragma warning disable CS0618

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Wrapper class for Unity WorldAnchor, facilitating creation and persistence.
    /// </summary>
    public class SpongyAnchorWSA : SpongyAnchor
    {
        /// <summary>
        /// Timeout that protects against SpatialAnchor easing
        /// </summary>
        /// <remark>
        /// The Unity WorldAnchor component is based on the API property Windows.Perception.Spatial.SpatialAnchor.CoordinateSystem
        /// (see https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchor.coordinatesystem)
        /// 
        /// In contrast to its companion property RawCoordinateSystem, this value is smoothed out over a time of 300ms
        /// (determined experimentally) whenever the correct anchor position is re-established after a tracking loss.
        /// 
        /// Since Unity does not offer access to the raw value, we here introduce a delay after each time isLocated switches back
        /// to true to avoid feeding the FrozenWorld Engine with incorrect initial data.
        /// 
        /// Note: It would be worth trying direct access to SpatialAnchor is possible (COM-5081). First attempts
        /// failed to do this in some straightforward way from Unity-C# code. Further research would be required.
        /// </remark>
        public static float TrackingStartDelayTime = 0.3f;

        private float lastNotLocatedTime = float.NegativeInfinity;
#if WLT_ENABLE_LEGACY_WSA
        private WorldAnchor worldAnchor = null;
#else
        private SpongyAnchor dummy = null;
#endif // WLT_ENABLE_LEGACY_WSA

        /// <summary>
        /// Returns true if the anchor is reliably located. False might mean loss of tracking or not fully initialized.
        /// </summary>
        public override bool IsLocated =>
             IsReliablyLocated && Time.unscaledTime > lastNotLocatedTime + TrackingStartDelayTime;

        private bool IsReliablyLocated
        {
            get
            {
#if WLT_ENABLE_LEGACY_WSA
                return worldAnchor != null && worldAnchor.isLocated;
#else // WLT_ENABLE_LEGACY_WSA
                return false;
#endif // WLT_ENABLE_LEGACY_WSA
            }
        }

        public override Pose SpongyPose
        {
            get
            {
                return transform.GetGlobalPose();
            }
        }

        // Start is called before the first frame update
        private void Start ()
        {
#if WLT_ENABLE_LEGACY_WSA
            if (worldAnchor == null)
            {
                worldAnchor = gameObject.AddComponent<UnityEngine.XR.WSA.WorldAnchor>();
            }
#else
            dummy = this;
            if (IsSaved && lastNotLocatedTime > 0)
            {
                IsSaved = false;
                lastNotLocatedTime = float.NegativeInfinity;
            }
#endif // WLT_ENABLE_LEGACY_WSA
        }

#if WLT_ENABLE_LEGACY_WSA
        // Update is called once per frame
        private void Update ()
        {
            if (!IsReliablyLocated)
            {
                lastNotLocatedTime = Time.unscaledTime;
            }
        }
#endif // WLT_ENABLE_LEGACY_WSA

#if WLT_ENABLE_LEGACY_WSA

        /// <summary>
        /// Save to the anchor store, replacing any existing anchor (by name).
        /// </summary>
        /// <param name="store"></param>
        public void Save (WorldAnchorStore store)
        {
            if (IsSaved)
            {
                return;
            }
            if (worldAnchor == null)
            {
                return;
            }

            store.Delete(name);
            bool success = store.Save(name, worldAnchor);
            Debug.Assert(success);
            IsSaved = true;
        }

        /// <summary>
        /// Load from the anchor store, losing whatever state was before load.
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public bool Load (WorldAnchorStore store)
        {
            if (worldAnchor)
            {
                Destroy(worldAnchor);
            }
            worldAnchor = store.Load(name, gameObject);
            IsSaved = true;
            return worldAnchor != null;
        }
#endif // WLT_ENABLE_LEGACY_WSA
    }
}
