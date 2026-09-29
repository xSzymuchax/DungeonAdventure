using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    [Header("Vitals")]
    [SerializeField] float baseMaxMana = 20;

    [Header("Attributes")]
    [SerializeField] int baseStrength = 10;
    [SerializeField] int baseKnowledge = 10;

    [Header("Wellbeing")]
    [SerializeField] float baseMaxSatiety = 100;
    [SerializeField] float baseMaxHydration = 100;
    [SerializeField] float baseMaxSanity = 100;

    [Header("Turn")]
    [SerializeField] float baseHealthRegen = 1;
    [SerializeField] float baseManaRegen = 1;
    [SerializeField] float baseSatietyBurn = 1;
    [SerializeField] float baseHydrationBurn = 1;
    [SerializeField] float baseSanityBurn = 1;

    public int Strength { get; private set; }
    public int Knowledge { get; private set; }
    public float MaxSatiety { get; private set; }
    public float MaxHydration { get; private set; }
    public float MaxSanity { get; private set; }
    public float HealthRegen { get; private set; }
    public float ManaRegen { get; private set; }
    public float SatietyBurn { get; private set; }
    public float HydrationBurn { get; private set; }
    public float SanityBurn { get; private set; }

    public override void Recalculate()
    {
        Recalculate(null);
    }

    public void Recalculate(IEnumerable<StatModifier> modifiers)
    {
        base.Recalculate();
        MaxMana = baseMaxMana + StatBonus.Sum(modifiers, StatId.MaxMana);
        Strength = baseStrength + Mathf.RoundToInt(StatBonus.Sum(modifiers, StatId.Strength));
        Knowledge = baseKnowledge + Mathf.RoundToInt(StatBonus.Sum(modifiers, StatId.Knowledge));
        MaxSatiety = Mathf.Max(0f, baseMaxSatiety);
        MaxHydration = Mathf.Max(0f, baseMaxHydration);
        MaxSanity = Mathf.Max(0f, baseMaxSanity);
        Damage += Mathf.RoundToInt(StatBonus.Sum(modifiers, StatId.Damage));
        MaxHealth += StatBonus.Sum(modifiers, StatId.MaxHealth);
        WalkCost = Mathf.Max(1f, WalkCost + StatBonus.Sum(modifiers, StatId.WalkCost));
        AttackCost = Mathf.Max(1f, AttackCost + StatBonus.Sum(modifiers, StatId.AttackCost));
        ViewRange = Mathf.Max(1, ViewRange + Mathf.RoundToInt(StatBonus.Sum(modifiers, StatId.ViewRange)));
        Defense = Mathf.RoundToInt(StatBonus.Sum(modifiers, StatId.Defense));
        Block = Mathf.RoundToInt(StatBonus.Sum(modifiers, StatId.Block));
        HealthRegen = baseHealthRegen + StatBonus.Sum(modifiers, StatId.HealthRegen);
        ManaRegen = baseManaRegen + StatBonus.Sum(modifiers, StatId.ManaRegen);
        SatietyBurn = baseSatietyBurn + StatBonus.Sum(modifiers, StatId.SatietyBurn);
        HydrationBurn = baseHydrationBurn + StatBonus.Sum(modifiers, StatId.HydrationBurn);
        SanityBurn = baseSanityBurn + StatBonus.Sum(modifiers, StatId.SanityBurn);
    }

    public override void ApplySaved(float maxHealth, float maxMana, float walkCost, float attackCost, int damage, int viewRange)
    {
        base.ApplySaved(maxHealth, maxMana, walkCost, attackCost, damage, viewRange);
        WalkCost = Mathf.Max(1f, WalkCost);
        AttackCost = Mathf.Max(1f, AttackCost);
    }

    public void ApplySavedAttributes(int strength, int knowledge, float maxSatiety, float maxHydration, float maxSanity)
    {
        Strength = strength;
        Knowledge = knowledge;
        MaxSatiety = maxSatiety;
        MaxHydration = maxHydration;
        MaxSanity = maxSanity;
    }
}
