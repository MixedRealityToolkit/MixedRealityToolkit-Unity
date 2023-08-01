// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.Input;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for the EyeCalibrationChecker. 
    /// </summary>
    public class EyeCalibrationCheckerTests : BaseRuntimeInputTests
    {
        private bool isCalibrated;
        private EyeCalibrationStatus calibrationStatus;

        [UnityTest]
        public IEnumerator TestEyeCalibrationEvents()
        {
            // Create an EyeCalibrationChecker and add event listeners
            GameObject testButton = new GameObject("EyeCalibrationChecker");
            EyeCalibrationChecker checker = testButton.AddComponent<EyeCalibrationChecker>();
            checker.Calibrated.AddListener(YesEyeCalibration);
            checker.NotCalibrated.AddListener(NoEyeCalibration);
            checker.CalibratedStatusChanged.AddListener(CalibrationEvent);
            yield return null;

            // Test whether the events fire when the status is changed
            isCalibrated = true;
            checker.EditorTestIsCalibrated = EyeCalibrationStatus.Calibrated;
            yield return null;
            checker.EditorTestIsCalibrated = EyeCalibrationStatus.NotCalibrated;
            yield return null;
            Assert.IsFalse(isCalibrated, "NotCalibrated event was not fired.");
            Assert.AreEqual(calibrationStatus, EyeCalibrationStatus.NotCalibrated, "CalibratedStatusChanged event was not fired.");
            yield return null;
            checker.EditorTestIsCalibrated = EyeCalibrationStatus.Calibrated;
            yield return null;
            Assert.IsTrue(isCalibrated, "Calibrated event was not fired.");
            Assert.AreEqual(calibrationStatus, EyeCalibrationStatus.Calibrated, "CalibratedStatusChanged event was not fired.");
            yield return null;

            checker.Calibrated.RemoveListener(NoEyeCalibration);
            checker.NotCalibrated.RemoveListener(YesEyeCalibration);
            checker.CalibratedStatusChanged.RemoveListener(CalibrationEvent);
        }

        private void CalibrationEvent(EyeCalibrationStatusEventArgs args)
        {
            calibrationStatus = args.CalibratedStatus;
        }

        private void YesEyeCalibration()
        {
            isCalibrated = true;
        }

        private void NoEyeCalibration()
        {
            isCalibrated = false;
        }
    }
}
#pragma warning restore CS1591