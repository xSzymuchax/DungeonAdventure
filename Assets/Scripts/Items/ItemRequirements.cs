public static class ItemRequirements
{
    public static bool Met(EquipmentItem item, PlayerStats stats)
    {
        if (item == null)
            return false;
        if (item.Requirements.Count == 0)
            return true;
        if (stats == null)
            return false;

        for (int i = 0; i < item.Requirements.Count; i++)
        {
            StatRequirement requirement = item.Requirements[i];
            if (requirement == null)
                continue;
            if (Value(stats, requirement.stat) < requirement.value)
                return false;
        }

        return true;
    }

    public static float Value(PlayerStats stats, StatId stat)
    {
        if (stats == null)
            return 0;

        switch (stat)
        {
            case StatId.Damage: return stats.Damage;
            case StatId.Defense: return stats.Defense;
            case StatId.Block: return stats.Block;
            case StatId.WalkCost: return stats.WalkCost;
            case StatId.AttackCost: return stats.AttackCost;
            case StatId.Strength: return stats.Strength;
            case StatId.Knowledge: return stats.Knowledge;
            case StatId.MaxHealth: return stats.MaxHealth;
            case StatId.MaxMana: return stats.MaxMana;
            case StatId.ViewRange: return stats.ViewRange;
            case StatId.MaxSatiety: return stats.MaxSatiety;
            case StatId.MaxHydration: return stats.MaxHydration;
            case StatId.MaxSanity: return stats.MaxSanity;
            case StatId.HealthRegen: return stats.HealthRegen;
            case StatId.ManaRegen: return stats.ManaRegen;
            case StatId.SatietyBurn: return stats.SatietyBurn;
            case StatId.HydrationBurn: return stats.HydrationBurn;
            case StatId.SanityBurn: return stats.SanityBurn;
            case StatId.MagicResistance: return stats.MagicResistance;
            case StatId.FireResistance: return stats.FireResistance;
            case StatId.ColdResistance: return stats.ColdResistance;
            case StatId.PoisonResistance: return stats.PoisonResistance;
            case StatId.ElectricityResistance: return stats.ElectricityResistance;
            case StatId.BleedingResistance: return stats.BleedingResistance;
            case StatId.HungerResistance: return stats.HungerResistance;
            case StatId.CriticalChance: return stats.CriticalChance;
            case StatId.Dodge: return stats.Dodge;
            case StatId.CounterDodge: return stats.CounterDodge;
            case StatId.MagicAmplify: return stats.MagicAmplify;
            default: return 0;
        }
    }
}
