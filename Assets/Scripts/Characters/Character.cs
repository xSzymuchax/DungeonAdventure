using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IActor, IWalkable, IHasRepresentation
{
    private double _energy;
    private Position2D _currentPosition = new();
    private Position2D _currentTarget = new();
    private List<Position2D> _currentPath = new();
    private GameObject _myRepresentation;

    public double Energy { get => _energy; set { _energy = value; } }

    public double WalkCost => stats.CurrentWalkingCost;

    public MoveCostManager MoveCostManager => moveCostManager;

    public Position2D CurrentTarget { get => _currentTarget; set { _currentTarget = value; } }

    public List<Position2D> CurrentPath { get => _currentPath; set { _currentPath = value; } }

    public Position2D Position { get => _currentPosition; set { _currentPosition = value; } }

    public GameObject Representation => _myRepresentation;

    protected MoveCostManager moveCostManager;
    protected CharacterStats stats;

    private void Start()
    {
        InitMoveCosts();
        stats = GetComponent<CharacterStats>();
        _myRepresentation = gameObject;
        Energy = 10;
    }

    protected virtual void InitMoveCosts() { }

    public CharacterStats GetStats()
    {
        return stats;
    }

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

    public void SetTarget(Position2D target)
    {
        _currentTarget = target;
    }

    public void RecalculatePath(Position2D target)
    {
        GameController.Instance.RequestCalculatePath(this, target);
    }
}
