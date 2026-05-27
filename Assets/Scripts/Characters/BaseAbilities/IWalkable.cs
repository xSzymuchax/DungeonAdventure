using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWalkable : IActor
{
    public double WalkCost { get; }
    public Position2D CurrentTarget { get; set; }
    public List<Position2D> CurrentPath { get; set; }
    public MoveCostManager MoveCostManager { get; }
}