using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RuneRecipe", menuName = "Dungeon/Rune Recipe")]
public class RuneRecipe : ScriptableObject
{
    public const string InscribedPrefix = "inscribed:";

    public RecipeIngredient[] ingredients;
    public RuneDefinition result;
    public bool inscribe;

    public bool Inscribes => inscribe;

    public static int InscribeChance(int spellLevel, int knowledge)
    {
        int level = spellLevel < 1 ? 1 : spellLevel;
        int chance = 90 - (level - 1) * 10 + knowledge;
        if (chance > 100)
            return 100;
        if (chance < 0)
            return 0;
        return chance;
    }

    public static bool InscribeSucceeds(int spellLevel, int knowledge)
    {
        return UnityEngine.Random.Range(0, 100) < InscribeChance(spellLevel, knowledge);
    }

    public bool HasIngredients(IReadOnlyList<Item> slots)
    {
        if (inscribe)
            return IsInscription(slots);
        return UpgradeRecipe.MatchesExactly(slots, ingredients);
    }

    public Item CreateResult(IReadOnlyList<Item> slots)
    {
        if (inscribe)
            return Inscribe(slots);

        if (result == null || string.IsNullOrEmpty(result.id))
            return null;

        RuneStone rune = new RuneStone(result.id, result.displayName);
        ItemGenerator.ApplyRune(rune, result);
        rune.SetLevel(1);
        rune.SpellLevel = 1;
        rune.Charge = 0f;
        return rune;
    }

    public Scroll PlacedScroll(IReadOnlyList<Item> slots)
    {
        FindPair(slots, out Scroll scroll, out _);
        return scroll;
    }

    public bool Spend(Item[] slots)
    {
        if (!inscribe || !IsInscription(slots))
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i] = null;
        }

        return true;
    }

    static bool IsInscription(IReadOnlyList<Item> slots)
    {
        if (!FindPair(slots, out Scroll scroll, out RuneStone rune))
            return false;
        return scroll != null && rune != null && UpgradeRecipe.CountOf(slots, rune.Id) == 1 && Present(slots) == 2;
    }

    static RuneStone Inscribe(IReadOnlyList<Item> slots)
    {
        if (!FindPair(slots, out Scroll scroll, out RuneStone vessel) || scroll == null || vessel == null)
            return null;

        ItemGenerator.EnsureEffects(scroll);
        string spell = SpellLabel(scroll);
        string name = string.IsNullOrEmpty(spell) ? scroll.DisplayName : spell;
        RuneStone rune = new RuneStone(InscribedPrefix + scroll.Id, name);
        rune.ViewPrefab = vessel.ViewPrefab;
        rune.Description = "";
        int charges = vessel.MaxCharges - (vessel.Level - 1);
        rune.SetBaseMaxCharges(charges < 1 ? 1 : charges);
        rune.SetLevel(vessel.Level);
        rune.SpellLevel = scroll.SpellLevel < 1 ? 1 : scroll.SpellLevel;
        rune.ThrowDamage = vessel.ThrowDamage;
        rune.Charge = vessel.Charge;
        for (int i = 0; i < scroll.Effects.Count; i++)
        {
            if (scroll.Effects[i] != null)
                rune.Effects.Add(scroll.Effects[i]);
        }

        return rune;
    }

    static bool FindPair(IReadOnlyList<Item> slots, out Scroll scroll, out RuneStone rune)
    {
        scroll = null;
        rune = null;
        if (slots == null)
            return false;

        for (int i = 0; i < slots.Count; i++)
        {
            Item slot = slots[i];
            if (slot == null)
                continue;
            if (slot is Scroll foundScroll)
            {
                if (scroll != null)
                    return false;
                scroll = foundScroll;
                continue;
            }

            if (slot is RuneStone foundRune && UpgradeRecipe.SameId(foundRune.Id, "emptyRune"))
            {
                if (rune != null)
                    return false;
                rune = foundRune;
                continue;
            }

            return false;
        }

        return true;
    }

    static int Present(IReadOnlyList<Item> slots)
    {
        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
                total += slots[i].Count;
        }

        return total;
    }

    static string SpellLabel(Scroll scroll)
    {
        for (int i = 0; i < scroll.Effects.Count; i++)
        {
            if (scroll.Effects[i] is SkillEffect spell && !string.IsNullOrEmpty(spell.Label))
                return spell.Label;
        }

        return "";
    }
}
