using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;

public enum EnemyState
{
    SLEEPING, WANDERING, CHASING
}

public class EnemyController : MonoBehaviour, IEnemyController, IPerceptionUser
{
    public EnemyState myState = EnemyState.SLEEPING;

    private EnemyCharacter myCharacter;
    private IActor chasedCharacter;
    private bool shouldRecalculateFOV = false;

    private float BASE_WAKE_UP_CHANCE = 0.01f;
    private float BASE_DETECTION_CHANCE = 0.67f;
    private float GUARANTEED_WAKE_UP_DISTANCE = 3;
    private int CIRCLE_CHASE_DISTANCE = 2;

    public IActor Actor => myCharacter;
    private HashSet<Position2D> seenFields = new();
    private readonly List<Position2D> chaseTrail = new();
    private Position2D lastKnownPlayerPosition;
    private bool hasLastKnownPlayerPosition;

    private int maxNotDoAction = 5;
    private int currentNotDoAction = 0;
    private void Start()
    {
        myCharacter = GetComponent<EnemyCharacter>();
    }

    public void SetChasedCharacter(IActor chasedActor)
    {
        chasedCharacter = chasedActor;
    }

    public void OnDamaged()
    {
        if (myCharacter == null)
            myCharacter = GetComponent<EnemyCharacter>();
        seenFields = SeenFields();
        BeginChase();
    }

    private IEnumerator MoveTowardsChasedCharacter(Position2D target)
    {
        yield return GameController.Instance.movementSystem.Walk(myCharacter, target);
    }

    private IEnumerator WaitATurn()
    {
        yield return GameController.Instance.RequestWaitTurn(myCharacter);
    }

    private Vector3 TileCenter(Position2D tile)
    {
        return new Vector3(
            tile.x * Consts.TILE_SIZE + 0.5f * Consts.TILE_SIZE,
            1,
            tile.y * Consts.TILE_SIZE + 0.5f * Consts.TILE_SIZE);
    }

    private void ReplaceMarker()
    {
        if (myCharacter == null || myCharacter.CurrentPath == null || myCharacter.CurrentPath.Count == 0)
            return;

        Gizmos.DrawSphere(TileCenter(myCharacter.CurrentPath[myCharacter.CurrentPath.Count - 1]), 5);
    }

    private void DrawLastKnownPlayerGizmo()
    {
        if (myCharacter == null || chasedCharacter == null)
            return;
        if (myState != EnemyState.CHASING || !hasLastKnownPlayerPosition)
            return;
        if (IsPlayerVisible())
            return;

        Vector3 lastKnown = TileCenter(lastKnownPlayerPosition);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(lastKnown, Consts.TILE_SIZE * 0.35f);
        Gizmos.DrawWireCube(lastKnown, Vector3.one * Consts.TILE_SIZE);
        Gizmos.DrawLine(transform.position, lastKnown);

        Gizmos.color = new Color(1f, 0.5f, 1f, 0.8f);
        for (int i = 0; i < chaseTrail.Count; i++)
            Gizmos.DrawWireCube(TileCenter(chaseTrail[i]), Vector3.one * Consts.TILE_SIZE * 0.7f);
    }

    public IEnumerator MakeMove() // powinien zwracac parda/falsz jesli sie udalo wykonac akcje
    {
        Debug.Log("Enemy making move");
        // Debug.Log(myState);

        seenFields = SeenFields();
        yield return GameController.Instance.fightingSystem.TickTokens(myCharacter);
        if (myCharacter.IsDead)
            yield break;

        if (myState == EnemyState.SLEEPING)
        {
            if (UnityEngine.Random.value <= CalculateWakeUpChance())
                WakeUp();
            yield return WaitATurn();

            Debug.Log("sleeping, waiting");
            yield break;
        }
        else if (myState == EnemyState.WANDERING)
        {
            if (CanDetect(chasedCharacter) && UnityEngine.Random.value <= CalculateDetectionChance())
            {
                Debug.Log("wandering - detected player, chasing");
                BeginChase();
            }
            else
            {
                Debug.Log("wandering - making step");
                yield return GameController.Instance.movementSystem.Walk(myCharacter, GetMyNextPathStep());
                currentNotDoAction = 0;
                yield break;
            }
        }

        if (myState == EnemyState.CHASING)
        {
            yield return FollowChasePath();
            currentNotDoAction = 0;
            yield break;
        }

        currentNotDoAction++;
        // if cannot do anything, wait a turn
        if (currentNotDoAction >= 5)
        {
            currentNotDoAction = 0;
            Debug.Log("cant do any moves");
            yield return WaitATurn();
        }    
    }



