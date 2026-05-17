using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    protected override void InitMoveCosts()
    {
        moveCostManager = new();
        moveCostManager.AddCost(FloorFieldType.BASE_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.CORRIDOR_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.POSSIBLE_DOOR_FIELD, 1);
    }
}
