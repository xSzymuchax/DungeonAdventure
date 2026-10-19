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
    private bool actionActive;
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
            if (GameController.Instance.IsInspectArmed)
            {
                GameController.Instance.Inspect(hits);
                return;
            }

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

        if (playerCharacter.IsFrozen())
        {
            StartCoroutine(PlayerWait());
            return;
        }

        if (GameController.Instance != null && GameController.Instance.WorkbenchOpen)
            return;

        if (hits.Length > 0)
        {
            if (TryStartAttackFromHits(hits))
                return;

            if (TryOpenWorkbench(FindBench(hits)))
                return;

            TileInfo tile = FindTile(hits);
            if (tile == null)
                return;

            if (TryOpenWorkbench(GameController.Instance.WorkbenchAt(tile.position)))
                return;

            if (TryStartAttack(tile.position))
                return;

            if (TryUnlockSteelDoor(tile))
                return;

            if (TryUseChest(tile))
                return;

            if (tile.position.x == playerCharacter.Position.x && tile.position.y == playerCharacter.Position.y)
            {
                if (GameController.Instance.dungeon.HasGroundItem(tile.position))
                    GameController.Instance.BeginPlayerAction(new PickupAction(playerCharacter, tile.position, GameController.Instance.dungeon));
                return;
            }

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

        yield return SpendWait();
    }

    IEnumerator SpendWait()
    {
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
                BeginAttack(damagable);
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

        BeginAttack(damagable);
        return true;
    }

    CraftBench FindBench(RaycastHit[] hits)
    {
        for (int i = 0; i < hits.Length; i++)
        {
            CraftBench bench = hits[i].collider.GetComponent<CraftBench>();
            if (bench != null)
                return bench;
        }

        return null;
    }

    bool TryOpenWorkbench(CraftBench bench)
    {
        if (bench == null)
            return false;

        BeginWorkbench(bench);
        return true;
    }

    bool TryUnlockSteelDoor(TileInfo tile)
    {
        if (tile == null || tile.tileObject is not Door door)
            return false;
        if (door.Lock == null || door.Lock.Kind != PadlockKind.Steel || !door.Lock.Engaged)
            return false;
        if (SteelKeyIndex() < 0)
            return false;

        BeginUnlock(door);
        return true;
    }

    void BeginUnlock(Door door)
    {
        stopRequested = false;
        actionActive = false;
        moveRoutine = StartCoroutine(PlayerUnlock(door));
        if (!actionActive)
            moveRoutine = null;
    }

    IEnumerator PlayerUnlock(Door door)
    {
        actionActive = true;
        while (!isInterrupted && !stopRequested && !playerCharacter.IsDead && door != null && playerCharacter.Energy > 0)
        {
            if (playerCharacter.IsFrozen())
            {
                yield return SpendWait();
                break;
            }

            if (playerCharacter.Position.ChebyshevTo(door.Position) == 1)
            {
                int key = SteelKeyIndex();
                if (key >= 0)
                    yield return GameController.Instance.PlayAction(new UseKeyAction(playerCharacter, key, door.Position));
                break;
            }

            playerCharacter.RecalculatePath(door.Position);
            if (!HasStep())
                break;

            Position2D step = playerCharacter.CurrentPath[0];
            if (step.x == door.Position.x && step.y == door.Position.y)
                break;

            TileInfo tileInfo = GameController.Instance.dungeon.GetTileInfos()[step.x, step.y];
            if (tileInfo == null || tileInfo.isOccupied || GameController.Instance.dungeon.PassageBlocked(step))
                break;

            Position2D before = playerCharacter.Position;
            yield return GameController.Instance.fightingSystem.TickTokens(playerCharacter);
            if (playerCharacter.IsDead || stopRequested)
                break;

            yield return GameController.Instance.movementSystem.Walk(playerCharacter, step);
            yield return GameController.Instance.EvaluateTurn();
            if (playerCharacter.IsDead || stopRequested)
                break;
            if (playerCharacter.Position.x == before.x && playerCharacter.Position.y == before.y)
                break;

            if (GameController.Instance.TryChangeFloor(step))
                break;

            isInterrupted = GameController.Instance.CheckPlayerPerception();
        }

        actionActive = false;
        moveRoutine = null;
    }

    int SteelKeyIndex()
    {
        if (playerCharacter.Inventory == null)
            return -1;

        IReadOnlyList<Item> bag = playerCharacter.Inventory.Bag;
        for (int i = 0; i < bag.Count; i++)
        {
            if (DoorItems.IsSteelKey(bag[i]))
                return i;
        }

        return -1;
    }

    bool TryUseChest(TileInfo tile)
    {
        if (tile == null || tile.tileObject is not Chest chest)
            return false;

        if (chest.Lock != null && chest.Lock.Kind == PadlockKind.Gold && chest.Lock.Engaged && GoldKeyIndex() >= 0)
        {
            BeginChest(chest, ChestUse.GoldKey);
            return true;
        }

        if (!chest.IsOpen && (chest.Lock == null || !chest.Lock.BlocksPassage))
        {
            BeginChest(chest, ChestUse.Open);
            return true;
        }

        if (chest.IsOpen && chest.HasItems)
        {
            BeginChest(chest, ChestUse.Take);
            return true;
        }

        return false;
    }

    void BeginChest(Chest chest, ChestUse use)
    {
        stopRequested = false;
        actionActive = false;
        moveRoutine = StartCoroutine(PlayerChest(chest, use));
        if (!actionActive)
            moveRoutine = null;
    }

    IEnumerator PlayerChest(Chest chest, ChestUse use)
    {
        actionActive = true;
        while (!isInterrupted && !stopRequested && !playerCharacter.IsDead && chest != null && playerCharacter.Energy > 0)
        {
            if (playerCharacter.IsFrozen())
            {
                yield return SpendWait();
                break;
            }

            if (playerCharacter.Position.ChebyshevTo(chest.Position) == 1)
            {
                int key = use == ChestUse.GoldKey ? GoldKeyIndex() : -1;
                yield return GameController.Instance.PlayAction(new ChestAction(playerCharacter, chest.Position, use, key));
                break;
            }

            playerCharacter.RecalculatePath(chest.Position);
            if (!HasStep())
                break;

            Position2D step = playerCharacter.CurrentPath[0];
            if (step.x == chest.Position.x && step.y == chest.Position.y)
                break;

            TileInfo tileInfo = GameController.Instance.dungeon.GetTileInfos()[step.x, step.y];
            if (tileInfo == null || tileInfo.isOccupied || GameController.Instance.dungeon.PassageBlocked(step))
                break;

            Position2D before = playerCharacter.Position;
            yield return GameController.Instance.fightingSystem.TickTokens(playerCharacter);
            if (playerCharacter.IsDead || stopRequested)
                break;

            yield return GameController.Instance.movementSystem.Walk(playerCharacter, step);
            yield return GameController.Instance.EvaluateTurn();
            if (playerCharacter.IsDead || stopRequested)
                break;
            if (playerCharacter.Position.x == before.x && playerCharacter.Position.y == before.y)
                break;

            if (GameController.Instance.TryChangeFloor(step))
                break;

            isInterrupted = GameController.Instance.CheckPlayerPerception();
        }

        actionActive = false;
        moveRoutine = null;
    }

    int GoldKeyIndex()
    {
        if (playerCharacter.Inventory == null)
            return -1;

        IReadOnlyList<Item> bag = playerCharacter.Inventory.Bag;
        for (int i = 0; i < bag.Count; i++)
        {
            if (DoorItems.IsGoldKey(bag[i]))
                return i;
        }

        return -1;
    }

    void BeginWorkbench(CraftBench bench)
    {
        stopRequested = false;
        actionActive = false;
        moveRoutine = StartCoroutine(PlayerWorkbench(bench));
        if (!actionActive)
            moveRoutine = null;
    }

    IEnumerator PlayerWorkbench(CraftBench bench)
    {
        actionActive = true;
        while (!isInterrupted && !stopRequested && !playerCharacter.IsDead && bench != null && playerCharacter.Energy > 0)
        {
            if (playerCharacter.IsFrozen())
            {
                yield return SpendWait();
                break;
            }

            if (playerCharacter.Position.ChebyshevTo(bench.Position) == 1)
            {
                yield return GameController.Instance.PlayAction(new OpenWorkbenchAction(playerCharacter, bench));
                break;
            }

            playerCharacter.RecalculatePath(bench.Position);
            if (!HasStep())
                break;

            Position2D step = playerCharacter.CurrentPath[0];
            if (step.x == bench.Position.x && step.y == bench.Position.y)
                break;

            TileInfo tileInfo = GameController.Instance.dungeon.GetTileInfos()[step.x, step.y];
            if (tileInfo == null || tileInfo.isOccupied || GameController.Instance.dungeon.PassageBlocked(step))
                break;

            Position2D before = playerCharacter.Position;
            yield return GameController.Instance.fightingSystem.TickTokens(playerCharacter);
            if (playerCharacter.IsDead || stopRequested)
                break;

            yield return GameController.Instance.movementSystem.Walk(playerCharacter, step);
            yield return GameController.Instance.EvaluateTurn();
            if (playerCharacter.IsDead || stopRequested)
                break;
            if (playerCharacter.Position.x == before.x && playerCharacter.Position.y == before.y)
                break;

            if (GameController.Instance.TryChangeFloor(step))
                break;

            isInterrupted = GameController.Instance.CheckPlayerPerception();
        }

        actionActive = false;
        moveRoutine = null;
    }

    void BeginAttack(IDamagable target)
    {
        stopRequested = false;
        actionActive = false;
        moveRoutine = StartCoroutine(PlayerAttack(target));
        if (!actionActive)
            moveRoutine = null;
    }

    private IEnumerator PlayerAttack(IDamagable target)
    {
        actionActive = true;
        FightingSystem fighting = GameController.Instance.fightingSystem;
        while (!isInterrupted && !stopRequested && !playerCharacter.IsDead && target != null && !target.IsDead && playerCharacter.Energy > 0)
        {
            if (playerCharacter.IsFrozen())
            {
                yield return SpendWait();
                break;
            }

            if (fighting.CanUse(playerCharacter.BasicAttack, playerCharacter, target))
            {
                yield return fighting.TickTokens(playerCharacter);
                if (playerCharacter.IsDead || stopRequested)
                    break;

                float energy = playerCharacter.Energy;
                yield return fighting.UseBasicAttack(playerCharacter, target);
                if (playerCharacter.Energy < energy)
                    Durability.OnWeaponAttack(playerCharacter);
                yield return GameController.Instance.EvaluateTurn();
                break;
            }

            if (target is not IHasPosition positioned)
                break;

            playerCharacter.RecalculatePath(positioned.Position);
            if (!HasStep())
                break;

            Position2D step = playerCharacter.CurrentPath[0];
            if (step.x == positioned.Position.x && step.y == positioned.Position.y)
                break;

            TileInfo tileInfo = GameController.Instance.dungeon.GetTileInfos()[step.x, step.y];
            if (tileInfo == null || tileInfo.isOccupied || GameController.Instance.dungeon.PassageBlocked(step))
                break;

            Position2D before = playerCharacter.Position;
            yield return GameController.Instance.fightingSystem.TickTokens(playerCharacter);
            if (playerCharacter.IsDead || stopRequested)
                break;

            yield return GameController.Instance.movementSystem.Walk(playerCharacter, step);
            yield return GameController.Instance.EvaluateTurn();
            if (playerCharacter.IsDead || stopRequested)
                break;
            if (playerCharacter.Position.x == before.x && playerCharacter.Position.y == before.y)
                break;

            if (GameController.Instance.TryChangeFloor(step))
                break;

            isInterrupted = GameController.Instance.CheckPlayerPerception();
        }

        actionActive = false;
        moveRoutine = null;
    }

    private IEnumerator PlayerMove()
    {
        while (!isInterrupted && !stopRequested && HasStep())
        {
            if (playerCharacter.IsFrozen())
            {
                yield return SpendWait();
                break;
            }

            Position2D tile = playerCharacter.CurrentPath[0];
            if (GameController.Instance.dungeon.GetTileInfos()[tile.x, tile.y].isOccupied
                || GameController.Instance.dungeon.PassageBlocked(tile))
                break;

            yield return GameController.Instance.fightingSystem.TickTokens(playerCharacter);
            if (playerCharacter.IsDead || stopRequested)
                break;

            yield return GameController.Instance.movementSystem.Walk(playerCharacter, tile);
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
