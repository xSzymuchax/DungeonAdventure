using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWalkable : IActor
{
    public double WalkCost { get; }
    public Position2D CurrentTarget { get; }
    public List<Position2D> CurrentPath { get; }
    public MoveCostManager MoveCostManager { get; }
    public IEnumerator WalkingAnimation(Vector3 target);
    public void SetTarget(Position2D target);
    public void RecalculatePath(Position2D target);

}