// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//#define WLT_EXTRA_LOGGING

#if WLT_DISABLE_LOGGING
#undef WLT_EXTRA_LOGGING
#endif // WLT_DISABLE_LOGGING

using UnityEngine;
#if WLT_ARSUBSYSTEMS_PRESENT
using UnityEngine.XR.ARSubsystems;
#endif // WLT_ARSUBSYSTEMS_PRESENT

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Wrapper class for Unity XRAnchor, facilitating creation and persistence.
    /// </summary>
    public class SpongyAnchorXR : SpongyAnchor
    {
        public static float TrackingStartDelayTime  = 0.3f;

        private float lastNotLocatedTime = float.NegativeInfinity;

#if WLT_ARSUBSYSTEMS_PRESENT
        private TrackableId trackableId = TrackableId.invalidId;

        public TrackableId TrackableId { get { return trackableId; } set { trackableId = value; } }
#endif // WLT_ARSUBSYSTEMS_PRESENT

        /// <summary>
        /// Whether the anchor is being tracked reliably. 
        /// </summary>
        /// <remarks>
        /// This state is managed by the anchor manager.
        /// </remarks>
        public bool IsReliablyLocated { get; set; }

        /// <summary>
        /// Returns true if the anchor is reliably located. False might mean loss of tracking or not fully initialized.
        /// </summary>
        public override bool IsLocated
        {
            get
            {
#if WLT_EXTRA_LOGGING
                if (IsReliablyLocated && !(Time.unscaledTime > lastNotLocatedTime + TrackingStartDelayTime))
                {
                    Debug.Log($"Anchor {name} located but waiting TrackingStartDelayTime {Time.unscaledTime} > {lastNotLocatedTime} + {TrackingStartDelayTime}");
                }
#endif // WLT_EXTRA_LOGGING
                return IsReliablyLocated && Time.unscaledTime > lastNotLocatedTime + TrackingStartDelayTime;
            }
        }

        public override Pose SpongyPose
        {
            get
            {
                Pose spongyPose = transform.GetGlobalPose();
                return spongyPose;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mgr"></param>
        /// <returns></returns>
        /// <remarks>
        /// The ARReferencePoint (or later version ARAnchor) returned by mgr.AddReferencePoint() (later mgr.AddAnchor())
        /// is a Unity component, attached (obviously) to a GameObject.
        /// So, an alternate (read better) way might be:
        ///  AnchorManagerARF creates an ARReferencePoint(ARAnchor).
        ///  It then adds a SpongyAnchorARF (while creating it with AddComponent) to the referencePoint's gameObject.
        ///  SpongyAnchorARF then gets ref to referencePoint in Start (or Awake?) to implement IsLocated
        ///  (and hopefully Save/Load later).
        ///  There is a PROBLEM HERE:
        ///  ARReferencePoints (ARAnchors) can't be destroyed via normal Unity destruction path. They must
        ///  be explicitly destroyed via the ARReferencePointManager.RemoveReferencePoint().
        ///  The good news is that we are in control of the destruction of SpongyAnchors. So rather
        ///  than destroying a SpongyAnchor by destroying its GameObject, as we do now (see AnchorManager.Reset() 
        ///  for example, plus other Destroy() calls in AnchorManager), we'll need a virtual specifically for
        ///  destroying SpongyAnchors.
        /// </remarks>

        // Start is called before the first frame update
        private void Start ()
        {
            lastNotLocatedTime = Time.unscaledTime;
        }

        // mafinc - This seems a wasteful use of Update. Possibly set lastNotLocatedTime while iterating
        // in AnchorManager?
        private void Update ()
        {
            /// Set lastNotLocatedTime if not located
            if (!IsReliablyLocated)
            {
#if WLT_EXTRA_LOGGING
                Debug.Log($"LastNotLocated {name} is {Time.unscaledTime}");
#endif // WLT_EXTRA_LOGGING
                lastNotLocatedTime = Time.unscaledTime;
            }
        }

    }
}
