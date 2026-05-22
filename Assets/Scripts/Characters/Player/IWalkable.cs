using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWalkable : IActor
{
    public double WalkCost { get; }
    public MoveCostManager MoveCostManager { get; }

    public IEnumerator WalkingAnimation(Vector3 target);
}