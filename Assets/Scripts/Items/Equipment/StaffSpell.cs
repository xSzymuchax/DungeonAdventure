using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffSpell
{
    public List<Effect> Effects { get; } = new();
    public int SpellLevel { get; set; } = 1;
    public int RuneLevel { get; private set; } = 1;
    public int MaxCharges { get; private set; } = 1;
    public float Charge { get; set; }
    public const float RechargePace = 2f;

    int baseMaxCharges = 1;
    int extraCharges;

    public bool ChargeReady => Charge >= 1f;
    public float RechargePerTurn => Consts.RUNE_CHARGE_PER_TURN * (1f + 0.2f * (RuneLevel - 1)) * RechargePace;

    public void SetBaseMaxCharges(int max)
    {
        baseMaxCharges = max < 1 ? 1 : max;
        RefreshCharges();
    }

    public void SetRuneLevel(int next)
    {
        RuneLevel = Mathf.Clamp(next < 1 ? 1 : next, 1, RuneStone.MaxLevel);
    }

    public void SetExtraCharges(int count)
    {
        extraCharges = count < 0 ? 0 : count;
        RefreshCharges();
    }

    public void Recharge()
    {
        Charge = Mathf.Min(MaxCharges, Charge + RechargePerTurn);
    }

    public IEnumerator Use(PlayerCharacter player, Position2D target)
    {
        ItemGenerator.EnsureStaffEffects(this);
        ArmSpellLevel();
        try
        {
            yield return Effect.ApplyAll(Effects, player, target);
        }
        finally
        {
            DisarmSpellLevel();
        }
    }

    public StaffSpell Copy()
    {
        StaffSpell copy = new StaffSpell();
        copy.SetBaseMaxCharges(baseMaxCharges);
        copy.SetExtraCharges(extraCharges);
        copy.SetRuneLevel(RuneLevel);
        copy.SpellLevel = SpellLevel;
        copy.Charge = Charge;
        copy.Effects.AddRange(Effects);
        return copy;
    }

    void RefreshCharges()
    {
        int previous = MaxCharges;
        bool full = Charge >= previous;
        MaxCharges = baseMaxCharges + extraCharges;
        if (MaxCharges < 1)
            MaxCharges = 1;
        Charge = full ? MaxCharges : Mathf.Min(Charge, MaxCharges);
    }

    void ArmSpellLevel()
    {
        for (int i = 0; i < Effects.Count; i++)
        {
            if (Effects[i] is SkillEffect effect && effect.skill != null)
                effect.skill.BeginCast(SpellLevel);
        }
    }

    void DisarmSpellLevel()
    {
        for (int i = 0; i < Effects.Count; i++)
        {
            if (Effects[i] is SkillEffect effect && effect.skill != null)
                effect.skill.EndCast();
        }
    }
}
