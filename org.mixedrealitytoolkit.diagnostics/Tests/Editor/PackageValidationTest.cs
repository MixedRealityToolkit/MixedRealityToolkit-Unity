// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591
#if HAS_ASSET_STORE_VALIDATION && HAS_MRTK_CORE

using MixedReality.Toolkit.Core.Tests.EditMode;
using NUnit.Framework;

namespace MixedReality.Toolkit.Diagnostics.Tests.EditMode
{
    internal class PackageValidationTest
    {
        [Test]
        public void PackageTest()
        {
            PackageValidatorResults results = PackageValidator.Validate("org.mixedrealitytoolkit.diagnostics");
            Assert.AreEqual(0, results.FailedCount, $"Failed tests found.\n{results.ToString(PackageValidatorResults.MessageType.Failed)}");
            Assert.IsTrue(0 < results.SucceededCount, "No tests succeeded");
        }
    }
}
#endif // HAS_ASSET_STORE_VALIDATION && HAS_MRTK_CORE
#pragma warning restore CS1591
