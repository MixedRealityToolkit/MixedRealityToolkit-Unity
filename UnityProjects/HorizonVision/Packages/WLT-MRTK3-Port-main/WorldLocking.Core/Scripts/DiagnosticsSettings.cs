// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Client tune-able settings for the diagnostics. Set through the WorldLockingManager.
    /// </summary>
    [System.Serializable]
    public struct DiagnosticsSettings
    {
        [SerializeField]
        [Tooltip("Ignore set values and use default behavior. When set, will reset all values to defaults.")]
        private bool useDefaults;
        /// <summary>
        /// Ignore set values and use default behavior. When set, will reset all values to defaults.
        /// </summary>
        public bool UseDefaults
        {
            get { return useDefaults; }
            set
            {
                useDefaults = value;
                if (useDefaults)
                {
                    InitToDefaults();
                }
            }
        }

        /// <summary>
        /// Diagnostics can be disabled if unneeded to reclaim any lost performance.
        /// </summary>
        [Tooltip("Diagnostics can be disabled if unneeded to reclaim any lost performance.")]
        public bool Enabled;

        /// <summary>
        /// Folder in which to keep diagnostics.
        /// </summary>
        [Tooltip("Folder in which to keep diagnostics.")]
        public string StorageSubdirectory;

        /// <summary>
        /// Base for auto-generated unique filename.
        /// </summary>
        [Tooltip("Base for auto-generated unique filename.")]
        public string StorageFileTemplate;

        /// <summary>
        /// Max file size.
        /// </summary>
        [Tooltip("Max file size.")]
        public int MaxKilobytesPerFile;

        /// <summary>
        /// Limit number of auto-generated files.
        /// </summary>
        [Tooltip("Limit number of auto-generated files.")]
        public int MaxNumberOfFiles;

        public DiagnosticsSettings InitToDefaults()
        {
            useDefaults = true;
            Enabled = false;
            StorageSubdirectory = "FrozenWorldDiagnostics";
            StorageFileTemplate = "FrozenWorld-[Machine]-[Timestamp].hkfw";
            MaxKilobytesPerFile = 2048;
            MaxNumberOfFiles = 64;
            return this;
        }
    }

    /// <summary>
    /// The SharedSettings boxes the Settings into a sharable reference.
    /// </summary>
    [System.Serializable]
    public class SharedDiagnosticsSettings
    {
        public DiagnosticsSettings settings;

        public SharedDiagnosticsSettings()
        {
            settings.InitToDefaults();
        }
    }

}