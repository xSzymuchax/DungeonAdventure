using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneStone : Item
{
    public override ItemKind Kind => ItemKind.Rune;
    public override bool TracksDurability => false;
    public List<Effect> Effects { get; } = new();
    public int MaxCharges { get; private set; } = 1;
    public int Level { get; private set; } = 1;
    public int ThrowDamage { get; set; }
    public float Charge { get; set; }
    public const int MaxLevel = 3;

    public float RechargePerTurn => Consts.RUNE_CHARGE_PER_TURN * (1f + 0.2f * (Level - 1));

    int baseMaxCharges = 1;

    public bool ChargeReady => Charge >= 1f;

    public RuneStone(string id, string displayName) : base(id, displayName)
    {
    }

    public void SetBaseMaxCharges(int max)
    {
        baseMaxCharges = max < 1 ? 1 : max;
        MaxCharges = baseMaxCharges + (Level - 1);
        Charge = Mathf.Min(Charge, MaxCharges);
    }

    public void SetLevel(int level)
    {
        Level = Mathf.Clamp(level < 1 ? 1 : level, 1, MaxLevel);
        MaxCharges = baseMaxCharges + (Level - 1);
        Charge = Mathf.Min(Charge, MaxCharges);
    }

    public void Recharge()
    {
        Charge = Mathf.Min(MaxCharges, Charge + RechargePerTurn);
    }

    public IEnumerator UseRune(PlayerCharacter player, Position2D target)
    {
        ItemGenerator.EnsureEffects(this);
        yield return Effect.ApplyAll(Effects, player, target);
    }

    public override Item Copy()
    {
        RuneStone copy = new RuneStone(Id, DisplayName);
        copy.FillFrom(this);
        copy.SetBaseMaxCharges(baseMaxCharges);
        copy.SetLevel(Level);
        copy.ThrowDamage = ThrowDamage;
        copy.Charge = Charge;
        copy.Effects.AddRange(Effects);
        return copy;
    }
}
