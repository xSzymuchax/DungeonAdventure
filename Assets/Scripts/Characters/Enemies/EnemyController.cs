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

    public IActor Actor => myCharacter;
    private HashSet<Position2D> seenFields = new();

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

    private IEnumerator MoveTowardsChasedCharacter(Position2D target)
    {
        yield return GameController.Instance.movementSystem.Walk(myCharacter, target);
    }

    private IEnumerator WaitATurn()
    {
        yield return GameController.Instance.RequestWaitTurn(myCharacter);
    }

    private void ReplaceMarker()
    {
        if (myCharacter.CurrentPath.Count != 0)
            Gizmos.DrawSphere(new Vector3(
                myCharacter.CurrentPath[myCharacter.CurrentPath.Count - 1].x * Consts.TILE_SIZE + 5,
                1,
                myCharacter.CurrentPath[myCharacter.CurrentPath.Count - 1].y * Consts.TILE_SIZE + 5),
                5);

    }

    public IEnumerator MakeMove() // powinien zwracac parda/falsz jesli sie udalo wykonac akcje
    {
        Debug.Log("Enemy making move");
        // Debug.Log(myState);

        seenFields = SeenFields();

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
            if (CanDetect(chasedCharacter) && UnityEngine.Random.value <= CalculateDetectionChance()) // if detects player, chase him
            {
                Debug.Log("wandering - detected player, chasing");
                myCharacter.CurrentTarget = chasedCharacter.Position;
                GameController.Instance.movementSystem.RecalculatePath(myCharacter, chasedCharacter.Position);
                myState = EnemyState.CHASING;
            }

            Debug.Log("wandering - making step");
            yield return GameController.Instance.movementSystem.Walk(myCharacter, GetMyNextPathStep());
            currentNotDoAction = 0;
            yield break;
        }
        else if (myState == EnemyState.CHASING)
        {
            if (CanAttack())
            {
                Debug.Log("attacking player");
                // attack
                yield return WaitATurn();
                myCharacter.CurrentTarget = chasedCharacter.Position;
                GameController.Instance.movementSystem.RecalculatePath(myCharacter, chasedCharacter.Position);;
                currentNotDoAction = 0;
            }
            else
            {
                if (CanSee(chasedCharacter))
                {
                    Debug.Log("chasing, seeing, recalculating on empty path");
                    // if sees, recalculate path

                    //if (myCharacter.CurrentPath.Count == 0)
                    //{
                    yield return GameController.Instance.movementSystem.Walk(myCharacter, GetMyNextPathStep());

                    myCharacter.CurrentTarget = chasedCharacter.LastPosition;
                    GameController.Instance.movementSystem.RecalculatePath(myCharacter, chasedCharacter.LastPosition);

                    //}
                    currentNotDoAction = 0;
                }
                else
                { // else go to the lastest known position
                    Debug.Log("cant see, going to last know player position");
                    if (myCharacter.CurrentPath.Count == 0) // change to wandering if still desnt see player
                    {
                        myState = EnemyState.WANDERING;
                    }
                    //GameController.Instance.movementSystem.RecalculatePath(myCharacter, chasedCharacter.Position);
                    yield return GameController.Instance.movementSystem.Walk(myCharacter, GetMyNextPathStep());
                    currentNotDoAction = 0;
                }
                    
            }
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
        int dx = Math.Abs(myCharacter.Position.x - chasedCharacter.Position.x);
        int dy = Math.Abs(myCharacter.Position.y - chasedCharacter.Position.y);
        int distance = Math.Max(dx, dy);

        if (distance == 1)
            return true;
        else
            return false;
    }

    private float CalculateWakeUpChance()
    {
        Position2D playerPosition = GameController.Instance.GetPlayerReference().Position;
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
        {
            myState = EnemyState.CHASING;
            myCharacter.CurrentTarget = chasedCharacter.Position;
            GameController.Instance.movementSystem.RecalculatePath(myCharacter, chasedCharacter.Position);
        }
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
        Position2D playerPosition = GameController.Instance.GetPlayerReference().Position;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.WakeUpRange * 2, 1 / Consts.TILE_SIZE, myCharacter.WakeUpRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.DetectionRange * 2, 2 / Consts.TILE_SIZE, myCharacter.DetectionRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.AttackRange * 2, 3 / Consts.TILE_SIZE, myCharacter.AttackRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.ViewRange * 2, 4 / Consts.TILE_SIZE, myCharacter.ViewRange * 2) * Consts.TILE_SIZE);

        ReplaceMarker();
    }
}
