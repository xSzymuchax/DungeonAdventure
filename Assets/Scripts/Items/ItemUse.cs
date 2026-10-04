using System.Collections.Generic;

public static class ItemUse
{
    public static bool CanUse(Item item)
    {
        if (item is Food)
            return true;
        if (item is Scroll scroll)
        {
            ItemGenerator.EnsureEffects(scroll);
            return scroll.Effects.Count > 0;
        }
        if (item is RuneStone rune)
        {
            ItemGenerator.EnsureEffects(rune);
            return rune.ChargeReady && rune.Effects.Count > 0;
        }
        if (item is StaffItem staff && staff.Spell != null)
        {
            ItemGenerator.EnsureEffects(staff);
            return staff.Spell.ChargeReady && staff.Spell.Effects.Count > 0;
        }
        return false;
    }

    public static bool CanRepair(Item item)
    {
        return item != null && item is not Ammunition && item.TracksDurability && item.Durability < item.MaxDurability;
    }

    public static bool NeedsTarget(Item item)
    {
        IList<Effect> effects = EffectsOf(item);
        if (effects == null)
            return false;

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null && effects[i].NeedsTarget)
                return true;
        }

        return false;
    }

    public static bool InRange(PlayerCharacter player, Item item, Position2D tile)
    {
        if (player == null)
            return false;

        IList<Effect> effects = EffectsOf(item);
        if (effects == null)
            return true;

        for (int i = 0; i < effects.Count; i++)
        {
            Effect effect = effects[i];
            if (effect == null || !effect.NeedsTarget)
                continue;
            int distance = player.Position.ChebyshevTo(tile);
            if (distance <= 0 || distance > effect.Range)
                return false;
        }

        return true;
    }

    public static IList<Effect> EffectsOf(Item item)
    {
        ItemGenerator.EnsureEffects(item);
        if (item is Scroll scroll)
            return scroll.Effects;
        if (item is RuneStone rune)
            return rune.Effects;
        if (item is StaffItem staff && staff.Spell != null)
            return staff.Spell.Effects;
        return null;
    }
}
