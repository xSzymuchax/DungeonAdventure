public static class FoodLabel
{
    public static string Name(FoodFreshness freshness)
    {
        switch (freshness)
        {
            case FoodFreshness.Fresh: return "Świeże";
            case FoodFreshness.Stale: return "Nieświeże";
            case FoodFreshness.Spoiled: return "Zepsute";
            default: return "Zgnite";
        }
    }

    public static string Line(Food food)
    {
        string stage = Name(food.Freshness);
        if (food.Freshness == FoodFreshness.Rotten)
            return stage;
        return stage + ", zostało " + food.TurnsUntilNextStage + " tur";
    }
}
