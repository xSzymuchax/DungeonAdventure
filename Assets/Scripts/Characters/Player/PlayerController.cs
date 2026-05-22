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

            if (hits.Length > 0)
            {
                TileInfo tile = hits[0].collider.gameObject.GetComponent<TileInfo>();
                if (!CanWalkOn(tile.type))
                    return;

                StartCoroutine(PlayerMove(tile.position));
            }
        }
    }

    private IEnumerator PlayerMove(Position2D tilePosition)
    {
        yield return GameController.Instance.StartCoroutine(GameController.Instance.RequestMoveTo(playerCharacter, tilePosition));
        yield return GameController.Instance.EvaluateTurn();
    }

    private bool CanWalkOn(FloorFieldType fieldType)
    {
        if (fieldType == FloorFieldType.SIDE_FIELD)
            return false;
        return true;
    }
}
