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

    private IEnumerator MoveTowardsChasedCharacter()
    {
        Position2D targetPosition = GameController.Instance.GetPositionOfActor(chasedCharacter);
        yield return GameController.Instance.RequestMoveTo(myCharacter, targetPosition);
    }

    public IEnumerator MakeMove()
    {
        Debug.Log("Enemy making move");
        yield return StartCoroutine(MoveTowardsChasedCharacter());
    }
}
