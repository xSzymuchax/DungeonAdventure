using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target;
    public Vector3 offset;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    private void Follow()
    {
        if (target == null)
            return;

        transform.position = target.position + offset;
        transform.LookAt(target);  
    }

    private void Update()
    {
        Follow();
    }
}
