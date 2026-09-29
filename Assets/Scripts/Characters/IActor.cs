using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActor : IHasPosition
{
    public float Energy { get; set; }
    public new Position2D Position { get; set; }
    public Position2D LastPosition { get; set; }

    public bool HasEnergy => Energy > 0;

    public float GetEnergy();
    public void AddEnergy(float amount);

    public void RemoveEnergy(float amount);
}
