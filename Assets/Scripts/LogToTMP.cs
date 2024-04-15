using System;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.SampleQRCodes;
using Microsoft.MixedReality.QR;


public class LogToTMP : MonoBehaviour
{
    public TextMeshProUGUI logText,unitylogText;
    public QRCodesManager qrCodesManager;
    public QRCodesVisualizer qrCodesVisualizer;

    public ljt.Config config;

    private void Update() {
        int QrcodeCount = FindObjectsOfType<Microsoft.MixedReality.SampleQRCodes.QRCode>().Length;
        bool isTracking = qrCodesManager.isTracking;
        bool isSupport = qrCodesManager.IsSupported;
        bool qrwatchallow = qrCodesManager.accessStatus == QRCodeWatcherAccessStatus.Allowed;
        int queueCount = qrCodesVisualizer.pendingActions.Count;
        string token = config.token;
        string mode = config.mode.ToString();
        
        logText.text = $"token = {token}, mode = {mode}, Qrcode Count = {QrcodeCount}, isTracking = {isTracking}, isSupport = {isSupport}, qrwatchallow = {qrwatchallow}, queueCount = {queueCount}";
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;  // 註冊Unity日誌回調
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;  // 解除註冊
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        unitylogText.text += logString + "\n";  // 將日誌文本添加到TextMeshPro元件中
    }
}
