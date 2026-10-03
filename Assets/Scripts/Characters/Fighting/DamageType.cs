using UnityEngine;

public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Poison,
    Electricity,
    Bleeding,
    Hunger
}

public static class DamageTypes
{
    public static bool IsUnavoidable(DamageType type)
    {
        return type == DamageType.Poison || type == DamageType.Bleeding || type == DamageType.Hunger;
    }

    public static bool IsElemental(DamageType type)
    {
        return type == DamageType.Fire || type == DamageType.Cold || type == DamageType.Electricity;
    }

    public static bool IsMagical(DamageType type)
    {
        return IsElemental(type);
    }

    public static string Label(DamageType type)
    {
        switch (type)
        {
            case DamageType.Fire: return "Ogień";
            case DamageType.Cold: return "Zimno";
            case DamageType.Poison: return "Trucizna";
            case DamageType.Electricity: return "Elektryczność";
            case DamageType.Bleeding: return "Krwawienie";
            case DamageType.Hunger: return "Głód";
            default: return "Fizyczne";
        }
    }
}

public static class DamageRoll
{
    public static int Of(float baseAmount)
    {
        if (baseAmount <= 0f)
            return 0;

        float rolled = baseAmount * Random.Range(0.75f, 1.25f);
        return Mathf.FloorToInt(rolled + 0.5f);
    }
}
