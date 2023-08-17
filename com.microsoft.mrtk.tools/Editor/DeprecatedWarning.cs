// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Tools.Deprecated
{
    /// <summary>
    /// This class shows a warning that com.microsoft.mrtk.* packages are now deprecated.
    /// </summary>
    class DeprecatedWarning
    {
        const string k_Title = "Deprecated: The com.microsoft.mrtk.* packages";
        const string k_Message = "all com.microsoft.mrtk.* packages has been deprecated. The new packages are org.mixedrealitytoolkit.*  See https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity";
        const string k_HideWarningKey = "HideOldMRTKPackageDeprecatedWarning";

        [InitializeOnLoadMethod]
        static void ShowWarning()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (EditorUserSettings.GetConfigValue(k_HideWarningKey)?.Equals("true") ?? false)
            {
                return;
            }

            var hideWarning = !EditorUtility.DisplayDialog(
                k_Title,
                k_Message,
                "Understood",
                "Don't warn me again for this project"
            );
            EditorUserSettings.SetConfigValue(k_HideWarningKey, hideWarning.ToString().ToLower());
        }
    }
}