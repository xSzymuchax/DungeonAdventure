using System.Collections.Generic;

public static class SpellMana
{
    public const float LevelStep = 0.5f;
    public const float StaffShare = 0.8f;

    public static float Cost(Item item)
    {
        if (item == null)
            return 0f;

        IList<Effect> effects = ItemUse.EffectsOf(item);
        if (effects == null)
            return 0f;

        int level = LevelOf(item);
        float share = item is StaffItem ? StaffShare : 1f;
        float total = 0f;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] is not SkillEffect effect || effect.skill == null)
                continue;
            total += Scaled(effect.skill.ManaCost, level) * share;
        }

        return total;
    }

    public static bool CanPay(PlayerCharacter player, Item item)
    {
        if (player is not ISkillCaster caster)
            return false;
        return caster.HasMana(Cost(item));
    }

    public static float Scaled(float baseCost, int level)
    {
        int step = level < 1 ? 1 : level;
        return baseCost * (1f + LevelStep * (step - 1));
    }

    static int LevelOf(Item item)
    {
        if (item is Scroll scroll)
            return scroll.SpellLevel;
        if (item is RuneStone rune)
            return rune.SpellLevel;
        if (item is StaffItem staff && staff.Spell != null)
            return staff.Spell.SpellLevel;
        return 1;
    }
}
