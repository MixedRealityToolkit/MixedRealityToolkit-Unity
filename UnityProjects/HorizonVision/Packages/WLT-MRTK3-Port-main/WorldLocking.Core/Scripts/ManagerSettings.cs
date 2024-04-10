// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Manager settings.
    /// </summary>
    [System.Serializable]
    public struct ManagerSettings
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
        /// Whether the WorldLocking stabilization is active or bypassed (if not Enabled).
        /// </summary>
        [Tooltip("Whether the WorldLocking stabilization is active or bypassed (if not Enabled).")]
        public bool Enabled;

        /// <summary>
        /// Automatically trigger a fragment merge whenever the FrozenWorld engine indicates that
        /// one would be appropriate.
        /// </summary>
        [Tooltip("Automatically trigger a fragment merge whenever the FrozenWorld engine indicates that one would be appropriate.")]
        public bool AutoMerge;

        /// <summary>
        /// Automatically trigger a refreeze whenever the FrozenWorld engine indicates that
        /// one would be appropriate.
        /// </summary>
        [Tooltip("Automatically trigger a fragment refreeze whenever the FrozenWorld engine indicates that one would be appropriate.")]
        public bool AutoRefreeze;

        /// <summary>
        /// Automatically load the WorldLocking state from disk from previous run at startup.
        /// </summary>
        [Tooltip("Automatically load the WorldLocking state from disk from previous run at startup.")]
        public bool AutoLoad;

        /// <summary>
        /// Periodically save the WorldLocking state to disk.
        /// </summary>
        [Tooltip("Periodically save the WorldLocking state to disk.")]
        public bool AutoSave;

        /// <summary>
        /// Put this into default initialized state.
        /// </summary>
        /// <returns>This initialized to defaults.</returns>
        public ManagerSettings InitToDefaults()
        {
            AutoMerge = true;
            AutoRefreeze = true;
            AutoLoad = true;
            AutoSave = true;
            Enabled = true;
            return this;
        }
    }

    /// <summary>
    /// Shareable (reference type) version of Settings (value struct).
    /// </summary>
    [System.Serializable]
    public class SharedManagerSettings
    {
        /// <summary>
        /// The manager settings to be shared.
        /// </summary>
        public ManagerSettings settings;

        /// <summary>
        /// Transform links to be shared.
        /// </summary>
        public LinkageSettings linkageSettings;

        /// <summary>
        /// Anchor management settings.
        /// </summary>
        public AnchorSettings anchorSettings;

        /// <summary>
        /// Wrap a copy of settings initialized to default values.
        /// </summary>
        public SharedManagerSettings()
        {
            settings.InitToDefaults();
            linkageSettings.InitToDefaults();
            anchorSettings.InitToDefaults();
        }
    }

}