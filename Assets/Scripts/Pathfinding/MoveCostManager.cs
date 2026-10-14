using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCostManager 
{
    private Dictionary<FloorFieldType, int> moveCosts;
    public MoveCostManager() { moveCosts = new(); }

    public void AddCost(FloorFieldType type, int cost)
    {
        moveCosts.Add(type, cost);
    }

    public int GetCost(FloorFieldType type)
    {
        if (moveCosts.ContainsKey(type))
            return moveCosts[type];
        return int.MaxValue;
    }

    public bool CanStep(FloorFieldType type)
    {
        return GetCost(type) < int.MaxValue;
    }

    public static MoveCostManager CreateDefault()
    {
        MoveCostManager manager = new MoveCostManager();
        manager.AddCost(FloorFieldType.BASE_FIELD, 1);
        manager.AddCost(FloorFieldType.CORRIDOR_FIELD, 1);
        manager.AddCost(FloorFieldType.POSSIBLE_DOOR_FIELD, 1);
        manager.AddCost(FloorFieldType.SPAWN_FIELD, 1);
        manager.AddCost(FloorFieldType.EXIT_FIELD, 1);
        return manager;
    }
}
