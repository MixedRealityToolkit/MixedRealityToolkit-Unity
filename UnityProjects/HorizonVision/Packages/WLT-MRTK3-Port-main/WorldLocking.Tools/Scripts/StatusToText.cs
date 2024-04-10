// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Helper to bind WorldLockingManager diagnostics to text meshes for display.
    /// </summary>
    public class StatusToText : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Text mesh to display version and timestamp.")]
        private TextMesh textMeshVersionTimestamp = null;
        /// <summary>
        /// Text mesh to display version and timestamp.
        /// </summary>
        public bool VersionTimestampEnabled { get { return getEnabled(textMeshVersionTimestamp); } set { setEnabled(textMeshVersionTimestamp, value); } }

        [SerializeField]
        [Tooltip("Text mesh for display of error status.")]
        private TextMesh textMeshErrorStatus = null;
        /// <summary>
        /// Text mesh for display of error status.
        /// </summary>
        public bool ErrorStatusEnabled { get { return getEnabled(textMeshErrorStatus); } set { setEnabled(textMeshErrorStatus, value); } }

        [SerializeField]
        [Tooltip("Text mesh for display of current state.")]
        private TextMesh textMeshStateIndicator = null;
        /// <summary>
        /// Text mesh for display of current state.
        /// </summary>
        public bool StateIndicatorEnabled { get { return getEnabled(textMeshStateIndicator); } set { setEnabled(textMeshStateIndicator, value); } }

        [SerializeField]
        [Tooltip("Text mesh for display of summary info.")]
        private TextMesh textMeshInfo = null;
        /// <summary>
        /// Text mesh for display of summary info.
        /// </summary>
        public bool InfoEnabled { get { return getEnabled(textMeshInfo); } set { setEnabled(textMeshInfo, value); } }

        [SerializeField]
        [Tooltip("Text mesh for display of detailed metrics.")]
        /// <summary>
        /// Text mesh for display of detailed metrics.
        /// </summary>
        private TextMesh textMeshMetrics = null;

        /// <summary>
        /// Simple class to smooth framerate/frametime.
        /// </summary>
        private class FramerateSmoother
        {
            private int bufferLength = 30;
            private float[] timeBuffer = null;
            private int nextIndex = 0;

            public int BufferLength
            {
                get { return bufferLength; }
                set
                {
                    Debug.Assert(value > 0, "Invalid smoothing buffer length");
                    if (value != bufferLength)
                    {
                        bufferLength = value;
                        timeBuffer = null;
                        nextIndex = 0;
                    }
                }
            }
            public void AddTime(float deltaSecs)
            {
                if (timeBuffer == null)
                {
                    timeBuffer = new float[bufferLength];
                    for (int i = 0; i < timeBuffer.Length; ++i)
                    {
                        timeBuffer[i] = deltaSecs;
                    }
                }
                else
                {
                    timeBuffer[nextIndex] = deltaSecs;
                    if (++nextIndex >= timeBuffer.Length)
                    {
                        nextIndex = 0;
                    }
                }
            }

            public void GetSmoothStats(out float average, out float minimum, out float maximum)
            {
                float tot = timeBuffer[0];
                float lo = tot;
                float hi = tot;
                for (int i = 1; i < timeBuffer.Length; ++i)
                {
                    tot += timeBuffer[i];
                    lo = Mathf.Min(lo, timeBuffer[i]);
                    hi = Mathf.Max(hi, timeBuffer[i]);
                }
                average =  tot / timeBuffer.Length;
                minimum = lo;
                maximum = hi;
            }

        }

        private FramerateSmoother framerateSmoother = new FramerateSmoother();

        /// <summary>
        /// Whether display of detailed metrics currently enabled.
        /// </summary>
        public bool MetricsEnabled { get { return getEnabled(textMeshMetrics); } set { setEnabled(textMeshMetrics, value); } }

        private bool getEnabled(TextMesh textMesh)
        {
            return textMesh != null ? textMesh.gameObject.activeSelf : false;
        }

        private void setEnabled(TextMesh textMesh, bool enabled)
        {
            if (textMesh != null && (textMesh.gameObject.activeSelf != enabled))
            {
                textMesh.gameObject.SetActive(enabled);
            }
        }

        private void Start()
        {
        }

        private void SetTextIfChanged(TextMesh textMesh, string newText)
        {
            if (textMesh.text != newText)
            {
                textMesh.text = newText;
            }
        }

        private void Update()
        {
            var worldLockingManager = WorldLockingManager.GetInstance();
            if (worldLockingManager != null)
            {
                if (VersionTimestampEnabled)
                {
                    SetTextIfChanged(textMeshVersionTimestamp, CaptureVersionTimestamp(worldLockingManager));
                }
                if (ErrorStatusEnabled)
                {
                    SetTextIfChanged(textMeshErrorStatus, worldLockingManager.ErrorStatus);
                }

                bool indicatorActive = false;
                if (textMeshStateIndicator.text != null)
                {
                    if (worldLockingManager.RefreezeIndicated)
                    {
                        SetTextIfChanged(textMeshStateIndicator, "Refreeze indicated");
                        indicatorActive = true;
                    }
                    else if (worldLockingManager.MergeIndicated)
                    {
                        SetTextIfChanged(textMeshStateIndicator, "Merge indicated");
                        indicatorActive = true;
                    }
                }
                StateIndicatorEnabled = indicatorActive;

                if (InfoEnabled)
                {
                    SetTextIfChanged(textMeshInfo, CaptureInfoText(worldLockingManager));
                }

                if (MetricsEnabled)
                {
                    SetTextIfChanged(textMeshMetrics, CaptureMetrics(worldLockingManager));
                }
            }
        }

        /// <summary>
        /// Format the version and time info.
        /// </summary>
        /// <param name="manager">Source manager</param>
        /// <returns>Formatted string</returns>
        private string CaptureVersionTimestamp(WorldLockingManager manager)
        {
            framerateSmoother.AddTime(Time.unscaledDeltaTime);
            float average;
            float minimum;
            float maximum;
            framerateSmoother.GetSmoothStats(out average, out minimum, out maximum);
            float fps = average > 0 ? 1.0f / average : 0.0f;
            float secToMs = 1000.0f;
            string version = ""
                + string.Format("WLT Version\t: {0}\n", WorldLockingManager.Version)
                + string.Format("DLL Version\t\t: {0}\n", manager.Plugin.VersionCompact)
                + string.Format("TimeStamp\t\t: {0:F3}\n", Time.time)
                + string.Format("Frames/sec\t\t: {0:F1} [{1:F1}ms ({2:F1})max]", fps, average * secToMs, maximum * secToMs);
            return version;
        }


        /// <summary>
        /// Format the summary info.
        /// </summary>
        /// <param name="manager">Source manager</param>
        /// <returns>Formatted string</returns>
        private string CaptureInfoText(WorldLockingManager manager)
        {
            string infoText = ""
            + string.Format("Anchors          : {0}\n", manager.AnchorManager.NumAnchors)
            + string.Format("Edges            : {0}\n", manager.AnchorManager.NumEdges)
            + string.Format("Fragments        : {0}\n", manager.FragmentManager.NumFragments)
            + string.Format("Current Fragment : {0}\n", manager.FragmentManager.CurrentFragmentId)
            + string.Format("Anchor Subsystem : {0}\n", manager.AnchorSettings.anchorSubsystem)
            + string.Format("Can Save/Load    : {0} ({1})\n", manager.AnchorManager.SupportsPersistence, manager.FrozenWorldFileName);
            
            return infoText;
        }

        /// <summary>
        /// Format the detailed metrics.
        /// </summary>
        /// <param name="manager">Source manager</param>
        /// <returns>Formatted string</returns>
        private string CaptureMetrics(WorldLockingManager manager)
        {
            var m = manager.Plugin.Metrics;

            return ""
                + string.Format("refitMergeIndicated          : {0}\n", m.RefitMergeIndicated)
                + string.Format("refitRefreezeIndicated       : {0}\n", m.RefitRefreezeIndicated)
                + string.Format("numTrackableFragments        : {0}\n", m.NumTrackableFragments)
                + string.Format("numVisualSupports            : {0}\n", m.NumVisualSupports)
                + string.Format("numVisualSupportAnchors      : {0}\n", m.NumVisualSupportAnchors)
                + string.Format("numIgnoredSupports           : {0}\n", m.NumIgnoredSupports)
                + string.Format("numIgnoredSupportAnchors     : {0}\n", m.NumIgnoredSupportAnchors)
                + string.Format("maxLinearDeviation           : {0}\n", m.MaxLinearDeviation)
                + string.Format("maxLateralDeviation          : {0}\n", m.MaxLateralDeviation)
                + string.Format("maxAngularDeviation          : {0}\n", m.MaxAngularDeviation)
                + string.Format("maxLinearDeviationInFrustum  : {0}\n", m.MaxLinearDeviationInFrustum)
                + string.Format("maxLateralDeviationInFrustum : {0}\n", m.MaxLateralDeviationInFrustum)
                + string.Format("maxAngularDeviationInFrustum : {0}\n", m.MaxAngularDeviationInFrustum)
            ;

        }
    }
}
