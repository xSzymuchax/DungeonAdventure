using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target;
    public Vector3 offset;

    private Vector3 velocity;
    private readonly float smoothTime = 0.1f;

    public void SetTarget(Transform target)
    {
        this.target = target;
        //transform.LookAt(target);

    }

    private void Follow()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private void Update()
    {
        Follow();
    }
}
