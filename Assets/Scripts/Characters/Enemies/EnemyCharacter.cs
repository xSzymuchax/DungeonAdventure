using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character, IHasPerception
{
    public int _viewRange;
    public int _wakeUpRange;
    public int _attackRange;
    public int _detectionRange;

    public int ViewRange => _viewRange;

    public int WakeUpRange => _wakeUpRange;

    public int AttackRange => _attackRange;

    public int DetectionRange => _detectionRange;

    protected override void InitMoveCosts()
    {
        moveCostManager = new();
        moveCostManager.AddCost(FloorFieldType.BASE_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.CORRIDOR_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.POSSIBLE_DOOR_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.SPAWN_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.EXIT_FIELD, 1);
    }
}
