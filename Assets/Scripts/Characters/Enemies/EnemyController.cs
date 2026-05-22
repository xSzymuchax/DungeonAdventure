using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    SLEEPING, WANDERING, CHASING
}

public class EnemyController : MonoBehaviour, IEnemyController
{
    private EnemyState myState = EnemyState.SLEEPING;
    private double attackRange = 1;
    private double wakeUpRange = 5;
    private double chasingRange = 10;

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

    public IEnumerator MakeMove()
    {
        Debug.Log("Enemy making move");

        Position2D targetPosition = GameController.Instance.GetPositionOfActor(chasedCharacter);
        /* TODO
         * sprawdziæ czy widzi
         * sprawdzic jak daleko
         * porownac czy da sie atakowac
         * czy da sie isc
         */

        if (Random.value <= 0.5)
        {
            yield return StartCoroutine(WaitATurn());
        }
        else
        {
            yield return StartCoroutine(MoveTowardsChasedCharacter(targetPosition));
        }   
    }
}
