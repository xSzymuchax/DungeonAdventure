using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform raySource;
    private MoveCostManager moveCostManager;
    private Position2D myPosition;

    private void Start()
    {
        InitMoveCosts();
    }

    private void InitMoveCosts()
    {
        moveCostManager = new();
        moveCostManager.AddCost(FloorFieldType.BASE_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.CORRIDOR_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.POSSIBLE_DOOR_FIELD, 1);
    }

    public void SetRaySource(Transform transform)
    {
        raySource = transform;
    }

    public void SetPositionOnFloor(Position2D position)
    {
        myPosition = position;
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
            if (hits.Length > 0)
                GameController.Instance.RequestMoveTo();
        }
    }
}
