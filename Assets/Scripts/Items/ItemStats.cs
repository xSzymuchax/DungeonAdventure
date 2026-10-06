using System;
using System.Collections.Generic;

public enum StatId
{
    Damage,
    Defense,
    Block,
    WalkCost,
    AttackCost,
    Strength,
    Knowledge,
    MaxHealth,
    MaxMana,
    ViewRange,
    MaxSatiety,
    MaxHydration,
    MaxSanity,
    HealthRegen,
    ManaRegen,
    SatietyBurn,
    HydrationBurn,
    SanityBurn,
    ArrowDamage,
    BoltDamage,
    DartDamage,
    MagicResistance,
    FireResistance,
    ColdResistance,
    PoisonResistance,
    ElectricityResistance,
    Dodge,
    CounterDodge,
    MagicAmplify,
    BleedingResistance,
    HungerResistance
}

[Serializable]
public class StatModifier
{
    public StatId stat;
    public float value;
}

[Serializable]
public class StatRequirement
{
    public StatId stat;
    public int value;
}

public static class StatBonus
{
    public static float Sum(IEnumerable<StatModifier> modifiers, StatId stat)
    {
        if (modifiers == null)
            return 0f;

        float sum = 0f;
        foreach (StatModifier modifier in modifiers)
        {
            if (modifier != null && modifier.stat == stat)
                sum += modifier.value;
        }

        return sum;
    }
}
