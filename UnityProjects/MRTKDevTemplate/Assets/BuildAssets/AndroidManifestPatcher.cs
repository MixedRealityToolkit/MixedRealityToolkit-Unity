// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using Unity.XR.Management.AndroidManifest.Editor;
using UnityEngine.XR.OpenXR;

namespace MixedReality.Toolkit.Examples.Build
{
    /// <summary>
    /// Adds a required manifest entry to use the virtual keyboard on Quest.
    /// </summary>
    internal class AndroidManifestPatcher : IAndroidManifestRequirementProvider
    {
        /// <inheritdoc/>
        ManifestRequirement IAndroidManifestRequirementProvider.ProvideManifestRequirement() => new()
        {
            SupportedXRLoaders = new HashSet<Type>()
            {
                typeof(OpenXRLoader)
            },
            NewElements = new List<ManifestElement>()
            {
                new()
                {
                    ElementPath = new List<string> { "manifest", "uses-feature" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "oculus.software.overlay_keyboard" },
                        { "required", "false" }
                    }
                }
            },
        };
    }
}
