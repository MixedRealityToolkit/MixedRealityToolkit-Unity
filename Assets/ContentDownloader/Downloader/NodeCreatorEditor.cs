using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(NodeCreator))]
public class NodeCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 繪製默認的inspector界面
        DrawDefaultInspector();

        NodeCreator nodeCreator = (NodeCreator)target;

        // 為Init方法添加一個按鈕
        if (GUILayout.Button("上傳Transform"))
        {
            nodeCreator.UpdateTransform();
        }
    }
}
#endif