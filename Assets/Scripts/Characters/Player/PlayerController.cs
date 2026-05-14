using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform raySource;

    public void SetRaySource(Transform transform)
    {
        raySource = transform;
    }

    private RaycastHit[] ShootRay(Vector2 contactPoint)
    {
        Camera cam = raySource.GetComponent<Camera>();
        Ray ray = cam.ScreenPointToRay(contactPoint);
        return Physics.RaycastAll(ray);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hits = ShootRay(Input.mousePosition);
            
            // na pewno trafia

        }
    }
}
