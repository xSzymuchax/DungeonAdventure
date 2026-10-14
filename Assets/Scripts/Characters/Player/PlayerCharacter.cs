using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    [SerializeField] FireballSkill fireball;

    public int ViewRange => GetStats().ViewRange;
    public int Strength => PlayerStats.Strength;
    public int Knowledge => PlayerStats.Knowledge;

    public event System.Action SatietyChanged;
    public event System.Action HydrationChanged;
    public event System.Action SanityChanged;

    public float Satiety { get; private set; }
    public float Hydration { get; private set; }
    public float Sanity { get; private set; }
    public float AccumulatedRegenHealth => accumulatedRegenHealth;
    public float AccumulatedRegenMana => accumulatedRegenMana;

    float accumulatedRegenHealth;
    float accumulatedRegenMana;

    public ISkill Fireball => fireball;

    public PlayerStats PlayerStats => GetStats() as PlayerStats;
    public PlayerInventory Inventory { get; private set; }

    private bool needsRestored;

    void Awake()
    {
        PlayerStats sheet = GetComponent<PlayerStats>();
        Inventory = new PlayerInventory(sheet);
        SetStatsProvider(new StatsProvider(sheet, Inventory));
    }

    protected override void OnStarted()
    {
        Inventory.Changed -= RefreshStats;
        Inventory.Changed += RefreshStats;
        RefreshStats();
        if (needsRestored || PlayerStats == null)
            return;

        Satiety = PlayerStats.MaxSatiety;
        Hydration = PlayerStats.MaxHydration;
        Sanity = PlayerStats.MaxSanity;
    }

    public void RestoreNeeds(float satiety, float hydration, float sanity)
    {
        needsRestored = true;
        Satiety = satiety;
        Hydration = hydration;
        Sanity = sanity;
        SatietyChanged?.Invoke();
        HydrationChanged?.Invoke();
        SanityChanged?.Invoke();
        RefreshNeedTokens();
    }

    public void RestoreRegen(float health, float mana)
    {
        accumulatedRegenHealth = health;
        accumulatedRegenMana = mana;
    }

    public void SetSatiety(float value)
    {
        float next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxSatiety : 0);
        if (next == Satiety)
            return;
        Satiety = next;
        SatietyChanged?.Invoke();
        RefreshNeedTokens();
    }

    public void SetHydration(float value)
    {
        float next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxHydration : 0);
        if (next == Hydration)
            return;
        Hydration = next;
        HydrationChanged?.Invoke();
        RefreshNeedTokens();
    }

    public void SetSanity(float value)
    {
        float next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxSanity : 0);
        if (next == Sanity)
            return;
        Sanity = next;
        SanityChanged?.Invoke();
    }

    const float HalfNeed = 0.5f;
    const float StopNeed = 0.1f;
    const float FullResourceBurn = 0.5f;

    public void ApplyTurnUpkeep()
    {
        if (IsDead || PlayerStats == null)
            return;

        if (PlayerStats.CanRegen)
            accumulatedRegenHealth += PlayerStats.HealthRegen * NeedFactor(Satiety, PlayerStats.MaxSatiety);
        if (IsEmpty(Satiety, PlayerStats.MaxSatiety) && PlayerStats.SatietyBurn > 0)
            accumulatedRegenHealth -= PlayerStats.SatietyBurn;
        if (PlayerStats.CanRegen && accumulatedRegenHealth >= 1f)
        {
            float whole = Mathf.Floor(accumulatedRegenHealth);
            Heal(whole);
            accumulatedRegenHealth -= whole;
        }
        else if (accumulatedRegenHealth <= -1f)
        {
            float whole = Mathf.Floor(-accumulatedRegenHealth);
            DamageResult hunger = DamageCalculator.Direct(this, DamageType.Hunger, whole);
            if (hunger.Amount > 0)
                TakeDamage(hunger.Amount, DamageType.Hunger, hunger.Scale);
            accumulatedRegenHealth += whole;
        }

        accumulatedRegenMana += PlayerStats.ManaRegen * NeedFactor(Hydration, PlayerStats.MaxHydration);
        if (IsEmpty(Hydration, PlayerStats.MaxHydration) && PlayerStats.HydrationBurn > 0)
            accumulatedRegenMana -= PlayerStats.HydrationBurn;
        if (accumulatedRegenMana >= 1f)
        {
            float whole = Mathf.Floor(accumulatedRegenMana);
            AddMana(whole);
            accumulatedRegenMana -= whole;
        }
        else if (accumulatedRegenMana <= -1f)
        {
            float whole = Mathf.Floor(-accumulatedRegenMana);
            RemoveMana(whole);
            accumulatedRegenMana += whole;
        }

        float satietyBurn = PlayerStats.SatietyBurn;
        if (MaxHealth > 0 && Health >= MaxHealth)
            satietyBurn *= FullResourceBurn;
        float hydrationBurn = PlayerStats.HydrationBurn;
        if (PlayerStats.MaxMana > 0 && Mana >= PlayerStats.MaxMana)
            hydrationBurn *= FullResourceBurn;

        SetSatiety(Satiety - satietyBurn);
        SetHydration(Hydration - hydrationBurn);
        SetSanity(Sanity - PlayerStats.SanityBurn);
        if (Inventory != null)
        {
            Inventory.RechargeRunes();
            Inventory.AgeFood();
            Durability.RollBag(this);
        }

        if (GameController.Instance != null && GameController.Instance.dungeon != null)
            GameController.Instance.dungeon.AgeFood();
    }

    protected override void OnDamaged()
    {
        Durability.OnPlayerHit(this);
    }

    public void RefreshNeedTokens()
    {
        if (PlayerStats == null)
            return;

        ApplyNeed(Satiety, PlayerStats.MaxSatiety, new LowSatietyToken(0, 1), new StrongHungerToken(0, 1), new StarvingToken(0, 1));
        ApplyNeed(Hydration, PlayerStats.MaxHydration, new LowHydrationToken(0, 1), new StrongThirstToken(0, 1), new DehydratedToken(0, 1));
    }

    void ApplyNeed(float current, float max, IToken slow, IToken stopped, IToken empty)
    {
        bool isEmpty = IsEmpty(current, max);
        float ratio = NeedRatio(current, max);
        SetNeedToken(!isEmpty && ratio <= HalfNeed && ratio > StopNeed, slow);
        SetNeedToken(!isEmpty && ratio <= StopNeed, stopped);
        SetNeedToken(isEmpty, empty);
    }

    void SetNeedToken(bool present, IToken token)
    {
        IReadOnlyList<IToken> active = ActiveTokens;
        IToken held = null;
        for (int i = 0; i < active.Count; i++)
        {
            if (active[i] != null && active[i].TokenType == token.TokenType)
            {
                held = active[i];
                break;
            }
        }

        if (present)
        {
            if (held == null)
                AddToken(token);
        }
        else if (held != null)
            RemoveToken(held);
    }

    static float NeedFactor(float current, float max)
    {
        float ratio = NeedRatio(current, max);
        if (ratio <= StopNeed)
            return 0;
        if (ratio <= HalfNeed)
            return 0.5f;
        return 1;
    }

    static bool IsEmpty(float current, float max)
    {
        return max > 0 && current <= 0;
    }

    static float NeedRatio(float current, float max)
    {
        if (max <= 0)
            return 1;
        return current / max;
    }

    void RefreshStats()
    {
        GetStats();
    }

    private static float ClampNeed(float value, float max)
    {
        if (max <= 0)
            return 0;
        return Mathf.Clamp(value, 0, max);
    }

    protected override void PopulateSkills()
    {
        base.PopulateSkills();
        if (fireball != null)
            skills.Add(fireball);
    }
}
