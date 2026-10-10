using System;

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
    HungerResistance,
    CriticalChance
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
