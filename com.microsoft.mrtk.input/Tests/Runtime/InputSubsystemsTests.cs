// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Subsystems;
using System.Collections;
using UnityEngine.TestTools;


namespace MixedReality.Toolkit.Input.Tests
{
    using AggregatorDescriptor = MRTKSubsystemDescriptor<HandsAggregatorSubsystem, HandsAggregatorSubsystem.Provider>;

    /// <summary>
    /// Tests for verifying the behavior of the various input-related subsystems.
    /// </summary>
    public class InputSubsystemsTests : BaseRuntimeInputTests
    {
        [UnityTest]
        public IEnumerator MRTKAggregatorSmoke()
        {
            var subsystem = SubsystemTestUtilities.CreateAndEnsureExists<MRTKHandsAggregatorSubsystem, AggregatorDescriptor>();
            SubsystemTestUtilities.TestStart<MRTKHandsAggregatorSubsystem>();
            yield return null;
        }

        [UnityTest]
        public IEnumerator XRSDKHandsSmoke()
        {
            var subsystem = SubsystemTestUtilities.CreateAndEnsureExists<XRSDKHandsSubsystem, HandsSubsystemDescriptor>();
            SubsystemTestUtilities.TestStart<MRTKHandsAggregatorSubsystem>();
            yield return null;
        }
    }
}
#pragma warning restore CS1591