    private void BeginChase()
    {
        myState = EnemyState.CHASING;
        chaseTrail.Clear();
        RememberPlayerLocation();
    }

    private bool IsPlayerVisible()
    {
        return seenFields.Contains(chasedCharacter.Position);
    }

    private int Chebyshev(Position2D a, Position2D b)
    {
        return Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
    }

    private void SetLastKnown(Position2D tile)
    {
        lastKnownPlayerPosition = tile;
        hasLastKnownPlayerPosition = true;
    }

    private void RememberPlayerLocation()
    {
        Position2D current = chasedCharacter.Position;
        Position2D previous = chasedCharacter.LastPosition;
        bool seesCurrent = seenFields.Contains(current);
        bool seesPrevious = seenFields.Contains(previous);

        // Player already moved this turn. If they stepped out of FOV, the last
        // tile we actually saw them on is previous — never the fog tile.
        if (seesPrevious && !seesCurrent)
        {
            SetLastKnown(previous);
            RecordTrailTile(previous);
            return;
        }

        if (!seesCurrent)
            return;

        bool stepped = Chebyshev(previous, current) == 1;
        if (stepped && seesPrevious)
        {
            if (Chebyshev(myCharacter.Position, current) <= CIRCLE_CHASE_DISTANCE)
                RecordTrailTile(previous);
            SetLastKnown(previous);
        }
        else
            SetLastKnown(current);
    }

    private void RecordTrailTile(Position2D tile)
    {
        if (tile == myCharacter.Position)
            return;
        if (chaseTrail.Count > 0 && chaseTrail[chaseTrail.Count - 1] == tile)
            return;
        chaseTrail.Add(tile);
    }

    private void PruneTrail()
    {
        while (chaseTrail.Count > 0 && chaseTrail[0] == myCharacter.Position)
            chaseTrail.RemoveAt(0);
    }

    private bool TryGetChaseDestination(out Position2D destination)
    {
        if (IsPlayerVisible())
        {
            int distance = Chebyshev(myCharacter.Position, chasedCharacter.Position);
            if (distance > CIRCLE_CHASE_DISTANCE)
            {
                chaseTrail.Clear();
                destination = chasedCharacter.Position;
                return true;
            }

            Position2D footstep = chasedCharacter.LastPosition;
            if (footstep != myCharacter.Position && Chebyshev(footstep, chasedCharacter.Position) == 1)
            {
                destination = footstep;
                return true;
            }

            destination = chasedCharacter.Position;
            return true;
        }

        if (hasLastKnownPlayerPosition && lastKnownPlayerPosition != myCharacter.Position)
        {
            chaseTrail.Clear();
            destination = lastKnownPlayerPosition;
            return true;
        }

        destination = default;
        return false;
    }

    private IEnumerator FollowChasePath()
    {
        RememberPlayerLocation();

        if (CanAttack())
        {
            Debug.Log("in attack range, attacking");
            yield return GameController.Instance.fightingSystem.UseBasicAttack(myCharacter, chasedCharacter as IDamagable);
            yield break;
        }

        if (!TryGetChaseDestination(out Position2D destination))
        {
            if (!IsPlayerVisible())
            {
                Debug.Log("lost player, wandering");
                myState = EnemyState.WANDERING;
                chaseTrail.Clear();
                hasLastKnownPlayerPosition = false;
                FindWanderPoint();
                yield return WalkNextChaseStep();
            }
            else
            {
                yield return WaitATurn();
            }
            yield break;
        }

        Debug.Log("chasing towards " + destination.x + "," + destination.y);
        myCharacter.CurrentTarget = destination;
        GameController.Instance.movementSystem.RecalculatePath(myCharacter, destination);
        yield return WalkNextChaseStep();
        PruneTrail();
    }

