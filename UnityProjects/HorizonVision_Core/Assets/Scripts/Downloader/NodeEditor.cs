using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Node))]
public class NodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 繪製默認的inspector界面
        DrawDefaultInspector();

        Node nodeScript = (Node)target;

        // 為Init方法添加一個按鈕
        if (GUILayout.Button("啟動 Init"))
        {
            nodeScript.Init();
        }
        if(GUILayout.Button("啟動 End"))
        {
            nodeScript.End();
        }
    }
}
#endif

