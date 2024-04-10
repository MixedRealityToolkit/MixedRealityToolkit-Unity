using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;


public class NodeSpawner : MonoBehaviour
{
    public Node node;
    public Node.NodeConfig config => node.config;
    public GameObject textPrefab,imagePrefab,videoPrefab;
    public Image background;
    public Transform Canvas;
    public Transform CanvasParent=>Canvas.parent.parent;
    public RectTransform CanvasRect;
    public Camera cam;
    public float spawnOffset = 0.5f;
    public List<GameObject> textSpaces = new List<GameObject>();
    

    [SerializeField][TextArea(3, 10)]
    public string TransformJson;

    public void Ending(){
        Canvas.gameObject.SetActive(false);
    }
    public void Spawn(){
        rect = config.rect;
        Debug.Log(config.transform);
        if(config.transform == null || config.transform == ""){
            CanvasParent.position = cam.transform.position + cam.transform.forward * spawnOffset;
            CanvasParent.rotation = new Quaternion(0, 0, 0, 0);
            CanvasParent.localScale = new Vector3(1, 1, 1);
            Debug.Log(CanvasParent.name);
        }else{
            Config.TransformFromJson(CanvasParent, config.transform);
        }
        background.color = config.backgroundColor;

        foreach(var space in config.spaces){
            switch(space.type){
                case Node.NodeType.Text:
                    var text = Instantiate(textPrefab, Canvas);
                    text.SetActive(true);
                    textSpaces.Add(text);
                    Text text1 = text.GetComponent<Text>();
                    text1.text = space.content;
                    text1.fontSize = space.fontSize;
                    text1.font = space.font;
                    text1.alignment = space.align;
                    text.GetComponent<RectTransform>().sizeDelta = space.rect;
                    text.GetComponent<RectTransform>().anchoredPosition = new Vector2(space.posision.x+space.rect.x/2,-space.posision.y-space.rect.y/2);
                    break;
                case Node.NodeType.Image:
                    var image = Instantiate(imagePrefab, Canvas);
                    image.SetActive(true);
                    RawImage rawImage = image.GetComponent<RawImage>();
                    //load texture from file path
                    Debug.Log("load file:"+ space.filePath);
                    rawImage.texture = Config.LoadPNG(space.filePath);
                    image.GetComponent<RectTransform>().sizeDelta = space.rect;
                    image.GetComponent<RectTransform>().anchoredPosition = new Vector2(space.posision.x+space.rect.x/2,-space.posision.y-space.rect.y/2);
                    break;
                case Node.NodeType.Video:
                    var video = Instantiate(videoPrefab, Canvas);
                    video.SetActive(true);
                    VideoPlayer videoPlayer = video.GetComponent<VideoPlayer>();
                    //load video from file path
                    Debug.Log("load file:"+ space.filePath);
                    //create a render texture
                    RenderTexture renderTexture = new RenderTexture((int)space.rect.x, (int)space.rect.y, 24);
                    videoPlayer.targetTexture = renderTexture;
                    videoPlayer.url = space.filePath;
                    videoPlayer.isLooping = true;
                    videoPlayer.Play();

                    RawImage rawImage1 = video.GetComponent<RawImage>();
                    rawImage1.texture = renderTexture;
                    
                    video.GetComponent<RectTransform>().sizeDelta = space.rect;
                    video.GetComponent<RectTransform>().anchoredPosition = new Vector2(space.posision.x+space.rect.x/2,-space.posision.y-space.rect.y/2);
                    break;
            
            }
        }
    }

    private void Update() {
        if(CanvasParent == null){
            return;
        }
        TransformJson = Config.TransformToJson(CanvasParent);
    }

    public Vector2 rect{
        set{
            CanvasRect.sizeDelta = value;
        }
        get{
            return CanvasRect.sizeDelta;
        }
    }
}
