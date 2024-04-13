using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NodeCreator : MonoBehaviour
{
    public static NodeCreator _Instance;
    public static NodeCreator Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<NodeCreator>();
                if (_Instance == null)
                {
                    _Instance = new GameObject("NodeCreator").AddComponent<NodeCreator>();
                }
            }
            return _Instance;
        }
    }
    public GameObject spawner,prefab,groupPrefab;
    public Dictionary<int, GameObject> groups = new Dictionary<int, GameObject>();
    [SerializeField]
    public NodeDict nodes = new NodeDict();
    public UnityEvent<string> onTransformUpdate = new UnityEvent<string>();
    public void CreateScene(RawScene scene)
    {
        foreach(int group in scene.groups){
            var groupObj = Instantiate(groupPrefab);
            groupObj.name = "Group " + group;
            groupObj.transform.parent = spawner.transform;
            groupObj.transform.localScale = new Vector3(1, 1, 1);
            groups.Add(group, groupObj);

            foreach(RawNode node in scene.heads){
                if(node.groupid == group){
                    var nodeObj = CreateNode(node);
                    nodeObj.transform.parent = groupObj.transform;
                }
            }

            foreach(RawNode node in scene.bodys){
                if(node.groupid == group){
                    var nodeObj = CreateNode(node);
                    nodeObj.transform.parent = groupObj.transform;
                    // nodeObj.SetActive(false);
                }
            }
        }
    }

    public GameObject CreateNode(RawNode node)
    {
        var nodeObj = Instantiate(prefab);
        var Node = nodeObj.GetComponent<Node>();
        nodeObj.name = "Node " + node.web_id;   
        nodeObj.transform.position = new Vector3(0, 0, 0);
        nodeObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        Debug.Log(nodeObj.transform.localScale);
        // Debug.Log(nodeObj.transform.lossyScale);

        Node.Build(node);

        nodes.Add(node.id,node.web_id, Node);
        return nodeObj;
    }

    public void UpdateTransform(){
        // Debug.Log(nodes.GetNodeTransforms());
        onTransformUpdate.Invoke(nodes.GetNodeTransforms());
    }

    [System.Serializable]
    public class NodeKeyValue{
        public int id;
        public int key;
        public Node value;
        public string GetNodeTransform(){
            return value.json;
        }
    }

    [System.Serializable]
    public class NodeDict{
        public List<NodeKeyValue> nodes;
        public void Add(int id, int key, Node value){
            nodes.Add(new NodeKeyValue{key = key, value = value, id = id});
        }
        public Node this[int key]{
            get{
                return nodes.Find(x => x.key == key).value;
            }
        }
        public bool ContainsKey(int key){
            return nodes.Exists(x => x.key == key);
        }
        public string GetNodeTransforms(){
            string json = "{";
            foreach(NodeKeyValue node in nodes){
                json += "\"" + node.id + "\":" + node.GetNodeTransform() + ",";
            }
            json = json.Remove(json.Length - 1);
            json += "}";
            return json;
        }
    }
}
