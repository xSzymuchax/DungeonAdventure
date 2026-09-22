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
        base.Recalculate();
        MaxMana = baseMaxMana;
        Strength = baseStrength;
        Knowledge = baseKnowledge;
        MaxSatiety = baseMaxSatiety;
        MaxHydration = baseMaxHydration;
        MaxSanity = baseMaxSanity;
    }
}
