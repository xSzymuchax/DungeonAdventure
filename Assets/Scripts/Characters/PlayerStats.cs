using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats
{
    [Header("Vitals")]
    [SerializeField] double baseMaxMana = 20;

    [Header("Attributes")]
    [SerializeField] int baseStrength = 10;
    [SerializeField] int baseKnowledge = 10;

    [Header("Wellbeing")]
    [SerializeField] int baseMaxSatiety = 100;
    [SerializeField] int baseMaxHydration = 100;
    [SerializeField] int baseMaxSanity = 100;

    public int Strength { get; private set; }
    public int Knowledge { get; private set; }
    public int MaxSatiety { get; private set; }
    public int MaxHydration { get; private set; }
    public int MaxSanity { get; private set; }

    public override void Recalculate()
    {
        Recalculate(null);
    }

    public void Recalculate(IEnumerable<StatModifier> modifiers)
    {
        base.Recalculate();
        MaxMana = baseMaxMana + StatBonus.Sum(modifiers, StatId.MaxMana);
        Strength = baseStrength + StatBonus.Sum(modifiers, StatId.Strength);
        Knowledge = baseKnowledge + StatBonus.Sum(modifiers, StatId.Knowledge);
        MaxSatiety = System.Math.Max(0, baseMaxSatiety + StatBonus.Sum(modifiers, StatId.MaxSatiety));
        MaxHydration = System.Math.Max(0, baseMaxHydration + StatBonus.Sum(modifiers, StatId.MaxHydration));
        MaxSanity = System.Math.Max(0, baseMaxSanity + StatBonus.Sum(modifiers, StatId.MaxSanity));
        Damage += StatBonus.Sum(modifiers, StatId.Damage);
        MaxHealth += StatBonus.Sum(modifiers, StatId.MaxHealth);
        WalkCost = System.Math.Max(1, WalkCost + StatBonus.Sum(modifiers, StatId.WalkCost));
        AttackCost = System.Math.Max(1, AttackCost + StatBonus.Sum(modifiers, StatId.AttackCost));
        ViewRange = System.Math.Max(1, ViewRange + StatBonus.Sum(modifiers, StatId.ViewRange));
        Defense = StatBonus.Sum(modifiers, StatId.Defense);
        Block = StatBonus.Sum(modifiers, StatId.Block);
    }

    public void ApplySavedAttributes(int strength, int knowledge, int maxSatiety, int maxHydration, int maxSanity)
    {
        Strength = strength;
        Knowledge = knowledge;
        MaxSatiety = maxSatiety;
        MaxHydration = maxHydration;
        MaxSanity = maxSanity;
    }
}
