public static class StatLabel
{
    public static string Name(StatId stat, float value = 0f)
    {
        if (value < 0f)
        {
            switch (stat)
            {
                case StatId.FireResistance: return "Wrażliwość na ogień";
                case StatId.ColdResistance: return "Wrażliwość na zimno";
                case StatId.PoisonResistance: return "Wrażliwość na truciznę";
                case StatId.ElectricityResistance: return "Wrażliwość na elektryczność";
                case StatId.BleedingResistance: return "Wrażliwość na krwawienie";
                case StatId.HungerResistance: return "Wrażliwość na głód";
            }
        }

        switch (stat)
        {
            case StatId.Damage: return "Atak";
            case StatId.Defense: return "Obrona";
            case StatId.Block: return "Blok";
            case StatId.WalkCost: return "Koszt ruchu";
            case StatId.AttackCost: return "Koszt ataku";
            case StatId.Strength: return "Siła";
            case StatId.Knowledge: return "Wiedza";
            case StatId.MaxHealth: return "Zdrowie";
            case StatId.MaxMana: return "Mana";
            case StatId.ViewRange: return "Zasięg widzenia";
            case StatId.MaxSatiety: return "Sytość";
            case StatId.MaxHydration: return "Nawodnienie";
            case StatId.MaxSanity: return "Poczytalność";
            case StatId.HealthRegen: return "Regeneracja zdrowia";
            case StatId.ManaRegen: return "Regeneracja many";
            case StatId.SatietyBurn: return "Spalanie najedzenia";
            case StatId.HydrationBurn: return "Spalanie napicia";
            case StatId.SanityBurn: return "Spalanie poczytalności";
            case StatId.ArrowDamage: return "Obrażenia strzał";
            case StatId.BoltDamage: return "Obrażenia bełtów";
            case StatId.DartDamage: return "Obrażenia rzutek";
            case StatId.MagicResistance: return "Odporność magiczna";
            case StatId.FireResistance: return "Odporność na ogień";
            case StatId.ColdResistance: return "Odporność na zimno";
            case StatId.PoisonResistance: return "Odporność na truciznę";
            case StatId.ElectricityResistance: return "Odporność na elektryczność";
            case StatId.BleedingResistance: return "Odporność na krwawienie";
            case StatId.HungerResistance: return "Odporność na głód";
            case StatId.CriticalChance: return "Szansa na trafienie krytyczne";
            case StatId.Dodge: return "Unik";
            case StatId.CounterDodge: return "Kontra uniku";
            case StatId.MagicAmplify: return "Wzmocnienie magii";
            default: return stat.ToString();
        }
    }
}
