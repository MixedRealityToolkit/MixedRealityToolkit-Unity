// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !UNITY_2020_1_OR_NEWER

using UnityEngine;
#if WLT_ARFOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif // WLT_ARFOUNDATION_PRESENT

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Wrapper class for Unity WorldAnchor, facilitating creation and persistence.
    /// </summary>
    public class SpongyAnchorARF : SpongyAnchor
    {
        public static float TrackingStartDelayTime  = 0.3f;

#if WLT_ARFOUNDATION_PRESENT
        private ARReferencePoint arReferencePoint = null;
#endif // WLT_ARFOUNDATION_PRESENT

        private float lastNotLocatedTime = float.NegativeInfinity;

        private bool IsReliablyLocated
        {
            get
            {
#if WLT_ARFOUNDATION_PRESENT
#if WLT_EXTRA_LOGGING
                // mafinc - Rather than returning true if trackingState == Tracking, 
                // should this return true if trackingState != None?
                // I.e., do we consider Limited to be "reliable"?
                if (arReferencePoint != null)
                {
                    if (arReferencePoint.pending)
                    {
                        Debug.Log($"Anchor {name} is pending");
                    }
                    else if (arReferencePoint.trackingState != TrackingState.Limited)
                    {
                        Debug.Log($"Anchor {name} state is {(arReferencePoint == null ? "null" : arReferencePoint.trackingState.ToString())}");
                    }
                }
#endif // WLT_EXTRA_LOGGING
#if false
                return (arReferencePoint != null)
                    && !(arReferencePoint.pending)
                    && (arReferencePoint.trackingState == TrackingState.Tracking);
#else
                return (arReferencePoint != null)
                    && !(arReferencePoint.pending)
                    && (arReferencePoint.trackingState != TrackingState.None);
#endif
#else // WLT_ARFOUNDATION_PRESENT
                return false;
#endif // WLT_ARFOUNDATION_PRESENT
            }
        }

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
                if (WorldLockingManager.GetInstance().ApplyAdjustment)
                {
                    // Global space is frozen space. Transform into spongy space.
                    Pose frozenFromAnchor = transform.GetGlobalPose();
                    Pose spongyFromFrozen = WorldLockingManager.GetInstance().SpongyFromFrozen;
                    Pose spongyFromAnchor = spongyFromFrozen.Multiply(frozenFromAnchor);
                    return spongyFromAnchor;
                }
                // Global space is spongy space. Return global pose.
                return transform.GetGlobalPose();
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
#if WLT_ARFOUNDATION_PRESENT
            arReferencePoint = GetComponent<ARReferencePoint>();
#endif // WLT_ARFOUNDATION_PRESENT
            lastNotLocatedTime = Time.unscaledTime;
        }

#if WLT_ARFOUNDATION_PRESENT
        public void Cleanup(ARReferencePointManager arReferencePointManager)
        {
            if ((arReferencePointManager != null) && (arReferencePoint != null))
            {
                arReferencePointManager.RemoveReferencePoint(arReferencePoint);
                arReferencePoint = null;
            }
        }
#endif // WLT_ARFOUNDATION_PRESENT

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
#if WLT_EXTRA_LOGGING
            Debug.Log($"{name} - {transform.GetGlobalPose().position} - {SpongyPose.position}");
#endif // WLT_EXTRA_LOGGING
        }

    }
}

#endif // !UNITY_2020_1_OR_NEWER
