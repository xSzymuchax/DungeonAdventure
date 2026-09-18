using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
                if (TryStartAttackFromHits(hits))
                    return;

                TileInfo tile = FindTile(hits);
                if (tile == null)
                    return;

                if (TryStartAttack(tile.position))
                    return;

                if (!CanWalkOn(tile.type))
                    return;

                playerCharacter.RecalculatePath(tile.position);    
                StartCoroutine(PlayerMove());
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit[] hits = ShootRay(Input.mousePosition);
            if (hits.Length == 0)
                return;

            foreach (RaycastHit hit in hits)
            {
                Character clicked = hit.collider.GetComponentInParent<Character>();
                if (clicked != null && clicked != playerCharacter)
                {
                    StartCoroutine(PlayerFireball(clicked.Position));
                    return;
                }
            }

            TileInfo tile = FindTile(hits);
            if (tile != null)
                StartCoroutine(PlayerFireball(tile.position));
        }
    }

    private IEnumerator PlayerFireball(Position2D target)
    {
        FightingSystem fighting = GameController.Instance.fightingSystem;
        if (!fighting.CanUse(playerCharacter.Fireball, playerCharacter, target))
            yield break;

        fighting.TickTokens(playerCharacter);
        yield return fighting.UseSkill(playerCharacter.Fireball, playerCharacter, target);
        yield return GameController.Instance.EvaluateTurn();
    }

    public IEnumerator PlayerWait()
    {
        GameController.Instance.fightingSystem.TickTokens(playerCharacter);
        yield return GameController.Instance.RequestWaitTurn(playerCharacter);
        yield return GameController.Instance.EvaluateTurn();
    }

    private TileInfo FindTile(RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits)
        {
            TileInfo tile = hit.collider.GetComponent<TileInfo>();
            if (tile != null)
                return tile;
        }
        return null;
    }

    private bool TryStartAttackFromHits(RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits)
        {
            Character clicked = hit.collider.GetComponentInParent<Character>();
            if (clicked == null || clicked == playerCharacter)
                continue;
            if (clicked is IDamagable damagable)
            {
                StartCoroutine(PlayerAttack(damagable));
                return true;
            }
        }
        return false;
    }

    private bool TryStartAttack(Position2D position)
    {
        if (!GameController.Instance.dungeon.TryGetActorAt(position, out IActor occupant))
            return false;
        if (occupant == playerCharacter)
            return false;
        if (occupant is not IDamagable damagable)
            return true;

        StartCoroutine(PlayerAttack(damagable));
        return true;
    }

    private IEnumerator PlayerAttack(IDamagable target)
    {
        FightingSystem fighting = GameController.Instance.fightingSystem;
        if (!fighting.CanUse(playerCharacter.BasicAttack, playerCharacter, target))
            yield break;

        fighting.TickTokens(playerCharacter);
        yield return fighting.UseBasicAttack(playerCharacter, target);
        yield return GameController.Instance.EvaluateTurn();
    }

    private IEnumerator PlayerMove()
    {
        while (!isInterrupted && playerCharacter.CurrentPath.Count != 0)
        {
            Position2D tile = playerCharacter.CurrentPath[0];
            if (GameController.Instance.dungeon.GetTileInfos()[tile.x, tile.y].isOccupied)
                yield break;

            GameController.Instance.fightingSystem.TickTokens(playerCharacter);
            yield return GameController.Instance.movementSystem.Walk(playerCharacter, tile);
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
