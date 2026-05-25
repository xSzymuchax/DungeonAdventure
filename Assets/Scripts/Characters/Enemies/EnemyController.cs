using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    SLEEPING, WANDERING, CHASING
}

public class EnemyController : MonoBehaviour, IEnemyController, IPerceptionUser
{
    private EnemyState myState = EnemyState.SLEEPING;

    private EnemyCharacter myCharacter;
    private IActor chasedCharacter;

    public IActor Actor => myCharacter;

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
        yield return GameController.Instance.RequestMoveTo(myCharacter, target);
    }

    private IEnumerator WaitATurn()
    {
        yield return GameController.Instance.RequestWaitTurn(myCharacter);
    }

    public IEnumerator MakeMove() // powinien zwracac parda/falsz jesli sie udalo wykonac akcje
    {
        Debug.Log("Enemy making move");

        Position2D targetPosition = GameController.Instance.GetPositionOfActor(chasedCharacter);
        /* TODO
         * sprawdziæ czy widzi
         * sprawdzic jak daleko
         * porownac czy da sie atakowac
         * czy da sie isc
         */

        if (CanSee(chasedCharacter)) Debug.Log("CAN_SEE");
        if (CanAttack(chasedCharacter)) Debug.Log("CAN_ATTACK");
        if (CanDetect(chasedCharacter)) Debug.Log("CAN_DETECT");
        if (CanWakeUp(chasedCharacter)) Debug.Log("CAN_WAKE_UP");

        //if (CanAttack(chasedCharacter))
        //{
        //    yield return StartCoroutine(WaitATurn());
        //}
        //else
        //{
        //    yield return StartCoroutine(MoveTowardsChasedCharacter(targetPosition));
        //}   

        yield return StartCoroutine(WaitATurn());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.WakeUpRange*2, 1 / Consts.TILE_SIZE, myCharacter.WakeUpRange*2) * Consts.TILE_SIZE);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.DetectionRange*2, 2 / Consts.TILE_SIZE, myCharacter.DetectionRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.AttackRange*2, 3 / Consts.TILE_SIZE, myCharacter.AttackRange * 2) * Consts.TILE_SIZE);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(myCharacter.ViewRange*2, 4 / Consts.TILE_SIZE, myCharacter.ViewRange * 2) * Consts.TILE_SIZE);
    }

    public bool CanAttack(IActor target)
    {
        return GameController.Instance.CanAttackActor(myCharacter, target);
    }

    public bool CanSee(IActor target)
    {
        return GameController.Instance.CanSeeActor(myCharacter, target);
    }

    public bool CanDetect(IActor target)
    {
        return GameController.Instance.CanDetectActor(myCharacter, target);
    }

    public bool CanWakeUp(IActor target)
    {
        return GameController.Instance.CanWakeUpActor(myCharacter, target);
    }
}
