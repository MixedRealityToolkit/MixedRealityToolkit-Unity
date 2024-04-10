// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    public delegate void PostAlignmentLoadedDelegate();

    /// <summary>
    /// Manage pinning the world-locked space to user defined coordinate frame
    /// at a discrete set of points in the world.
    /// </summary>
    public interface IAlignmentManager
    {
        /// <summary>
        /// The pose to insert into the camera's hierarchy above the WorldLocking Adjustment transform (if any).
        /// </summary>
        Pose PinnedFromLocked { get; }

        /// <summary>
        /// Update based on new head position.
        /// </summary>
        /// <param name="lockedHeadPose">Head pose in world locked space.</param>
        void ComputePinnedPose(Pose lockedHeadPose);

        /// <summary>
        /// Register for notification that pin data has finished loaded and is available.
        /// </summary>
        /// <param name="del">Delegate to notify.</param>
        /// <remarks>
        /// The delegate should be unregistered when no longer needed, e.g. owning object is destroyed.
        /// </remarks>
        void RegisterForLoad(PostAlignmentLoadedDelegate del);

        /// <summary>
        /// Unregister for notification that pin data has finished loaded and is available.
        /// </summary>
        /// <param name="del">Delegate to remove from notifications.</param>
        void UnregisterForLoad(PostAlignmentLoadedDelegate del);

        /// <summary>
        /// New triangulation was built based upon recent poses.
        /// </summary>
        event EventHandler<Triangulator.ITriangulator> OnTriangulationBuilt;

        /// <summary>
        /// Add an anchor for aligning a virtual pose to a pose in real space. 
        /// </summary>
        /// <param name="virtualPose">The pose in modeling space.</param>
        /// <param name="lockedPose">The pose in world locked space.</param>
        /// <returns>The id for the added anchor if successful, else AnchorId.Unknown. See remarks.</returns>
        /// <remarks>
        /// This must be followed by <see cref="SendAlignmentAnchors"/> before it will have any effect.
        /// The returned AnchorId may be stored for future manipulation of the created anchor (e.g. for individual removal in <see cref="RemoveAlignmentAnchor(AnchorId)"/>).
        /// The system must be currently tracking to successfully add an alignment anchor. The alignment anchor
        /// will be in the current <see cref="Microsoft.MixedReality.WorldLocking.Core.Fragment"/>.
        /// The current fragment will be available when there is no tracking, and so this call will fail. 
        /// If this call fails, indicated by a return of AnchorId.Unknown, then it should be called again 
        /// on a later frame until it succeeds.
        /// </remarks>
        AnchorId AddAlignmentAnchor(string uniqueName, Pose virtualPose, Pose lockedPose);

        /// <summary>
        /// Get the world locked space pose associated with this alignment anchor.
        /// </summary>
        /// <param name="anchorId">Which anchor.</param>
        /// <param name="lockedPose">Pose to fill out if alignment anchor is found.</param>
        /// <returns>True if anchor is found and lockedPose filled in, else false and lockedPose set to identity.</returns>
        bool GetAlignmentPose(AnchorId anchorId, out Pose lockedPose);

        /// <summary>
        /// Remove the given alignment anchor from the system.
        /// </summary>
        /// <param name="anchorId">The anchor to remove (as returned by <see cref="AddAlignmentAnchor(string, Pose, Pose)"/></param>
        /// <returns>True if the anchor was found.</returns>
        bool RemoveAlignmentAnchor(AnchorId anchorId);

        /// <summary>
        /// Remove all alignment anchors that have been added. More efficient than removing them individually, 
        /// and doesn't require having stored their ids on creation.
        /// </summary>
        /// <remarks>
        /// This is more efficient than removing one by one, but take care to discard all existing AnchorIds returned by <see cref="AddAlignmentAnchor(string, Pose, Pose)"/>
        /// after this call, as it will be an error to try to use any of them.
        /// Also note that this clears the Alignment Anchors staged for commit with the next <see cref="SendAlignmentAnchors"/>,
        /// but the current ones will remain effective until the next call to SendAlignmentAnchors, which will send an empty list,
        /// unless it has been repopulated after the call to ClearAlignmentAnchors.
        /// </remarks>
        void ClearAlignmentAnchors();

        /// <summary>
        /// Submit all accumulated alignment anchors. 
        /// </summary>
        /// <remarks>
        /// All anchors previously submitted via SendAlignmentAnchors() will be cleared and replaced by the current set.
        /// SendAlignmentAnchors() submits the current set of anchors, but they will have no effect until the next 
        /// <see cref="IFragmentManager.Refreeze()"/> is successfully performed.
        /// </remarks>
        void SendAlignmentAnchors();

        /// <summary>
        /// Attempt to restore an alignment anchor from an earlier session. Stored alignment anchor
        /// must match in both uniqueName and virtual pose.
        /// </summary>
        /// <param name="uniqueName">Unique name use previously to create the alignment anchor.</param>
        /// <param name="virtualPose">Virtual pose to match with stored anchor pose.</param>
        /// <returns>AnchorId of restored Alignment Anchor on success, else AnchorId.Invalid.</returns>
        /// <remarks>
        /// If successful, alignment anchor is added but not sent. It must be followed by a call to SendAlignmentAnchors
        /// to take effect.
        /// </remarks>
        AnchorId RestoreAlignmentAnchor(string uniqueName, Pose virtualPose);

        /// <summary>
        /// True if the persistent state of the alignment manager has changed since the last save.
        /// </summary>
        bool NeedSave { get; }

        /// <summary>
        /// Save state needed to reconstruct later from persistent storage.
        /// </summary>
        /// <returns>True if saved (even if empty).</returns>
        bool Save();

        /// <summary>
        /// Load all persisted state required for reconstructing the current pinning.
        /// </summary>
        /// <returns>True if loaded.</returns>
        /// <remarks>
        /// The state required for reconstructing the pinning is loaded, but the reconstruction
        /// does not occur. Rather, the <see cref="PostAlignmentLoadedDelegate"/> is triggered, to
        /// prompt external actors to use the <see cref="IAlignmentManager.RestoreAlignmentAnchor(string, Pose)"/>
        /// API to effect the reconstruction. 👍
        /// </remarks>
        bool Load();
    }
}