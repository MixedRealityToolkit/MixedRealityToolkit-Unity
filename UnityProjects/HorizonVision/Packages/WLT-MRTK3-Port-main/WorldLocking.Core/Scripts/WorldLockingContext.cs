// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// The central component for providing WorldLocking functionality to a scene
    /// </summary>
    /// <remarks>
    /// This component must be placed on a single GameObject in the scene. Typically, this would be a dedicated root GameObject with
    /// identity transform.
    /// </remarks>
    public class WorldLockingContext : MonoBehaviour {

        #region Public properties
        /// <summary>
        /// Hide settings in inspector, because they are published via custom editor.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private SharedManagerSettings shared = new SharedManagerSettings();

        /// <summary>
        /// WorldLocking settings. These are shared with the manager when active. Changes from script
        /// should be made through the manager's interface, but will be visible here in inspector.
        /// </summary>
        public SharedManagerSettings SharedSettings => shared;

        /// <summary>
        /// Hide settings in inspector, because they are published via custom editor.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private SharedDiagnosticsSettings diagnosticsSettings = new SharedDiagnosticsSettings();

        /// <summary>
        /// Diagnostics settings. These are shared with the manager when active. Changes from script
        /// should be made through the manager's interface, but will be visible here in inspector.
        /// </summary>
        public SharedDiagnosticsSettings DiagnosticsSettings => diagnosticsSettings;
        #endregion


        #region Unity internals
        /// <summary>
        /// Register for active scene change notifications. 
        /// Also, if the current scene is this's scene, push settings.
        /// </summary>
        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            CheckPushSettings(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// On disable, unregister for notifications.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        #endregion Unity internals

        #region Callback on scene change
        /// <summary>
        /// Callback for when the active scene changes. If the new scene will be the
        /// scene belonging to this, push contained settings. Otherwise ignore.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="next"></param>
        private void OnActiveSceneChanged(Scene current, Scene next)
        {
            CheckPushSettings(next);
        }

        /// <summary>
        /// Push contained settings if the input scene matches this's scene.
        /// </summary>
        /// <param name="scene"></param>
        private void CheckPushSettings(Scene scene)
        {
            if (scene == this.gameObject.scene)
            {
                WorldLockingManager manager = WorldLockingManager.GetInstance();
                manager.SetContext(this);
            }
        }
        #endregion Callback on scene change
    }
}
