// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.Diagnostics
{
    /// <summary>
    /// Script to display performance statistics as simple text objects.
    /// </summary>
    [AddComponentMenu("MRTK/Diagnostics/Simple Profiler")]
    public class SimpleProfiler : MonoBehaviour
    {
        #region Frame properties

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the frame rate.")]
        private TMP_Text frameRate = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the application frame rate.
        /// </summary>
        public TMP_Text FrameRate
        {
            get => frameRate;
            set => frameRate = value;
        }

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the display refresh rate.")]
        private TMP_Text refreshRate = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the display refresh rate.
        /// </summary>
        public TMP_Text RefreshRate
        {
            get => refreshRate;
            set => refreshRate = value;
        }

        #endregion Frame properties

        #region Memory properties

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the RAM addressable by the application.")]
        private TMP_Text ramLimit = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the RAM addressable
        /// by the application.
        /// </summary>
        public TMP_Text RamLimit
        {
            get => ramLimit;
            set => ramLimit = value;
        }

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the allocated RAM.")]
        private TMP_Text allocatedRam = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the allocated RAM.
        /// </summary>
        public TMP_Text AllocatedRam
        {
            get => allocatedRam;
            set => allocatedRam = value;
        }

        [SerializeField]
        [Tooltip("Text Mesh Pro object to use when displaying the peak allocated RAM.")]
        private TMP_Text peakAllocatedRam = null;

        /// <summary>
        /// Text Mesh Pro object to use when displaying the peak allocated RAM.
        /// </summary>
        public TMP_Text PeakAllocatedRam
        {
            get => peakAllocatedRam;
            set => peakAllocatedRam = value;
        }

        #endregion Memory properties

        private const string noData = "--";

        private readonly StringBuilder stringBuilder = new StringBuilder(64);
        private readonly char[] digitBuffer = new char[32];

        private ulong lastRamLimit = ulong.MaxValue;
        private ulong lastAllocatedRam = ulong.MaxValue;
        private ulong lastPeakAllocatedRam = ulong.MaxValue;
        private float lastFrameRate = -1f;
        private float lastRefreshRate = -1f;

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary> 
        private void Start()
        {
            if (RamLimit != null)
            {
                RamLimit.text = noData;
            }
            if (AllocatedRam != null)
            {
                AllocatedRam.text = noData;
            }
            if (PeakAllocatedRam != null)
            {
                PeakAllocatedRam.text = noData;
            }

            if (FrameRate != null)
            {
                FrameRate.text = noData;
            }
            if (RefreshRate != null)
            {
                RefreshRate.text = noData;
            }
        }

        /// <summary>
        /// A Unity event function that is called at an framerate independent frequency, and is only called if this object is enabled.
        /// </summary>
        private void FixedUpdate()
        {
            UpdateCounters();
        }

        /// <summary>
        /// Formats and updates the visualizer text counters.
        /// </summary>
        internal void UpdateCounters()
        {
            if (PerformanceStatsHelpers.Subsystem != null)
            {
                UpdateRamText(RamLimit, ref lastRamLimit, PerformanceStatsHelpers.Subsystem.RamLimit);
                UpdateRamText(AllocatedRam, ref lastAllocatedRam, PerformanceStatsHelpers.Subsystem.AllocatedRam);
                UpdateRamText(PeakAllocatedRam, ref lastPeakAllocatedRam, PerformanceStatsHelpers.Subsystem.PeakAllocatedRam);

                if (FrameRate != null)
                {
                    float fps = PerformanceStatsHelpers.Subsystem.FrameRate;
                    if (Mathf.Abs(fps - lastFrameRate) > MathUtilities.ZeroTolerance)
                    {
                        lastFrameRate = fps;
                        FrameRate.SetText("{0:0.00}", fps);
                    }
                }
            }

            if (RefreshRate != null)
            {
                float refRate = -1f;
                bool? haveRefreshRate = XRSubsystemHelpers.DisplaySubsystem?.TryGetDisplayRefreshRate(out refRate);
                if (haveRefreshRate.HasValue && haveRefreshRate.Value)
                {
                    if (Mathf.Abs(refRate - lastRefreshRate) > MathUtilities.ZeroTolerance)
                    {
                        lastRefreshRate = refRate;
                        RefreshRate.SetText("{0:0.00}", refRate);
                    }
                }
                else if (lastRefreshRate >= 0f || RefreshRate.text != noData)
                {
                    lastRefreshRate = -1f;
                    RefreshRate.text = noData;
                }
            }
        }

        private void UpdateRamText(TMP_Text textComponent, ref ulong lastBytes, ulong currentBytes)
        {
            if (textComponent == null)
            {
                return;
            }

            if (currentBytes != lastBytes)
            {
                lastBytes = currentBytes;
                float megabytes = MathUtilities.BytesToMegabytes(currentBytes);
                stringBuilder.Clear();
                FormatRamString(stringBuilder, currentBytes, megabytes);
                textComponent.SetText(stringBuilder);
            }
        }

        internal void FormatRamString(StringBuilder sb, ulong bytes, float megabytes)
        {
            AppendULong(sb, bytes);
            sb.Append(" (");
            AppendFixed2(sb, megabytes);
            sb.Append(')');
        }

        private void AppendULong(StringBuilder sb, ulong value)
        {
            if (value == 0)
            {
                sb.Append('0');
                return;
            }

            int startIndex = digitBuffer.Length;
            while (value > 0)
            {
                digitBuffer[--startIndex] = (char)('0' + (value % 10));
                value /= 10;
            }
            for (int i = startIndex; i < digitBuffer.Length; i++)
            {
                sb.Append(digitBuffer[i]);
            }
        }

        private void AppendFixed2(StringBuilder sb, float value)
        {
            if (value < 0f)
            {
                sb.Append('-');
                value = -value;
            }

            ulong totalHundredths = (ulong)(value * 100f + 0.5f);
            ulong whole = totalHundredths / 100UL;
            ulong fraction = totalHundredths % 100UL;

            AppendULong(sb, whole);
            sb.Append('.');
            sb.Append((char)('0' + (fraction / 10)));
            sb.Append((char)('0' + (fraction % 10)));
        }
    }
}
