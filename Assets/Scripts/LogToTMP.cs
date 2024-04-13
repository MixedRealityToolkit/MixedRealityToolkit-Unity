using System;
using UnityEngine;
using TMPro;

public class LogToTMP : MonoBehaviour
{
    public TextMeshProUGUI logText;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logText.text += logString + "\n";
        // 如果您也想顯示堆疊跟蹤或日誌類型，可以取消註釋下面的行
        // logText.text += "Type: " + type + " - " + logString + "\n" + stackTrace + "\n\n";
    }
}
