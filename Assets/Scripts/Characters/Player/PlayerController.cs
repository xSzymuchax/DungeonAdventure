using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform raySource;
    private PlayerCharacter playerCharacter;

    private bool isInterrupted = false;

    private void Start()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
    }

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
        isInterrupted = false;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hits = ShootRay(Input.mousePosition);

            if (hits.Length > 0)
            {
                TileInfo tile = hits[0].collider.gameObject.GetComponent<TileInfo>();
                if (!CanWalkOn(tile.type))
                    return;

                playerCharacter.RecalculatePath(tile.position);    
                StartCoroutine(PlayerMove());
            }
        }
    }

    private IEnumerator PlayerMove()
    {
        while (!isInterrupted && playerCharacter.CurrentPath.Count != 0)
        {
            Position2D tile = playerCharacter.CurrentPath[0];
            yield return GameController.Instance.StartCoroutine(GameController.Instance.RequestMoveTo(playerCharacter, tile));
            yield return GameController.Instance.EvaluateTurn();
            isInterrupted = GameController.Instance.CheckPlayerPerception();
            playerCharacter.CurrentPath.RemoveAt(0);
        }
            
    }

    private bool CanWalkOn(FloorFieldType fieldType)
    {
        if (fieldType == FloorFieldType.SIDE_FIELD)
            return false;
        return true;
    }
}
