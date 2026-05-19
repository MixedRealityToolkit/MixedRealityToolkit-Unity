// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

#if WINDOWS_UWP && MROPENXR_PRESENT
using System;
using Windows.Perception;
using Windows.Perception.People;
using Windows.Perception.Spatial;
using Windows.UI.Input.Spatial;
#endif

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A helper class used to check eye calibration status. Only for UWP Platforms.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Eye Calibration Checker")]
    public class EyeCalibrationChecker : MonoBehaviour
    {
        #region Serialized Fields

        /// <summary>
        /// For testing purposes, you can manually assign whether eyes are calibrated or not in editor.
        /// </summary>
        [field: SerializeField, FormerlySerializedAs("EditorTestIsCalibrated"), Tooltip("For testing purposes, you can manually assign whether eyes are calibrated or not in editor.")]
        public EyeCalibrationStatus EditorTestIsCalibrated { get; set; } = EyeCalibrationStatus.Calibrated;

        #endregion Serialized Fields

        #region Private Fields

        /// <summary>
        /// Tracks whether eyes are present and calibrated.
        /// </summary>
        public EyeCalibrationStatus CalibratedStatus { get; private set; }

        private EyeCalibrationStatus prevCalibrationStatus;

#if WINDOWS_UWP && MROPENXR_PRESENT
        private const int MaxPoseAgeInSeconds = 1;
#endif

        #endregion Private Fields

        #region Events

        [SerializeField]
        [Tooltip("Event fired when eye tracking is calibrated.")]
        private UnityEvent calibrated = new UnityEvent();

        /// <summary>
        /// Event fired when eye tracking is calibrated.
        /// </summary>
        public UnityEvent Calibrated => calibrated;

        [SerializeField]
        [Tooltip("Event fired when eye tracking is not calibrated.")]
        private UnityEvent notCalibrated = new UnityEvent();

        /// <summary>
        /// Event fired when eye tracking is not calibrated.
        /// </summary>
        public UnityEvent NotCalibrated => notCalibrated;

        [SerializeField]
        [Tooltip("Event fired whenever eye tracking status changes.")]
        private EyeCalibrationStatusEvent calibratedStatusChanged = new EyeCalibrationStatusEvent();

        /// <summary>
        /// Event fired whenever eye tracking status changes.
        /// </summary>
        public EyeCalibrationStatusEvent CalibratedStatusChanged => calibratedStatusChanged;

        #endregion Events

        #region MonoBehaviour Functions

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        private void Update()
        {
            if (Application.isEditor)
            {
                CalibratedStatus = EditorTestIsCalibrated;
            }
            else
            {
                CalibratedStatus = CheckCalibrationStatus();
            }

            if (prevCalibrationStatus != CalibratedStatus)
            {
                if (CalibratedStatus == EyeCalibrationStatus.Calibrated)
                {
                    calibrated.Invoke();
                }
                else if (CalibratedStatus == EyeCalibrationStatus.NotCalibrated)
                {
                    notCalibrated.Invoke();
                }
                calibratedStatusChanged.Invoke(new EyeCalibrationStatusEventArgs(CalibratedStatus));
                prevCalibrationStatus = CalibratedStatus;
            }
        }

        #endregion MonoBehaviour Functions

        #region Private Functions

        private EyeCalibrationStatus CheckCalibrationStatus()
        {
#if WINDOWS_UWP && MROPENXR_PRESENT
            if (Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(Pose.identity) is SpatialCoordinateSystem worldOrigin)
            {
                SpatialPointerPose pointerPose = SpatialPointerPose.TryGetAtTimestamp(worldOrigin, PerceptionTimestampHelper.FromHistoricalTargetTime(DateTimeOffset.Now));
                if (pointerPose != null)
                {
                    EyesPose eyes = pointerPose.Eyes;
                    if (eyes != null)
                    {
                        // If it's been longer than a second since the last perception snapshot, assume the information has expired.
                        if ((DateTimeOffset.Now - eyes.UpdateTimestamp.TargetTime).TotalSeconds > MaxPoseAgeInSeconds)
                        {
                            return EyeCalibrationStatus.NotCalibrated;
                        }
                        else if (eyes.IsCalibrationValid)
                        {
                            return EyeCalibrationStatus.Calibrated;
                        }
                        else
                        {
                            return EyeCalibrationStatus.NotCalibrated;
                        }
                    }
                }
            }
            return EyeCalibrationStatus.NotTracked;
#else
            return EyeCalibrationStatus.Unsupported;
#endif // WINDOWS_UWP && MROPENXR_PRESENT
        }

        #endregion Private Functions
    }
}
