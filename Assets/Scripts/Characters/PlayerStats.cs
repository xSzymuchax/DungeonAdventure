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
        Apply(null);
    }

    public void Apply(StatTotals bonuses)
    {
        base.Recalculate();
        MaxMana = baseMaxMana + Bonus(bonuses, StatId.MaxMana);
        Strength = baseStrength + Mathf.RoundToInt(Bonus(bonuses, StatId.Strength));
        Knowledge = baseKnowledge + Mathf.RoundToInt(Bonus(bonuses, StatId.Knowledge));
        MaxSatiety = Mathf.Max(0f, baseMaxSatiety);
        MaxHydration = Mathf.Max(0f, baseMaxHydration);
        MaxSanity = Mathf.Max(0f, baseMaxSanity);
        Damage += Mathf.RoundToInt(Bonus(bonuses, StatId.Damage));
        MaxHealth += Bonus(bonuses, StatId.MaxHealth);
        WalkCost = Mathf.Max(1f, WalkCost + Bonus(bonuses, StatId.WalkCost));
        AttackCost = Mathf.Max(1f, AttackCost + Bonus(bonuses, StatId.AttackCost));
        ViewRange = Mathf.Max(1, ViewRange + Mathf.RoundToInt(Bonus(bonuses, StatId.ViewRange)));
        Defense = Mathf.RoundToInt(Bonus(bonuses, StatId.Defense));
        Block = Percent(Bonus(bonuses, StatId.Block));
        Dodge = Percent(Dodge + Bonus(bonuses, StatId.Dodge));
        CounterDodge = Percent(CounterDodge + Bonus(bonuses, StatId.CounterDodge));
        CriticalChance = Percent(CriticalChance + Bonus(bonuses, StatId.CriticalChance));
        MagicAmplify = Mathf.Max(0f, MagicAmplify + Bonus(bonuses, StatId.MagicAmplify));
        MagicResistance = Mathf.Max(0f, MagicResistance + Bonus(bonuses, StatId.MagicResistance));
        FireResistance = Attitude(FireResistance + Bonus(bonuses, StatId.FireResistance));
        ColdResistance = Attitude(ColdResistance + Bonus(bonuses, StatId.ColdResistance));
        PoisonResistance = Attitude(PoisonResistance + Bonus(bonuses, StatId.PoisonResistance));
        ElectricityResistance = Attitude(ElectricityResistance + Bonus(bonuses, StatId.ElectricityResistance));
        BleedingResistance = Attitude(BleedingResistance + Bonus(bonuses, StatId.BleedingResistance));
        HungerResistance = Attitude(HungerResistance + Bonus(bonuses, StatId.HungerResistance));
        HealthRegen = baseHealthRegen + Bonus(bonuses, StatId.HealthRegen);
        ManaRegen = baseManaRegen + Bonus(bonuses, StatId.ManaRegen);
        SatietyBurn = baseSatietyBurn + Bonus(bonuses, StatId.SatietyBurn);
        HydrationBurn = baseHydrationBurn + Bonus(bonuses, StatId.HydrationBurn);
        SanityBurn = baseSanityBurn + Bonus(bonuses, StatId.SanityBurn);
    }

    static float Bonus(StatTotals bonuses, StatId stat)
    {
        return bonuses != null ? bonuses.Get(stat) : 0f;
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
