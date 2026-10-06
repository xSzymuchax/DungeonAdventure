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
    public static readonly Color Physical = Color.white;
    public static readonly Color Fire = new Color(1f, 0.38f, 0.08f);
    public static readonly Color Cold = new Color(0.45f, 0.78f, 1f);
    public static readonly Color Poison = new Color(0.4f, 0.85f, 0.22f);
    public static readonly Color Electricity = new Color(1f, 0.92f, 0.2f);
    public static readonly Color Bleeding = new Color(0.78f, 0.08f, 0.12f);
    public static readonly Color Hunger = new Color(0.72f, 0.46f, 0.16f);
    public static readonly Color Heal = new Color(0.7f, 1f, 0.65f);

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

    public static Color ColorOf(DamageType type)
    {
        switch (type)
        {
            case DamageType.Fire: return Fire;
            case DamageType.Cold: return Cold;
            case DamageType.Poison: return Poison;
            case DamageType.Electricity: return Electricity;
            case DamageType.Bleeding: return Bleeding;
            case DamageType.Hunger: return Hunger;
            default: return Physical;
        }
    }
}
