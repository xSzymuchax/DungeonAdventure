using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IActor
{
    public double Energy { get; set; }
    protected MoveCostManager moveCostManager;

    private void Start()
    {
        InitMoveCosts();
    }

    protected virtual void InitMoveCosts() { }

    public double GetEnergy()
    {
        return Energy;
    }
    public void AddEnergy(double amount)
    {
        Energy += amount;
    }

    public void RemoveEnergy(double amount)
    {
        Energy -= amount;
    }

    public MoveCostManager GetMoveCosts()
    {
        return moveCostManager;
    }
}
