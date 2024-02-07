// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591
#if HAS_ASSET_STORE_VALIDATION && HAS_MRTK_CORE

using MixedReality.Toolkit.Core.Tests.EditMode;
using NUnit.Framework;

namespace MixedReality.Toolkit.StandardAssets.Tests.EditMode
{
    /// <summary>
    /// This class is used to validate the package for the Mixed Reality Toolkit Standard Assets package, verifying that all
    /// requirements are met for publishing the package to the Unity Asset Store. 
    /// </summary>
    internal class PackageValidationTest
    {
        /// <summary>
        /// Test to validate the package for the Mixed Reality Toolkit Standard Assets package, verifying that all
        /// requirements are met for publishing the package to the Unity Asset Store.
        /// </summary>
        [Test]
        public void PackageTest()
        {
            PackageValidatorResults results = PackageValidator.Validate("org.mixedrealitytoolkit.standardassets");
            Assert.AreEqual(0, results.FailedCount, $"Failed tests found.\n{results.ToString(PackageValidatorResults.MessageType.Failed)}");
            Assert.IsTrue(0 < results.SucceededCount, "No tests succeeded");
        }
    }
}
#endif // HAS_ASSET_STORE_VALIDATION && HAS_MRTK_CORE
#pragma warning restore CS1591
