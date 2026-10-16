// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit;
using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Diagnostics;
using MixedReality.Toolkit.Subsystems;
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Debug = UnityEngine.Debug;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace MixedReality.Toolkit.Diagnostics.Tests
{
    /// <summary>
    /// Runtime test suite verifying null-safety, numeric formatting accuracy,
    /// and Zero-GC invariants across the SimpleProfiler visualizer and diagnostics subsystem hot paths.
    /// </summary>
    public class SimpleProfilerTests : BaseRuntimeTests
    {
        private PerformanceStatsSubsystem subsystem;

        [UnitySetUp]
        public override IEnumerator Setup()
        {
            yield return base.Setup();
            subsystem = SubsystemTestUtilities.CreateAndEnsureExists<PerformanceStatsSubsystem, PerformanceStatsSubsystemDescriptor>();
            subsystem.Start();
        }

        [UnityTearDown]
        public override IEnumerator TearDown()
        {
            if (subsystem != null)
            {
                if (subsystem.running)
                {
                    subsystem.Stop();
                }
                subsystem.Destroy();
                subsystem = null;
            }
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator SimpleProfiler_NullReferences_DoesNotThrow()
        {
            GameObject go = new GameObject("NullProfilerHolder");
            SimpleProfiler profiler = go.AddComponent<SimpleProfiler>();

            // All TMP_Text references are unassigned (null) by default.
            // With the subsystem running, UpdateCounters exercises the null checks on all text fields.
            Assert.DoesNotThrow(() =>
            {
                profiler.UpdateCounters();
            }, "SimpleProfiler.UpdateCounters must gracefully handle unassigned TMP_Text fields without throwing NullReferenceException.");

            // Let Unity's player loop execute Start() and FixedUpdate() naturally
            yield return null;

            Assert.DoesNotThrow(() =>
            {
                profiler.UpdateCounters();
            }, "SimpleProfiler.UpdateCounters must gracefully handle null fields on subsequent updates.");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SimpleProfiler_FixedUpdate_AllocatesZeroGC()
        {
            // Ensure subsystem memory statistics are populated
            ((IMRTKManagedSubsystem)subsystem).FixedUpdate();
            ((IMRTKManagedSubsystem)subsystem).LateUpdate();

            GameObject root = new GameObject("ProfilerTestRoot");
            SimpleProfiler profiler = root.AddComponent<SimpleProfiler>();

            // Set up TextMeshPro components
            GameObject goRamLimit = new GameObject("RamLimit");
            goRamLimit.transform.SetParent(root.transform);
            TMP_Text tmpRamLimit = goRamLimit.AddComponent<TextMeshPro>();

            GameObject goAllocRam = new GameObject("AllocatedRam");
            goAllocRam.transform.SetParent(root.transform);
            TMP_Text tmpAllocRam = goAllocRam.AddComponent<TextMeshPro>();

            GameObject goPeakRam = new GameObject("PeakRam");
            goPeakRam.transform.SetParent(root.transform);
            TMP_Text tmpPeakRam = goPeakRam.AddComponent<TextMeshPro>();

            GameObject goFps = new GameObject("FrameRate");
            goFps.transform.SetParent(root.transform);
            TMP_Text tmpFps = goFps.AddComponent<TextMeshPro>();

            GameObject goHz = new GameObject("RefreshRate");
            goHz.transform.SetParent(root.transform);
            TMP_Text tmpHz = goHz.AddComponent<TextMeshPro>();

            profiler.RamLimit = tmpRamLimit;
            profiler.AllocatedRam = tmpAllocRam;
            profiler.PeakAllocatedRam = tmpPeakRam;
            profiler.FrameRate = tmpFps;
            profiler.RefreshRate = tmpHz;

            yield return null;

            // Warmup: initial tick where lastBytes != currentBytes, populating all strings
            profiler.UpdateCounters();

            // Verify initial values were formatted and populated from the running subsystem
            Assert.AreNotEqual("--", tmpRamLimit.text);
            Assert.AreNotEqual("--", tmpAllocRam.text);
            Assert.AreNotEqual("--", tmpPeakRam.text);

            // Benchmark execution time across hot-path frames
            const int iterations = 1000;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                profiler.UpdateCounters();
            }
            sw.Stop();
            double usPerCall = (sw.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Debug.Log($"[Profiler Perf] SimpleProfiler.UpdateCounters: {usPerCall:F2} µs/call across {iterations} iterations.");

            // Zero-GC assertion on the per-frame hot path
            Assert.That(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    profiler.UpdateCounters();
                }
            }, Is.Not.AllocatingGCMemory(), "SimpleProfiler.UpdateCounters must not allocate managed heap memory on the per-frame hot path.");

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SimpleProfiler_TextOutput_AccurateFormatting()
        {
            // Populate subsystem stats
            ((IMRTKManagedSubsystem)subsystem).FixedUpdate();
            yield return new WaitForSeconds(0.12f);
            ((IMRTKManagedSubsystem)subsystem).LateUpdate();

            GameObject root = new GameObject("ProfilerFormattingRoot");
            SimpleProfiler profiler = root.AddComponent<SimpleProfiler>();

            GameObject goRamLimit = new GameObject("RamLimit");
            goRamLimit.transform.SetParent(root.transform);
            TMP_Text tmpRamLimit = goRamLimit.AddComponent<TextMeshPro>();

            GameObject goAllocRam = new GameObject("AllocatedRam");
            goAllocRam.transform.SetParent(root.transform);
            TMP_Text tmpAllocRam = goAllocRam.AddComponent<TextMeshPro>();

            GameObject goPeakRam = new GameObject("PeakRam");
            goPeakRam.transform.SetParent(root.transform);
            TMP_Text tmpPeakRam = goPeakRam.AddComponent<TextMeshPro>();

            GameObject goFps = new GameObject("FrameRate");
            goFps.transform.SetParent(root.transform);
            TMP_Text tmpFps = goFps.AddComponent<TextMeshPro>();

            GameObject goHz = new GameObject("RefreshRate");
            goHz.transform.SetParent(root.transform);
            TMP_Text tmpHz = goHz.AddComponent<TextMeshPro>();

            profiler.RamLimit = tmpRamLimit;
            profiler.AllocatedRam = tmpAllocRam;
            profiler.PeakAllocatedRam = tmpPeakRam;
            profiler.FrameRate = tmpFps;
            profiler.RefreshRate = tmpHz;

            yield return null;

            // SimpleProfiler.FixedUpdate() executes naturally, updating all counters via UpdateCounters().
            // RAM strings must match format: "<bytes> (<megabytes:0.00>)"
            StringAssert.IsMatch(@"^\d+ \(\d+\.\d{2}\)$", tmpRamLimit.text);
            StringAssert.IsMatch(@"^\d+ \(\d+\.\d{2}\)$", tmpAllocRam.text);
            StringAssert.IsMatch(@"^\d+ \(\d+\.\d{2}\)$", tmpPeakRam.text);

            // Frame rate must match format: "<fps:0.00>"
            StringAssert.IsMatch(@"^\d+\.\d{2}$", tmpFps.text);

            // Refresh rate remains "--" when no XRDisplaySubsystem is active in playmode test scene
            Assert.AreEqual("--", tmpHz.text);

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PerformanceStats_ZeroDivisionSafety()
        {
            // Verify MRTKPerformanceStatsSubsystem computes valid and finite stats without division by zero.
            // 1. Initial state before sampling period: FrameRate is safe (0, non-infinite, non-NaN)
            Assert.AreEqual(0f, subsystem.FrameRate);
            Assert.IsFalse(float.IsInfinity(subsystem.FrameRate));
            Assert.IsFalse(float.IsNaN(subsystem.FrameRate));

            // 2. Memory status tracking
            ((IMRTKManagedSubsystem)subsystem).FixedUpdate();
            Assert.Greater(subsystem.RamLimit, 0UL, "RamLimit must be greater than 0.");
            Assert.Greater(subsystem.AllocatedRam, 0UL, "AllocatedRam must be greater than 0.");
            Assert.GreaterOrEqual(subsystem.PeakAllocatedRam, subsystem.AllocatedRam, "PeakAllocatedRam must be >= AllocatedRam.");

            // 3. Tick LateUpdate immediately (elapsed time < samplePeriod) - must remain safe
            ((IMRTKManagedSubsystem)subsystem).LateUpdate();
            Assert.AreEqual(0f, subsystem.FrameRate);
            Assert.IsFalse(float.IsInfinity(subsystem.FrameRate));
            Assert.IsFalse(float.IsNaN(subsystem.FrameRate));

            // 4. Wait past the sample period (samplePeriod = 0.1s)
            yield return new WaitForSeconds(0.15f);
            ((IMRTKManagedSubsystem)subsystem).LateUpdate();

            // FrameRate must now be calculated, positive, finite, and not NaN
            Assert.Greater(subsystem.FrameRate, 0f, "FrameRate should be positive after sample period elapses.");
            Assert.IsFalse(float.IsInfinity(subsystem.FrameRate), "FrameRate must not be Infinity.");
            Assert.IsFalse(float.IsNaN(subsystem.FrameRate), "FrameRate must not be NaN.");
        }

        [Test]
        public void SimpleProfiler_FormatRamString_ZeroGC()
        {
            GameObject go = new GameObject("FormatRamTest");
            SimpleProfiler profiler = go.AddComponent<SimpleProfiler>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder(64);

            // 1. Boundary value 0
            profiler.FormatRamString(sb, 0UL, 0.0f);
            Assert.AreEqual("0 (0.00)", sb.ToString());

            // 2. Megabyte boundary (1 MB)
            sb.Clear();
            profiler.FormatRamString(sb, 1048576UL, 1.0f);
            Assert.AreEqual("1048576 (1.00)", sb.ToString());

            // 3. Fractional rounding
            sb.Clear();
            profiler.FormatRamString(sb, 2097152UL, 2.50f);
            Assert.AreEqual("2097152 (2.50)", sb.ToString());

            // 4. Large RAM (e.g. 16 GB)
            sb.Clear();
            profiler.FormatRamString(sb, 17179869184UL, 16384.0f);
            Assert.AreEqual("17179869184 (16384.00)", sb.ToString());

            // 5. Zero-GC Assertion
            TestDelegate formatDelegate = () =>
            {
                for (int i = 0; i < 50; i++)
                {
                    sb.Clear();
                    profiler.FormatRamString(sb, 17179869184UL, 16384.0f);
                }
            };
            formatDelegate();
            Assert.That(formatDelegate, Is.Not.AllocatingGCMemory(), "FormatRamString must not allocate any managed heap memory.");

            Object.Destroy(go);
        }
    }
}
#pragma warning restore CS1591
