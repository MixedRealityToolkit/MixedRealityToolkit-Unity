// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Settings related to management of the internal anchor graph.
    /// </summary>
    [System.Serializable]
    public struct AnchorSettings 
    {
        [SerializeField]
        [Tooltip("Ignore set values and use default behavior. When set, will reset all values to defaults.")]
        private bool useDefaults;

        public enum AnchorSubsystem
        {
            Null,
            WSA,
            XRSDK,
            ARFoundation,
            ARCore
        };

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
        /// Check the validity of the settings.
        /// </summary>
        public bool IsValid
        {
            get 
            {
                if (MinNewAnchorDistance <= 0)
                {
                    Debug.LogWarning($"Setting Invalid: MinNewAnchorDistance = {MinNewAnchorDistance}");
                    return false;
                }
                if (MaxAnchorEdgeLength <= MinNewAnchorDistance)
                {
                    Debug.LogWarning($"Setting Invalid: MinNewAnchorDistance = {MinNewAnchorDistance} - MaxNewAnchorEdgeLength = {MaxAnchorEdgeLength}");
                    return false;
                }
                if (anchorSubsystem == AnchorSubsystem.ARFoundation)
                {
                    /// These must be supplied for ARF. Ignored otherwise.
                    if ((ARSessionSource == null) || (XROriginSource == null))
                    {
                        Debug.LogWarning($"Setting Invalid: ARSessionSource and XROriginSource must be set.");
                        return false;
                    }
#if !WLT_ARFOUNDATION_PRESENT
                    Debug.LogWarning($"Setting Invalid: ARF selected, but no ARFOUNDATION_PRESENT");
                    return false;
#endif // WLT_ARFOUNDATION_PRESENT
                }
                if (anchorSubsystem == AnchorSubsystem.XRSDK)
                {
#if !WLT_ARSUBSYSTEMS_PRESENT
                    Debug.LogWarning($"Setting Invalid: XRSDK selected, but no ARSUBSYSTEMS_PRESENT");
                    return false;
#endif // WLT_ARSUBSYSTEMS_PRESENT
                }
                if (anchorSubsystem == AnchorSubsystem.WSA)
                {
#if !UNITY_WSA
                    Debug.LogWarning($"Setting Invalid: WSA selected but no UNITY_WSA");
                    return false;
#endif // UNITY_WSA
                }
                if (anchorSubsystem == AnchorSubsystem.ARCore)
                {
#if !WLT_ARCORE_SDK_INCLUDED
                    Debug.LogWarning($"Setting Invalid: ARCore selected but ARCore SDK not imported.");
                    return false;
#endif // WLT_ARCORE_SDK_INCLUDED
                }
                return true; 
            }
        }

        /// <summary>
        /// Choice of subsystem that supplies anchors.
        /// </summary>
        public AnchorSubsystem anchorSubsystem;

        /// <summary>
        /// GameObject which has (or will have) the ARSession component, required when using the AR Foundation.
        /// </summary>
        /// <remarks>
        /// Ignored except when anchorSubsystem == ARF.
        /// </remarks>
        public GameObject ARSessionSource;

        /// <summary>
        /// GameObject which has (or will have) the XROrigin component, required when using AR Foundation.
        /// </summary>
        /// <remarks>
        /// Ignored except when anchorSubsystem == ARF.
        /// </remarks>
        public GameObject XROriginSource;

        /// <summary>
        /// The minimum distance to the current closest anchor before creating a new anchor.
        /// </summary>
        /// <remarks>
        /// A greater value will result in a less dense anchor coverage.
        /// </remarks>
        [Tooltip("The minimum distance to the current closest anchor before creating a new anchor.")]
        public float MinNewAnchorDistance;

        /// <summary>
        /// The maximum distance between two anchors to connect them with a graph edge.
        /// </summary>
        /// <remarks>
        /// This must be greater than MinNewAnchorDistance to create a connected graph of anchors.
        /// </remarks>
        [Tooltip("The maximum distance between two anchors to connect them with a graph edge.")]
        public float MaxAnchorEdgeLength;

        /// <summary>
        /// The maximum number of local anchors in the internal anchor graph.
        /// </summary>
        /// <remarks>
        /// Zero or any negative value is considered to be infinite (unlimited).
        /// </remarks>
        [Tooltip("The maximum number of local anchors in the internal anchor graph. Non-positive is infinity.")]
        public int MaxLocalAnchors;

        /// <summary>
        /// Use the Null anchor subsystem when running in Unity Editor.
        /// </summary>
        [Tooltip("Switch to Null anchor subsystem when running in Unity Editor.")]
        public bool NullSubsystemInEditor;

        /// <summary>
        /// Init all fields to default values.
        /// </summary>
        public void InitToDefaults()
        {
            useDefaults = true;
            anchorSubsystem = AnchorSubsystem.WSA;
            ARSessionSource = null;
            XROriginSource = null;
            MinNewAnchorDistance = 1.0f;
            MaxAnchorEdgeLength = 1.2f;
            MaxLocalAnchors = 0;
            NullSubsystemInEditor = true;
        }
    }
}