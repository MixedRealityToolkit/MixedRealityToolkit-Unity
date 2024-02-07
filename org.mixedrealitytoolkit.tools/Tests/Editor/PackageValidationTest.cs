// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#if HAS_ASSET_STORE_VALIDATION

using MixedReality.Toolkit.Core.Tests.EditMode;
using NUnit.Framework;
using System;

namespace MixedReality.Toolkit.Tools.Tests.EditMode
{
    /// <summary>
    /// This class is used to validate the package for the Mixed Reality Toolkit Tools package, verifying that all
    /// requirements are met for publishing the package to the Unity Asset Store.
    /// </summary>
    internal class PackageValidationTest
    {
        /// <summary>
        /// Test to validate the package for the Mixed Reality Toolkit Tools package, verifying that all
        /// requirements are met for publishing the package to the Unity Asset Store.
        /// </summary>
        [Test]
        public void PackageTest()
        {
            PackageValidatorResults results = PackageValidator.Validate("org.mixedrealitytoolkit.tools");
            Assert.AreEqual(0, results.FailedCount, $"Failed tests found.{Environment.NewLine}{results.ToString(PackageValidatorResults.MessageType.Failed)}");
            Assert.IsTrue(0 < results.SucceededCount, "No tests succeeded");
        }
    }
}
#endif // HAS_ASSET_STORE_VALIDATION
