using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    [SerializeField] FireballSkill fireball;

    public int ViewRange => GetStats().ViewRange;
    public int Strength => PlayerStats.Strength;
    public int Knowledge => PlayerStats.Knowledge;

    public int Satiety { get; set; }
    public int Hydration { get; set; }
    public int Sanity { get; set; }

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

    protected override void PopulateSkills()
    {
        base.PopulateSkills();
        if (fireball != null)
            skills.Add(fireball);
    }
}
