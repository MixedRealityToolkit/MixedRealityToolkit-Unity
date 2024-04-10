using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Events;

public class Config : MonoBehaviour
{
    private static Config _instance;
    public static Config Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Config>();
                if (_instance == null)
                {
                    _instance = new GameObject("Config").AddComponent<Config>();
                }
            }
            return _instance;
        }
    }
    
    public enum Mode{
        edit,preview
    }

    public string token;
    public Mode mode;
    public string host = "https://horizonvision.ljthub.com/bg/";
    public FontConfig[] fonts;
    public Queue<ResourceQueue> downloadQueue = new Queue<ResourceQueue>();
    [SerializeField]
    private int downloadCount = 0;
    public int DownloadCount{
        get{
            return downloadCount;
        }
        set{
            downloadCount = value;
            if(downloadCount == 0){
                downloadQueue.Clear();
            }
        }
    }
    public bool isDownloading;
    private int totleDownloadCount;
    private int currentDownloadedCount;
    public float downloadProgress{
        get{
            if(totleDownloadCount == 0){
                return 0;
            }else{
                return (float)currentDownloadedCount / totleDownloadCount;
            }
        }
    }
    public UnityEvent<float> onDownloadProgress = new UnityEvent<float>();
    public UnityEvent<string> onDownloadProgressText = new UnityEvent<string>();


    public bool GetFont(string fontName,out Font font){
        foreach(FontConfig i in fonts){
            if(i.fontName == fontName){
                font = i.font;
                return true;
            }
        }
        if(fonts.Length > 0){
            font = fonts[0].font;
            return true;
        }else{
            font = null;
            return false;
        }
    }
    
    private void Start() {
        onDownloadProgress.Invoke(downloadProgress);
        onDownloadProgressText.Invoke(string.Format("{0}/{1}",currentDownloadedCount,totleDownloadCount));
    }

    private void Update() {
        downloadCount = downloadQueue.Count;
        if(downloadCount > 0 && !isDownloading){
            isDownloading = true;
            ResourceQueue rq = downloadQueue.Dequeue();
            StartCoroutine(DownloadResource(rq.url,rq.fileName,(string result) => {
                rq.callback(result);
                isDownloading = false;
                currentDownloadedCount++;
                onDownloadProgress.Invoke(downloadProgress);
                onDownloadProgressText.Invoke(string.Format("{0}/{1}",currentDownloadedCount,totleDownloadCount));
            }));

        }
    }

    public void AddDownloadQueue(string url,string fileName,System.Action<string> callback){
        ResourceQueue rq = new ResourceQueue();
        rq.url = url;
        rq.fileName = fileName;
        rq.callback = callback;
        downloadQueue.Enqueue(rq);
        totleDownloadCount++;
    }

    public IEnumerator DownloadResource(string url, string fileName, System.Action<string> callback)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log($"[DownloadResource] From: {url} To: {filePath}");

        if (File.Exists(filePath))
        {
            UnityWebRequest localFileRequest = UnityWebRequest.Get(url);
            yield return localFileRequest.SendWebRequest();

            if (localFileRequest.result == UnityWebRequest.Result.Success)
            {
                string localFileHash = ComputeHash(File.ReadAllBytes(filePath));
                string remoteFileHash = ComputeHash(localFileRequest.downloadHandler.data);

                if (localFileHash == remoteFileHash)
                {
                    // 文件已存在且相同，無需重新下載
                    callback(filePath);
                    yield break;
                }
            }
        }

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            File.WriteAllBytes(filePath, www.downloadHandler.data);
            callback(filePath);
        }
    }

    private string ComputeHash(byte[] data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashData = sha256.ComputeHash(data);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < hashData.Length; i++)
            {
                stringBuilder.Append(hashData[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
    
    [System.Serializable]
    public class FontConfig{
        public string fontName;
        public Font font;
    }

    public class ResourceQueue{
        public string url;
        public string fileName;
        public System.Action<string> callback;
    }

    public class TransformData{
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public TransformData(Transform transform){
            position = transform.position;
            rotation = transform.eulerAngles;
            scale = transform.localScale;
        }
    }

    public static string TransformToJson(Transform transform){
        return JsonUtility.ToJson(new TransformData(transform));
    }

    public static void TransformFromJson(Transform transform,string json){
        TransformData data = JsonUtility.FromJson<TransformData>(json);
        transform.position = data.position;
        transform.eulerAngles = data.rotation;
        transform.localScale = data.scale;
    }    
    public static Texture2D LoadPNG(string filePath) {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath)) 	{
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

}
