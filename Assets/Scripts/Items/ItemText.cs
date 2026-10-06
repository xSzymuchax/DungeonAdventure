public static class ItemText
{
    public const string Strike = "Siła ciosu";
    public const string Damage = "Obrażenia";
    public const string SpellPower = "Siła zaklęcia";
    public const string Spell = "zaklęcie";
    public const string Empty = "Brak bonusów.";

    public static string Requirement(string stat, int value)
    {
        return "Wymaga: " + stat + " " + value;
    }

    public static string Bonus(string stat, float value)
    {
        string sign = value >= 0 ? "+" : "";
        return stat + " " + sign + value.ToString("0.##");
    }

    public static string Durability(int current, int max)
    {
        return "Trwałość " + current + "/" + max;
    }

    public static string Amount(string label, string amount)
    {
        return label + " " + amount;
    }

    public static string Charge(float current, int max)
    {
        return "Ładunek " + current.ToString("0.0") + "/" + max;
    }

    public static string Recharge(float perTurn)
    {
        return "Regeneracja " + perTurn.ToString("0.##");
    }

    public static string Satiety(float value)
    {
        return "Najedzenie " + Signed(value);
    }

    public static string Hydration(float value)
    {
        return "Nawodnienie " + Signed(value);
    }

    public static string Sanity(float value)
    {
        return "Poczytalność " + Signed(value);
    }

    public static string HoldsRune(int runeLevel, string spell, int spellLevel, string mana)
    {
        return "Zawiera runę poziomu " + runeLevel + " z zaklęciem " + spell + " poziomu " + spellLevel
            + ". Użycie wymaga " + mana + " many.";
    }

    public static string HoldsScroll(string spell, int spellLevel, string mana)
    {
        return "Zawiera zaklęcie " + spell + " poziomu " + spellLevel + ". Użycie wymaga " + mana + " many.";
    }

    public static string Mana(float cost)
    {
        return "Mana " + cost.ToString("0.##");
    }

    static string Signed(float value)
    {
        return (value >= 0 ? "+" : "") + value.ToString("0.##");
    }
}
