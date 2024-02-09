// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591
#if HAS_ASSET_STORE_VALIDATION

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.AssetStoreValidation.ValidationSuite;

namespace MixedReality.Toolkit.Core.Tests.EditMode
{
    /// <summary>
    /// Static class for validating packages.
    /// </summary>
    public static class PackageValidator
    {
        /// <summary>
        /// Validate the package with the specified name.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// An exception is thrown if no package is found with the specified name.
        /// </exception>
        public static PackageValidatorResults Validate(string packageName)
        {
            PackageInfo info = UpmPackageInfo(packageName);
            if (info == null)
            {
                throw new ArgumentException($"No package found with name \"{packageName}\"");
            }

            string packageId = $"{info.name}@{info.version}";
            ValidationSuite.ValidatePackage(packageId, ValidationType.AssetStore);

            PackageValidatorResults results = new PackageValidatorResults(packageId);
            results.LoadResults();

            return results;
        }

        private static PackageInfo UpmPackageInfo(string packageIdOrName)
        {
            var packages = UpmListOffline(packageIdOrName);
            return packages.Length > 0 ? packages[0] : null;
        }

        private static PackageInfo[] UpmListOffline(string packageIdOrName = null)
        {
            var request = Client.List(offlineMode: true, includeIndirectDependencies: false);
            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            var result = new List<PackageInfo>();
            foreach (var upmPackage in request.Result)
            {
                if (!string.IsNullOrEmpty(packageIdOrName) && !(upmPackage.name == packageIdOrName || upmPackage.packageId == packageIdOrName))
                {
                    continue;
                }

                result.Add(upmPackage);
            }

            return result.ToArray();
        }
    }
}
#endif // HAS_ASSET_STORE_VALIDATION
#pragma warning restore CS1591
