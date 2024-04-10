// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Wrapper class for a no-op spatial, platform-free anchor.
    /// </summary>
    public class SpongyAnchorNull : SpongyAnchor
    {
        public static float TrackingStartDelayTime  = 0.3f;

        private bool IsReliablyLocated
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true if the anchor is reliably located. False might mean loss of tracking or not fully initialized.
        /// </summary>
        public override bool IsLocated
        {
            get
            {
                return IsReliablyLocated;
            }
        }

        public override Pose SpongyPose
        {
            get
            {
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
        }

    }
}
