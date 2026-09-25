// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Subsystems;
using NUnit.Framework;
using UnityEngine;

namespace MixedReality.Toolkit.Core.Tests.EditMode
{
    /// <summary>
    /// Unit tests for <see cref="MRTKLifecycleManager"/>.
    /// </summary>
    public class MRTKLifecycleManagerTests
    {
        [Test]
        public void DebugLogging_DefaultsToTrue()
        {
            MRTKProfile originalProfile = MRTKProfile.Instance;
            MRTKProfile testProfile = ScriptableObject.CreateInstance<MRTKProfile>();
            MRTKProfile.Instance = testProfile;

            GameObject go = new GameObject();
            try
            {
                MRTKLifecycleManager manager = go.AddComponent<MRTKLifecycleManager>();
                Assert.IsTrue(manager.DebugLogging, "DebugLogging should default to true.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                MRTKProfile.Instance = originalProfile;
                UnityEngine.Object.DestroyImmediate(testProfile);
            }
        }

        [Test]
        public void DebugLogging_CanBeToggled()
        {
            MRTKProfile originalProfile = MRTKProfile.Instance;
            MRTKProfile testProfile = ScriptableObject.CreateInstance<MRTKProfile>();
            MRTKProfile.Instance = testProfile;

            GameObject go = new GameObject();
            try
            {
                MRTKLifecycleManager manager = go.AddComponent<MRTKLifecycleManager>();
                manager.DebugLogging = false;
                Assert.IsFalse(manager.DebugLogging, "DebugLogging should be false after setting it to false.");
                manager.DebugLogging = true;
                Assert.IsTrue(manager.DebugLogging, "DebugLogging should be true after setting it to true.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                MRTKProfile.Instance = originalProfile;
                UnityEngine.Object.DestroyImmediate(testProfile);
            }
        }
    }
}
#pragma warning restore CS1591
