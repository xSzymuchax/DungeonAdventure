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
}
