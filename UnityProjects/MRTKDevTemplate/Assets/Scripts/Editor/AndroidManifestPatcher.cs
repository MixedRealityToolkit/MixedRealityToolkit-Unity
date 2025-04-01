// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using Unity.XR.Management.AndroidManifest.Editor;
using UnityEngine.XR.OpenXR;

/// <summary>
/// Adds a required manifest entry to use the virtual keyboard on Quest.
/// </summary>
public class AndroidManifestPatcher : IAndroidManifestRequirementProvider
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
