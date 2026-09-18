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

public class CharacterStats : MonoBehaviour
{
    [Header("Vitals")]
    [SerializeField] int baseMaxHealth = 20;
    [SerializeField] double baseMaxMana = 20;

    [Header("Speed")]
    [SerializeField] double baseWalkingCost = 10;
    [SerializeField] double baseAttackCost = 10;

    [Header("Combat")]
    [SerializeField] int baseDamage = 5;
    [SerializeField] int baseAttackRange = 1;

    [Header("Perception")]
    [SerializeField] int baseViewRange = 5;
    [SerializeField] int baseWakeUpRange = 3;
    [SerializeField] int baseDetectionRange = 5;

    [Header("Terrain")]
    [SerializeField] TileMoveCost[] baseTileMoveCosts;

    public int MaxHealth { get; private set; } = 20;
    public double MaxMana { get; private set; } = 20;
    public double WalkCost { get; private set; } = 10;
    public double AttackCost { get; private set; } = 10;
    public int Damage { get; private set; } = 5;
    public int AttackRange { get; private set; }
    public int ViewRange { get; private set; }
    public int WakeUpRange { get; private set; }
    public int DetectionRange { get; private set; }
    public MoveCostManager MoveCostManager { get; private set; }

    private void OnEnable()
    {
        Recalculate();
    }

    private void OnValidate()
    {
        Recalculate();
    }

    public void Recalculate()
    {
        MaxHealth = baseMaxHealth;
        MaxMana = baseMaxMana;
        WalkCost = baseWalkingCost;
        AttackCost = baseAttackCost;
        Damage = baseDamage;
        AttackRange = baseAttackRange;
        ViewRange = baseViewRange;
        WakeUpRange = baseWakeUpRange;
        DetectionRange = baseDetectionRange;
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
