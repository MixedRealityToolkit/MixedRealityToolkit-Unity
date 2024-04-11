// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.SampleQRCodes
{
    public class QRCodesSetup : MonoBehaviour
    {
        [Tooltip("Determines if the QR codes scanner should be automatically started.")]
        public bool AutoStartQRTracking = true;

        [Tooltip("Visualize the detected QRCodes in the 3d space.")]
        public bool VisualizeQRCodes = true;

        QRCodesManager qrCodesManager = null;

        void Awake()
        {
            qrCodesManager = QRCodesManager.Instance;
            if (AutoStartQRTracking)
            {
                qrCodesManager.StartQRTracking();
            }
            if (VisualizeQRCodes)
            {
                gameObject.AddComponent(typeof(QRCodesVisualizer));
            }
        }
    }
}
