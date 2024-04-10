// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//#define WLT_EXTRA_DEBUG_ADJUSTER

using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

#if WLT_EXTRA_DEBUG_ADJUSTER
using System.IO;
#endif // WLT_EXTRA_DEBUG_ADJUSTER

namespace Microsoft.MixedReality.WorldLocking.Tools
{

    /// <summary>
    /// Component to handle frozen world adjustments for fixed (stationary) objects.
    /// </summary>
    /// <remarks>
    /// For dynamic objects, use <see cref="AdjusterMoving"/>.
    /// 
    /// This component is appropriate for inheriting from, to let it take care of
    /// lifetime management and book-keeping, then just override <see cref="HandleAdjustLocation(Pose)"/>
    /// and/or <see cref="HandleAdjustState(AttachmentPointStateType)"/> with actions more suitable
    /// for your application.
    /// </remarks>
    public class AdjusterFixed : AdjusterBase
    {
        /// <summary>
        /// The attachment point manager interface which this component subscribes to.
        /// </summary>
        protected IAttachmentPointManager Manager { get { return WorldLockingManager.GetInstance().AttachmentPointManager; } }

        /// <summary>
        /// The attachment point which this component wraps.
        /// </summary>
        protected IAttachmentPoint AttachmentPoint { get; private set; }

        /// <summary>
        /// Ask the manager for an attachment point, passing delegates for update
        /// </summary>
        private void Start()
        {
            AttachmentPoint = Manager.CreateAttachmentPoint(gameObject.transform.position, null,
                HandleAdjustLocation,   // Handle adjustments to position
                HandleAdjustState  // Handle connectedness of fragment
                );
            AttachmentPoint.Name = string.Format($"{gameObject.name}=>{AttachmentPoint.Name}");
        }

        /// <summary>
        /// Let the manager know the attachment point is freed.   
        /// </summary>
        private void OnDestroy()
        {
            Manager.ReleaseAttachmentPoint(AttachmentPoint);
            AttachmentPoint = null;
        }

        /// <summary>
        /// For infrequent moves under script control, UpdatePosition notifies the system that the
        /// object has relocated. It should be called after any scripted movement of the object
        /// (but **not** after movement triggered by WLT, such as in <see cref="HandleAdjustLocation(Pose)"/>).
        /// </summary>
        public void UpdatePosition()
        {
            if (AttachmentPoint != null)
            {
                Manager.MoveAttachmentPoint(AttachmentPoint, gameObject.transform.position);
            }
        }

        /// <summary>
        /// Handle a pose adjustment due to a refit operation.
        /// </summary>
        /// <param name="adjustment">The pose adjustment to apply/</param>
        /// <remarks>
        /// This simple implementation folds the adjustment into the current pose.
        /// </remarks>
        protected virtual void HandleAdjustLocation(Pose adjustment)
        {
#if WLT_EXTRA_DEBUG_ADJUSTER
            string msg = $"{name}:\n";
            msg += DebugPose("adj", adjustment);
            msg += DebugPose("from", transform.GetGlobalPose());
#endif // WLT_EXTRA_DEBUG_ADJUSTER

            Pose pose = gameObject.transform.GetGlobalPose();
            pose = adjustment.Multiply(pose);
            gameObject.transform.SetGlobalPose(pose);

#if WLT_EXTRA_DEBUG_ADJUSTER
            msg += DebugPose("to", transform.GetGlobalPose());
            msg += DebugAttachmentPoint("att", AttachmentPoint);
            msg += $"End {name}\n";
            DebugMessage(msg);
#endif // WLT_EXTRA_DEBUG_ADJUSTER
        }

#if WLT_EXTRA_DEBUG_ADJUSTER
        private string DebugPose(string label, Pose pose)
        {
            return $"--- {label} {pose.position.ToString("F3")} {pose.rotation.ToString("F3")}\n";
        }

        private string DebugAttachmentPoint(string label, IAttachmentPoint att)
        {
            return $"aaa {label} A:{att.AnchorId} F:{att.FragmentId} {att.State} {att.LocationFromAnchor.ToString("F3")}\n";
        }

        private void DebugMessage(string msg)
        {
            string fileName = "refit.txt";
            fileName = Path.Combine(Application.persistentDataPath, fileName);
            using (StreamWriter writer = File.AppendText(fileName))
            {
                writer.WriteLine(msg);
                writer.Flush();
            }
        }
#endif // WLT_EXTRA_DEBUG_ADJUSTER

        /// <summary>
        /// Handle a change in associated fragment state.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <remarks>
        /// The only state under which the visual location can be regarded as reliable
        /// is the Normal state.
        /// This simple implementation disables the object tree when its location is unreliable,
        /// and enables it when its location is reliable.
        /// Actual appropriate behavior is highly application dependent. Some questions to ask:
        /// * Is there a more appropriate way to hide the object (e.g. move it far away)?
        /// * Should the update pause, or just stop rendering? (Disabling pauses update **and** render).
        /// * Is it better to hide the object, or to display it in alternate form?
        /// * Etc.
        /// </remarks>
        protected virtual void HandleAdjustState(AttachmentPointStateType state)
        {
#if WLT_EXTRA_DEBUG_ADJUSTER
            DebugMessage($"{name} to state {state}");
#endif // WLT_EXTRA_DEBUG_ADJUSTER
            bool visible = state == AttachmentPointStateType.Normal;
            if (visible != gameObject.activeSelf)
            {
                gameObject.SetActive(visible);
            }
        }

    }
}