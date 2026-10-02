using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneStone : Item
{
    public override ItemKind Kind => ItemKind.Rune;
    public override bool TracksDurability => false;
    public List<Effect> Effects { get; } = new();
    public int MaxCharges { get; set; } = 1;
    public int ThrowDamage { get; set; }
    public float Charge { get; set; }

    public bool ChargeReady => Charge >= 1f;

    public RuneStone(string id, string displayName) : base(id, displayName)
    {
    }

    public void Recharge()
    {
        Charge = Mathf.Min(MaxCharges, Charge + Consts.RUNE_CHARGE_PER_TURN);
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
        copy.MaxCharges = MaxCharges;
        copy.ThrowDamage = ThrowDamage;
        copy.Charge = Charge;
        copy.Effects.AddRange(Effects);
        return copy;
    }
}
