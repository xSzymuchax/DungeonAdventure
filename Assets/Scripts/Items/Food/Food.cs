public enum FoodFreshness
{
    Fresh,
    Stale,
    Spoiled,
    Rotten
}

public class Food : Item
{
    public override ItemKind Kind => ItemKind.Food;
    public override bool IsConsumable => true;
    public override bool TracksDurability => false;
    public float Satiety { get; set; }
    public float Hydration { get; set; }
    public float Sanity { get; set; }
    public int TimeToEat { get; set; } = 1;
    public bool Spoils { get; set; }
    public int Age { get; set; }

    public float Quality
    {
        get
        {
            if (!Spoils)
                return 1f;

            float quality = 1f - (int)Freshness * Consts.FOOD_STAGE_LOSS;
            return quality < 0f ? 0f : quality;
        }
    }

    public FoodFreshness Freshness
    {
        get
        {
            if (!Spoils)
                return FoodFreshness.Fresh;

            int stage = Consts.FOOD_STAGE_TURNS;
            if (Age < stage)
                return FoodFreshness.Fresh;
            if (Age < stage * 2)
                return FoodFreshness.Stale;
            if (Age < stage * 3)
                return FoodFreshness.Spoiled;
            return FoodFreshness.Rotten;
        }
    }

    public int TurnsUntilNextStage
    {
        get
        {
            if (Freshness == FoodFreshness.Rotten)
                return 0;
            int intoStage = Age % Consts.FOOD_STAGE_TURNS;
            return Consts.FOOD_STAGE_TURNS - intoStage;
        }
    }

    public void AgeOneTurn()
    {
        if (Spoils)
            Age++;
    }

    public Food(string id, string displayName) : base(id, displayName)
    {
    }

    public void Eat(PlayerCharacter player)
    {
        if (player == null)
            return;

        float quality = Quality;
        if (Satiety != 0)
            player.SetSatiety(player.Satiety + Satiety * quality);
        if (Hydration != 0)
            player.SetHydration(player.Hydration + Hydration * quality);
        if (Sanity != 0)
            player.SetSanity(player.Sanity + Sanity * quality);
        float poisonChance = Freshness == FoodFreshness.Spoiled ? Consts.FOOD_SPOILED_POISON_CHANCE
            : Freshness == FoodFreshness.Rotten ? Consts.FOOD_ROTTEN_POISON_CHANCE
            : 0f;
        if (poisonChance > 0f && UnityEngine.Random.value < poisonChance)
            player.AddToken(new BurningToken(1, 3));
    }

    public override Item Copy()
    {
        Food copy = new Food(Id, DisplayName);
        copy.FillFrom(this);
        copy.Satiety = Satiety;
        copy.Hydration = Hydration;
        copy.Sanity = Sanity;
        copy.TimeToEat = TimeToEat;
        copy.Spoils = Spoils;
        copy.Age = Age;
        return copy;
    }
}
