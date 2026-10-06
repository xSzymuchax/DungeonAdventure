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

    public ISkill Fireball => fireball;

    public PlayerStats PlayerStats => GetStats() as PlayerStats;
    public PlayerInventory Inventory { get; private set; }

    private bool needsRestored;

    void Awake()
    {
        Inventory = new PlayerInventory(PlayerStats);
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
    }

    public void SetSatiety(float value)
    {
        float next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxSatiety : 0);
        if (next == Satiety)
            return;
        Satiety = next;
        SatietyChanged?.Invoke();
    }

    public void SetHydration(float value)
    {
        float next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxHydration : 0);
        if (next == Hydration)
            return;
        Hydration = next;
        HydrationChanged?.Invoke();
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

        float healthRegen = PlayerStats.HealthRegen * NeedFactor(Satiety, PlayerStats.MaxSatiety);
        float manaRegen = PlayerStats.ManaRegen * NeedFactor(Hydration, PlayerStats.MaxHydration);

        if (healthRegen > 0)
            Heal(healthRegen);
        if (manaRegen > 0)
            AddMana(manaRegen);
        else if (PlayerStats.ManaRegen < 0)
            RemoveMana(-PlayerStats.ManaRegen);

        if (IsEmpty(Satiety, PlayerStats.MaxSatiety) && PlayerStats.SatietyBurn > 0)
        {
            DamageResult hunger = DamageCalculator.Direct(this, DamageType.Hunger, PlayerStats.SatietyBurn);
            if (hunger.Amount > 0)
                TakeDamage(hunger.Amount, DamageType.Hunger, hunger.Scale);
        }
        if (IsEmpty(Hydration, PlayerStats.MaxHydration) && PlayerStats.HydrationBurn > 0)
            RemoveMana(PlayerStats.HydrationBurn);

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
        if (PlayerStats != null)
            PlayerStats.Recalculate(Inventory.Modifiers());
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
