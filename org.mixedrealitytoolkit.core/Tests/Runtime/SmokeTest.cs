// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.Core.Tests
{
    internal class SmokeTest
    {
        [UnityTest]
        public IEnumerator CorePackageTest()
        {
            yield return null;
        }
    }
}
#pragma warning restore CS1591