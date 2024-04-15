using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowQrcode : MonoBehaviour
{
    private void Update() {
        var list = GameObject.FindWithTag("Root");
        if(list == null) return;
        if(transform.parent == list.transform) return;
        Transform target = list.transform;
        transform.SetParent(target.parent);
        transform.position = target.position;
    }
}
