using System.Collections.Generic;

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
    readonly List<int> ages = new List<int>();

    public int Age
    {
        get
        {
            if (!Spoils)
                return 0;
            EnsureAges();
            return ages[SoonestIndex()];
        }
    }

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
        if (!Spoils)
            return;

        EnsureAges();
        for (int i = 0; i < ages.Count; i++)
            ages[i]++;
    }

    public void Absorb(Food other)
    {
        if (other == null)
            return;
        if (!Spoils || !other.Spoils)
        {
            Count += other.Count < 1 ? 1 : other.Count;
            return;
        }

        EnsureAges();
        other.EnsureAges();
        ages.AddRange(other.ages);
        Count = ages.Count;
    }

    public Food SplitSoonest()
    {
        EnsureAges();
        if (ages.Count <= 1)
            return this;

        int index = SoonestIndex();
        int age = ages[index];
        ages.RemoveAt(index);
        Count = ages.Count;

        Food one = UntimedCopy();
        one.Count = 1;
        one.ages.Add(age);
        return one;
    }

    public int[] Ages()
    {
        if (!Spoils)
            return null;
        EnsureAges();
        return ages.ToArray();
    }

    public void RestoreAges(int[] savedAges, int savedAge, int count)
    {
        Count = count < 1 ? 1 : count;
        ages.Clear();
        if (!Spoils)
            return;

        if (savedAges != null && savedAges.Length > 0)
        {
            for (int i = 0; i < Count; i++)
            {
                int age = i < savedAges.Length ? savedAges[i] : savedAges[savedAges.Length - 1];
                ages.Add(age < 0 ? 0 : age);
            }
            return;
        }

        int fallback = savedAge < 0 ? 0 : savedAge;
        for (int i = 0; i < Count; i++)
            ages.Add(fallback);
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
            player.AddToken(new PoisonToken(1, 3));
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
        if (Spoils)
        {
            EnsureAges();
            copy.ages.AddRange(ages);
            copy.Count = ages.Count;
        }
        return copy;
    }

    Food UntimedCopy()
    {
        Food copy = new Food(Id, DisplayName);
        copy.FillFrom(this);
        copy.Satiety = Satiety;
        copy.Hydration = Hydration;
        copy.Sanity = Sanity;
        copy.TimeToEat = TimeToEat;
        copy.Spoils = Spoils;
        return copy;
    }

    void EnsureAges()
    {
        if (!Spoils)
            return;

        int count = Count < 1 ? 1 : Count;
        while (ages.Count < count)
            ages.Add(0);
        while (ages.Count > count)
            ages.RemoveAt(ages.Count - 1);
    }

    int SoonestIndex()
    {
        int best = 0;
        for (int i = 1; i < ages.Count; i++)
        {
            int turns = TurnsLeft(ages[i]);
            int bestTurns = TurnsLeft(ages[best]);
            if (turns < bestTurns || (turns == bestTurns && ages[i] > ages[best]))
                best = i;
        }

        return best;
    }

    static int TurnsLeft(int age)
    {
        int stage = Consts.FOOD_STAGE_TURNS;
        if (stage < 1)
            stage = 1;
        if (age >= stage * 3)
            return 0;
        return stage - age % stage;
    }
}
