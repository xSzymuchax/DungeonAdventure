using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    private Transform raySource;
    private PlayerCharacter playerCharacter;

    private bool isInterrupted = false;
    private bool stopRequested;
    private Coroutine moveRoutine;

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
        if (playerCharacter == null || playerCharacter.IsDead)
            return;

        isInterrupted = false;
        ScreenGesture.Poll();
        if (!ScreenGesture.Tap)
            return;

        if (moveRoutine != null)
        {
            stopRequested = true;
            return;
        }

        RaycastHit[] hits = ShootRay(ScreenGesture.Position);

        if (GameController.Instance != null && GameController.Instance.IsAiming)
        {
            TileInfo aimed = FindTile(hits);
            if (aimed != null)
            {
                if (GameController.Instance.IsThrowArmed)
                    GameController.Instance.ThrowArmedAt(aimed.position);
                else
                    GameController.Instance.UseArmedAt(aimed.position);
            }
            return;
        }

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

            OrderMove(tile.position);
        }
    }

    void OrderMove(Position2D destination)
    {
        stopRequested = false;
        playerCharacter.RecalculatePath(destination);
        moveRoutine = StartCoroutine(PlayerMove());
    }

    public IEnumerator PlayerWait()
    {
        if (playerCharacter == null || playerCharacter.IsDead || playerCharacter.Energy <= 0)
            yield break;

        if (GameController.Instance != null && GameController.Instance.IsAiming)
            yield break;
        stopRequested = true;
        if (playerCharacter.CurrentPath != null)
            playerCharacter.CurrentPath.Clear();

        yield return GameController.Instance.fightingSystem.TickTokens(playerCharacter);
        if (playerCharacter.IsDead || playerCharacter.Energy <= 0)
            yield break;

        float energy = playerCharacter.Energy;
        yield return GameController.Instance.RequestWaitTurn(playerCharacter);
        if (playerCharacter.Energy < energy)
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

        yield return fighting.TickTokens(playerCharacter);
        if (playerCharacter.IsDead)
            yield break;

        float energy = playerCharacter.Energy;
        yield return fighting.UseBasicAttack(playerCharacter, target);
        if (playerCharacter.Energy < energy)
            Durability.OnWeaponAttack(playerCharacter);
        yield return GameController.Instance.EvaluateTurn();
    }

    private IEnumerator PlayerMove()
    {
        while (!isInterrupted && !stopRequested && HasStep())
        {
            Position2D tile = playerCharacter.CurrentPath[0];
            if (GameController.Instance.dungeon.GetTileInfos()[tile.x, tile.y].isOccupied)
                break;

            yield return GameController.Instance.fightingSystem.TickTokens(playerCharacter);
            if (playerCharacter.IsDead || stopRequested)
                break;

            yield return GameController.Instance.movementSystem.Walk(playerCharacter, tile);
            yield return GameController.Instance.EvaluateTurn();
            if (playerCharacter.IsDead || stopRequested)
                break;

            float energy = playerCharacter.Energy;
            yield return GameController.Instance.TryPickupItem(tile);
            if (playerCharacter.Energy < energy)
                yield return GameController.Instance.EvaluateTurn();
            if (playerCharacter.IsDead || stopRequested)
                break;

            if (GameController.Instance.TryChangeFloor(tile))
                break;

            isInterrupted = GameController.Instance.CheckPlayerPerception();
            if (stopRequested || isInterrupted)
                break;

            if (HasStep())
                playerCharacter.CurrentPath.RemoveAt(0);
        }

        moveRoutine = null;
    }

    bool HasStep()
    {
        return playerCharacter.CurrentPath != null && playerCharacter.CurrentPath.Count > 0;
    }

    private bool CanWalkOn(FloorFieldType fieldType)
    {
        if (fieldType == FloorFieldType.SIDE_FIELD)
            return false;
        return true;
    }
}
