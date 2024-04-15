using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCameraHeight : MonoBehaviour
{
    public Transform target;
    public float speed = 0.1f;
    public float distance = 0.1f;

    public bool x, y, z;

    public bool face = true;

    void Update()
    {
        if (target == null)
            return;

        Vector3 pos = transform.position;
        Vector3 targetPos = target.position;

        if (x)
            pos.x = Mathf.Lerp(pos.x, targetPos.x, speed);
        if (y)
            pos.y = Mathf.Lerp(pos.y, targetPos.y, speed);
        if (z)
            pos.z = Mathf.Lerp(pos.z, targetPos.z, speed);

        transform.position = pos;

        if (face)
            transform.LookAt(target);
    }
}
