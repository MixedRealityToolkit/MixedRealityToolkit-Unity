using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class ContentDownloader : MonoBehaviour
{
    public string token => Config.Instance.token;
    public string url => Config.Instance.host;

    public RawScene result;
    public UnityEvent<RawScene> onDownloaded;

    public void DownloadSceneInfo()
    {
        var SceneInfoUrl = url + "getPageInfo.php";
        // 設定POST請求所需的參數
        WWWForm form = new();
        form.AddField("token", token);

        StartCoroutine(DownloadSceneInfo(SceneInfoUrl, form, (string result) => {
            Debug.Log(result);
            var json = JsonUtility.FromJson<RawScene>(result);
            this.result = json;
            onDownloaded.Invoke(json);
            

        }, (string error) => {
            Debug.LogError(error);
        }));
    }

    IEnumerator DownloadSceneInfo(string url, WWWForm form, System.Action<string> callback, System.Action<string> errorCallback = null)
    {
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();

        if (www.responseCode == 200)
        {
            callback(www.downloadHandler.text);
        }
        else
        {
            errorCallback?.Invoke(www.error);
        }
    }

    public void UpdateTransform(string json){
        var UpdateTransformUrl = url + "updateTransform.php";
        // 設定POST請求所需的參數
        WWWForm form = new();
        form.AddField("data", json);

        StartCoroutine(UpdateTransform(UpdateTransformUrl, form, (string result) => {
            Debug.Log(result);
        }, (string error) => {
            Debug.LogError(error);
        }));
    }

    IEnumerator UpdateTransform(string url, WWWForm form, System.Action<string> callback, System.Action<string> errorCallback = null)
    {
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();

        if (www.responseCode == 200)
        {
            callback(www.downloadHandler.text);
        }
        else
        {
            errorCallback?.Invoke(www.error);
        }
    }
}
