using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class RawScene{
    public int success;
    public string log;
    public List<int> groups;
    public List<RawNode> heads;
    public List<RawNode> bodys; // 假設body的結構目前未知，所以這裡使用object類型
}

[System.Serializable]
public class RawNode
{
    public int id;
    public int sid;
    public string prev;
    public string next;
    [TextArea(3, 10)]
    public string transform; // 假設transform的結構目前未知，所以這裡使用object類型
    public int groupid;
    public string head;
    public string position;
    public int web_id;
    [TextArea(3, 10)]
    public string json; // 更新為直接引用HeadJson類別
    public string hash;

}

[System.Serializable]
public class HeadJson
{
    public bool isBoundaryVisible;
    public bool isResizing;
    public int width;
    public int height;
    public string scale;
    public List<TextSpace> textSpaces;
    public List<MediaSpace> mediaSpaces;
    public int startDragX;
    public int startDragY;
    public string y; // 假設為string類型，具體類型根據實際情況確定
    public string x; // 假設為string類型，具體類型根據實際情況確定
}

[System.Serializable]
public class TextSpace
{
    public long id;
    public int x;
    public int y;
    public int width;
    public int height;
    public string content;
    public bool isBoundaryVisible;
    public string fontSize;
    public string fontFamily;
    public string textAlign;
    public float ratio; // 假設ratio的結構目前未知，所以這裡使用object類型
    public string type;
    public bool isResizing;
    public int startDragX;
    public int startDragY;
    public int startX;
    public int startY;
    // 對於TextSpace特有的屬性，如color，也需要在這裡定義
    public string color; // 假設為string類型，具體類型根據實際情況確定
}

[System.Serializable]
public class MediaSpace
{
    public long id;
    public int x;
    public int y;
    public int width;
    public int height;
    public object file; // 假設file的結構目前未知，所以這裡使用object類型
    public string type;
    public string url;
    public int file_width;
    public int file_height;
    public bool isBoundaryVisible;
    public float ratio;
    public int startX;
    public int startY;
    public bool isResizing;
    public int startDragX;
    public int startDragY;
}