    private IEnumerator WalkNextChaseStep()
    {
        if (myCharacter.CurrentPath == null || myCharacter.CurrentPath.Count == 0)
        {
            yield return WaitATurn();
            yield break;
        }

        Position2D next = myCharacter.CurrentPath[0];
        myCharacter.CurrentPath.RemoveAt(0);

        TileInfo tile = GameController.Instance.dungeon.GetTileInfos()[next.x, next.y];
        if (tile != null && tile.isOccupied)
        {
            yield return WaitATurn();
            yield break;
        }

        yield return GameController.Instance.movementSystem.Walk(myCharacter, next);
    }

    private Position2D GetMyNextPathStep()
    {
        while (myCharacter.CurrentPath.Count == 0)
            FindWanderPoint();

        Position2D p = myCharacter.CurrentPath[0];
        myCharacter.CurrentPath.RemoveAt(0);

        return p;
    }

    private bool CanAttack()
    {
        if (!IsPlayerVisible())
            return false;
        if (chasedCharacter is not IDamagable target)
            return false;

        return GameController.Instance.fightingSystem.CanUse(myCharacter.BasicAttack, myCharacter, target);
    }

    private float CalculateWakeUpChance()
    {
        Position2D playerPosition = GameController.Instance.Player.Position;
        int dx = Math.Abs(myCharacter.Position.x - playerPosition.x);
        int dy = Math.Abs(myCharacter.Position.y - playerPosition.y);
        int distance = Math.Max(dx, dy);

        float wakeUpChance = BASE_WAKE_UP_CHANCE;

        if (distance > myCharacter.WakeUpRange)
            return wakeUpChance;
        else
        {
            if (distance <= GUARANTEED_WAKE_UP_DISTANCE)
                return 1f;
            else
                return 1f * (distance / myCharacter.WakeUpRange);
        }
    }

    private void FindWanderPoint()
    {
        myCharacter.CurrentTarget = GameController.Instance.dungeon.GetRandomBaseField();
        GameController.Instance.movementSystem.RecalculatePath(myCharacter, myCharacter.CurrentTarget);
    }

    public void WakeUp()
    {
        if (CanDetect(chasedCharacter))
            BeginChase();
        else
        {
            myState = EnemyState.WANDERING;
            FindWanderPoint();
        }
    }

    public HashSet<Position2D> SeenFields()
    {
        HashSet<Position2D> result = new();
        result = GameController.Instance.visionSystem.AllVisibleFields(myCharacter.Position, myCharacter.ViewRange, GameController.Instance.dungeon);
        return result;
    }

    public bool CanSee(IActor target)
    {
        //Debug.Log("can see enemy");
        if (seenFields.Contains(target.LastPosition))
            return true;
        return false;
    }

    private float CalculateDetectionChance()
    {
        Position2D playerPosition = GameController.Instance.Player.Position;
        int dx = Math.Abs(myCharacter.Position.x - playerPosition.x);
        int dy = Math.Abs(myCharacter.Position.y - playerPosition.y);
        int distance = Math.Max(dx, dy);

        float detectionChance = BASE_DETECTION_CHANCE;

        if (distance > myCharacter.DetectionRange)
            return detectionChance;

        int difrence = Math.Abs(distance - myCharacter.DetectionRange);

        return BASE_DETECTION_CHANCE + 0.1f * distance;
    }

    public bool CanDetect(IActor target)
    {
        //Debug.Log("can detect enemy");
        if (seenFields.Contains(target.Position))
        {
            return true;
        }
            
        return false;
    }

    public bool CanWakeUp(IActor target)
    {
        if (seenFields.Contains(target.Position))
            return true;
        return false;
    }

    private void OnDrawGizmos()
    {
        DrawLastKnownPlayerGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        if (myCharacter == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.WakeUpRange * 2, 1 / Consts.TILE_SIZE, myCharacter.WakeUpRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.DetectionRange * 2, 2 / Consts.TILE_SIZE, myCharacter.DetectionRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.AttackRange * 2, 3 / Consts.TILE_SIZE, myCharacter.AttackRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.ViewRange * 2, 4 / Consts.TILE_SIZE, myCharacter.ViewRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.cyan;
        ReplaceMarker();
    }
}
