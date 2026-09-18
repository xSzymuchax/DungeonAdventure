using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActor : IHasPosition
{
    public double Energy { get; set; }
    public new Position2D Position { get; set; }
    public Position2D LastPosition { get; set; }

    public bool HasEnergy => Energy > 0;

    public double GetEnergy();
    public void AddEnergy(double amount);

    public void RemoveEnergy(double amount);
}
