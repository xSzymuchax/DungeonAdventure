using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileMoveCost
{
    public FloorFieldType type;
    public int cost = 1;
}

public abstract class CharacterStats : MonoBehaviour
{
    [Header("Vitals")]
    [SerializeField] int baseMaxHealth = 20;

    [Header("Speed")]
    [SerializeField] double baseWalkingCost = 10;
    [SerializeField] double baseAttackCost = 10;

    [Header("Combat")]
    [SerializeField] int baseDamage = 5;

    [Header("Perception")]
    [SerializeField] int baseViewRange = 5;

    [Header("Terrain")]
    [SerializeField] TileMoveCost[] baseTileMoveCosts;

    public int MaxHealth { get; protected set; } = 20;
    public double MaxMana { get; protected set; }
    public double WalkCost { get; protected set; } = 10;
    public double AttackCost { get; protected set; } = 10;
    public int Damage { get; protected set; } = 5;
    public int ViewRange { get; protected set; }
    public MoveCostManager MoveCostManager { get; protected set; }

    private void OnEnable()
    {
        Recalculate();
    }

    private void OnValidate()
    {
        Recalculate();
    }

    public virtual void Recalculate()
    {
        MaxHealth = baseMaxHealth;
        WalkCost = baseWalkingCost;
        AttackCost = baseAttackCost;
        Damage = baseDamage;
        ViewRange = baseViewRange;
        MoveCostManager = BuildMoveCosts();
    }

    private MoveCostManager BuildMoveCosts()
    {
        MoveCostManager manager = new();
        TileMoveCost[] costs = baseTileMoveCosts;
        if (costs == null || costs.Length == 0)
            costs = DefaultTileMoveCosts();

        foreach (TileMoveCost tileCost in costs)
            manager.AddCost(tileCost.type, tileCost.cost);

        return manager;
    }

    private static TileMoveCost[] DefaultTileMoveCosts()
    {
        return new TileMoveCost[]
        {
            new() { type = FloorFieldType.BASE_FIELD, cost = 1 },
            new() { type = FloorFieldType.CORRIDOR_FIELD, cost = 1 },
            new() { type = FloorFieldType.POSSIBLE_DOOR_FIELD, cost = 1 },
            new() { type = FloorFieldType.SPAWN_FIELD, cost = 1 },
            new() { type = FloorFieldType.EXIT_FIELD, cost = 1 },
        };
    }
}
