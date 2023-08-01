// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.Performance.Tests
{
    internal class SmokeTest
    {
        [UnityTest]
        public IEnumerator PerformancePackageTest()
        {
            yield return null;
        }
    }
}
#pragma warning restore CS1591