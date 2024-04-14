using System;
using UnityEngine;
using TMPro;


public class LogToTMP : MonoBehaviour
{
    public TextMeshProUGUI logText;

    private void Update() {
        int QrcodeCount = FindObjectsOfType<Microsoft.MixedReality.SampleQRCodes.QRCode>().Length;
        logText.text = $"Qrcode Count = {QrcodeCount}";
    }
}
