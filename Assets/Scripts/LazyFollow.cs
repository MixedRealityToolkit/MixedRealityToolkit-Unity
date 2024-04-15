using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazyFollow : MonoBehaviour
{
    public Transform target;
    public float speed = 0.1f;
    public float distance = 0.1f;

    public bool face = true;

    void Update()
    {
        if (target == null)
            return;

        Vector3 targetPos = target.position + target.forward * distance;
        transform.position = Vector3.Lerp(transform.position, targetPos, speed);

        if (face)
        {
            transform.LookAt(target);
        }
    }
}
