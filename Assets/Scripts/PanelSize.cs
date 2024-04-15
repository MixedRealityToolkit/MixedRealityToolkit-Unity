using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSize : MonoBehaviour
{
    public RectTransform panel;
    public BoxCollider boxCollider;

    private void Start() {
        if(panel == null){
            panel = GetComponent<RectTransform>();
        }
        if(boxCollider == null){
            boxCollider = GetComponent<BoxCollider>();
        }
    }

    private void Update() {
        if(panel == null || boxCollider == null) return;
        boxCollider.size = new Vector3(panel.rect.width, panel.rect.height, 1);

    }
}
