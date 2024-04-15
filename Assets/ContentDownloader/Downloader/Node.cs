using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using ljt;

public class Node : MonoBehaviour
{
    public string jsonOverwrite;
    [SerializeField]
    private RawNode node;
    [SerializeField]
    private RawNodeJson rawNodeJson = new RawNodeJson();
    public NodeConfig config = new NodeConfig();

    public int id => node.web_id;
    public List<int> prevNodeId => node.prev.Split('_').Select(x => int.Parse(x)).ToList();
    public List<int> nextNodeId => node.next.Split('_').Select(x => int.Parse(x)).ToList();
    public List<Node> prevNode = new List<Node>();
    public List<Node> nextNode = new List<Node>();
    public UnityEvent onInit = new UnityEvent();
    public UnityEvent onEnd = new UnityEvent();
    public NodeSpawner spawner;
    public string json => spawner.TransformJson;

    public void Build(RawNode node){
        this.node = node;
        rawNodeJson = JsonUtility.FromJson<RawNodeJson>(node.json);
        
        
        if(rawNodeJson != null){
            rawNodeJson.transform = node.transform;
            config.Format(rawNodeJson);
        }
        

    }

    private bool spawned = false;
    public void Init(){
        if(!spawned){
            spawner.Spawn(transform.parent.gameObject);
            spawned = true;
        }
            
        spawner.Canvas.gameObject.SetActive(true);
        gameObject.SetActive(true);
        Debug.Log("Init Node "+id);
        onInit.Invoke();

        Debug.Log($"Node {node.web_id}:\n{node.next.Split('_').Length} next nodes ({node.next})\n{node.prev.Split('_').Length} prev nodes ({node.prev})");
        foreach(string i in node.next.Split('_')){
            if(i != ""){
                int next = int.Parse(i);
                if(NodeCreator.Instance.nodes.ContainsKey(next)){
                    nextNode.Add(NodeCreator.Instance.nodes[next]);
                    
                    // 在lambda表達式外部創建一個新的局部變量
                    int nextCopy = next;
                    onEnd.AddListener(() => {
                        NodeCreator.Instance.nodes[nextCopy].Init();
                    });
                }else{
                    Debug.Log("Node "+next+" not found");
                
                }
            }
        }


        foreach(string i in node.prev.Split('_')){
            if(i != ""){
                int prev = int.Parse(i);
                if(NodeCreator.Instance.nodes.ContainsKey(prev)){
                    prevNode.Add(NodeCreator.Instance.nodes[prev]);
                    // 在lambda表達式外部創建一個新的局部變量
                    int prevCopy = prev;
                    // onInit.AddListener(() => {
                    //     NodeCreator.Instance.nodes[prevCopy].End();
                    // });
                }
            }
        }

    }

    private void Start() {
        if(jsonOverwrite.Length > 0){
            node = new RawNode();
            node.json = jsonOverwrite;
            node.web_id = 99;
            rawNodeJson = JsonUtility.FromJson<RawNodeJson>(node.json);
            config.Format(rawNodeJson);
            Init();
        }
    }

    public void End(){
        spawner.Ending();
        gameObject.SetActive(false);
        Debug.Log("End Node "+id);
        onEnd.Invoke();
    }

    public enum NodeType{
        Text,Image,Video
    }
    [System.Serializable]
    public class RawNodeJson{
        public float width;
        public float height;
        public string transform;
        public List<RawSpace> textSpaces;
        public List<RawSpace> mediaSpaces;
        public string selectedColor;
        public string selectedTransparency;
    }

    [System.Serializable]
    public class RawSpace{
        public float x;
        public float y;
        public float width;
        public float height;
        public string content;
        public string fontSize;
        public string fontFamily;
        public string textAlign;
        public string type;
        public string url;
        public string color;
    }

    [System.Serializable]
    public class NodeConfig{
        public Vector2 rect;
        public List<Space> spaces;
        public string transform;
        public Color backgroundColor;


        public void Format(RawNodeJson raw){
            this.rect = new Vector2(raw.width, raw.height);
            spaces = new List<Space>();
            transform = raw.transform;
            ColorUtility.TryParseHtmlString(raw.selectedColor,out backgroundColor);
            backgroundColor.a = float.Parse(raw.selectedTransparency)*255;

            foreach(RawSpace space in raw.textSpaces){
                Font spaceFont;
                if(Config.Instance.GetFont(space.fontFamily,out Font font)){
                        spaceFont = font;
                    }
                    else{
                        spaceFont = null;
                    }
                TextAnchor align;
                switch(space.textAlign){
                    case "left":
                        align = TextAnchor.MiddleLeft;
                        break;
                    case "center":
                        align = TextAnchor.MiddleCenter;
                        break;
                    case "right":
                        align = TextAnchor.MiddleRight;
                        break;
                    default:
                        align = TextAnchor.MiddleCenter;
                        break;
                }
                
                Color color1;
                ColorUtility.TryParseHtmlString(space.color,out color1);
                spaces.Add(new Space(){
                    color = color1,
                    type = NodeType.Text,
                    rect = new Vector2(space.width, space.height),
                    content = space.content,
                    fontSize = int.Parse(space.fontSize.Replace("px","")),
                    font = spaceFont,
                    align = align,
                    posision = new Vector2(space.x,space.y)
                });
            }

            foreach(RawSpace space in raw.mediaSpaces){
                string format;
                switch(space.type){
                    case "video/mp4":
                        format = "mp4";
                        break;
                    case "image/jpeg":
                        format = "jpeg";
                        break;
                    case "image/png":
                        format = "png";
                        break;
                    case "image/jpg":
                        format = "jpg";
                        break;
                    default:
                        format = "";
                        break;
                }
                spaces.Add(new Space(){
                    type = space.type.Contains("image") ? NodeType.Image : NodeType.Video,
                    rect = new Vector2(space.width, space.height),
                    format = format,
                    url = space.url,
                    posision = new Vector2(space.x,space.y)
                });
            }
        }
    }

    [System.Serializable]
    public class Space{
        public NodeType type;
        public Vector2 rect;
        public Vector2 posision;
        public string content;
        public int fontSize;
        public Font font;
        public TextAnchor align;
        public string filePath;
        public string url{
            set{
                Config.Instance.AddDownloadQueue(value,value.Split('/').Last()+"."+format,(string result) => {
                    filePath = result;
                });
            }
        }
        public string format;
        public Texture2D texture;
        public VideoClip video;
        public Color color;

        

    }
}
