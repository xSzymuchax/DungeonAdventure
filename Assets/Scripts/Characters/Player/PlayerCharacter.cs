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

    public int Satiety { get; private set; }
    public int Hydration { get; private set; }
    public int Sanity { get; private set; }

    public ISkill Fireball => fireball;

    public PlayerStats PlayerStats => GetStats() as PlayerStats;

    protected override void OnStarted()
    {
        if (PlayerStats == null)
            return;

        Satiety = PlayerStats.MaxSatiety;
        Hydration = PlayerStats.MaxHydration;
        Sanity = PlayerStats.MaxSanity;
    }

    public void SetSatiety(int value)
    {
        int next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxSatiety : 0);
        if (next == Satiety)
            return;
        Satiety = next;
        SatietyChanged?.Invoke();
    }

    public void SetHydration(int value)
    {
        int next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxHydration : 0);
        if (next == Hydration)
            return;
        Hydration = next;
        HydrationChanged?.Invoke();
    }

    public void SetSanity(int value)
    {
        int next = ClampNeed(value, PlayerStats != null ? PlayerStats.MaxSanity : 0);
        if (next == Sanity)
            return;
        Sanity = next;
        SanityChanged?.Invoke();
    }

    private static int ClampNeed(int value, int max)
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
