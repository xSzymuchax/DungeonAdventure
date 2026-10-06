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
    [SerializeField] float baseMaxHealth = 20;

    [Header("Speed")]
    [SerializeField] float baseWalkingCost = 10;
    [SerializeField] float baseAttackCost = 10;

    [Header("Combat")]
    [SerializeField] int baseDamage = 5;
    [SerializeField] int baseDodge;
    [SerializeField] int baseCounterDodge;
    [SerializeField] float baseMagicAmplify;

    [Header("Resistance")]
    [SerializeField] float baseMagicResistance;
    [SerializeField] int baseFireResistance;
    [SerializeField] int baseColdResistance;
    [SerializeField] int basePoisonResistance;
    [SerializeField] int baseElectricityResistance;
    [SerializeField] int baseBleedingResistance;
    [SerializeField] int baseHungerResistance;

    [Header("Perception")]
    [SerializeField] int baseViewRange = 5;

    [Header("Terrain")]
    [SerializeField] TileMoveCost[] baseTileMoveCosts;

    public float MaxHealth { get; protected set; } = 20;
    public float MaxMana { get; protected set; } = 10;
    public float WalkCost { get; protected set; } = 10;
    public float AttackCost { get; protected set; } = 10;
    public int Damage { get; protected set; } = 5;
    public int Defense { get; protected set; } = 0;
    public int Block { get; protected set; } = 0;
    public int Dodge { get; protected set; }
    public int CounterDodge { get; protected set; }
    public float MagicAmplify { get; protected set; }
    public float MagicResistance { get; protected set; }
    public int FireResistance { get; protected set; }
    public int ColdResistance { get; protected set; }
    public int PoisonResistance { get; protected set; }
    public int ElectricityResistance { get; protected set; }
    public int BleedingResistance { get; protected set; }
    public int HungerResistance { get; protected set; }
    public int ViewRange { get; protected set; }
    public MoveCostManager MoveCostManager { get; protected set; }

    public virtual void ApplySaved(float maxHealth, float maxMana, float walkCost, float attackCost, int damage, int viewRange)
    {
        MaxHealth = maxHealth;
        MaxMana = maxMana;
        WalkCost = walkCost;
        AttackCost = attackCost;
        Damage = damage;
        ViewRange = viewRange;
    }

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
        Defense = 0;
        Block = 0;
        Dodge = Percent(baseDodge);
        CounterDodge = Percent(baseCounterDodge);
        MagicAmplify = Mathf.Max(0f, baseMagicAmplify);
        MagicResistance = Mathf.Max(0f, baseMagicResistance);
        FireResistance = Attitude(baseFireResistance);
        ColdResistance = Attitude(baseColdResistance);
        PoisonResistance = Attitude(basePoisonResistance);
        ElectricityResistance = Attitude(baseElectricityResistance);
        BleedingResistance = Attitude(baseBleedingResistance);
        HungerResistance = Attitude(baseHungerResistance);
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

    protected static int Percent(float sum)
    {
        return Mathf.Clamp(Mathf.RoundToInt(sum), 0, 100);
    }

    protected static int Attitude(float sum)
    {
        if (sum >= 1f)
            return 1;
        if (sum <= -1f)
            return -1;
        return 0;
    }

    public int Resistance(DamageType type)
    {
        switch (type)
        {
            case DamageType.Fire: return FireResistance;
            case DamageType.Cold: return ColdResistance;
            case DamageType.Poison: return PoisonResistance;
            case DamageType.Electricity: return ElectricityResistance;
            case DamageType.Bleeding: return BleedingResistance;
            case DamageType.Hunger: return HungerResistance;
            default: return 0;
        }
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
