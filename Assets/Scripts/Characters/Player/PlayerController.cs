using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform raySource;
    private Position2D myPosition;
    private PlayerCharacter playerCharacter;

    private void Start()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
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

            // TODO - should work as requests
            // na pewno trafia
            if (hits.Length > 0)
            {
                TileInfo tile = hits[0].collider.gameObject.GetComponent<TileInfo>();
                GameController.Instance.RequestMoveTo(playerCharacter, tile);
                GameController.Instance.CheckPlayerOutEnergy();
            }
                
        }
    }
}
