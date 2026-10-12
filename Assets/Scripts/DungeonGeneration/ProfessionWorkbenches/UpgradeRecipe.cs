using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class RecipeIngredient
{
    public string itemId;
    public int count = 1;
}

[CreateAssetMenu(fileName = "UpgradeRecipe", menuName = "Dungeon/Upgrade Recipe")]
public class UpgradeRecipe : ScriptableObject
{
    public string[] itemIds;
    public RecipeIngredient[] ingredients;

    public bool Accepts(Item item)
    {
        if (item == null || itemIds == null)
            return false;

        for (int i = 0; i < itemIds.Length; i++)
        {
            if (SameId(item.Id, itemIds[i]))
                return true;
        }

        return false;
    }

    public bool HasIngredients(IReadOnlyList<Item> slots)
    {
        return MatchesExactly(slots, ingredients);
    }

    public static bool MatchesExactly(IReadOnlyList<Item> slots, RecipeIngredient[] ingredients)
    {
        int requiredTotal = 0;
        if (ingredients != null)
        {
            for (int i = 0; i < ingredients.Length; i++)
            {
                RecipeIngredient ingredient = ingredients[i];
                if (ingredient == null || ingredient.count <= 0 || string.IsNullOrEmpty(ingredient.itemId))
                    continue;

                int needed = 0;
                bool countedEarlier = false;
                for (int j = 0; j < ingredients.Length; j++)
                {
                    RecipeIngredient other = ingredients[j];
                    if (other == null || other.count <= 0 || !SameId(other.itemId, ingredient.itemId))
                        continue;
                    if (j < i)
                        countedEarlier = true;
                    needed += other.count;
                }

                if (countedEarlier)
                    continue;
                if (CountOf(slots, ingredient.itemId) != needed)
                    return false;
                requiredTotal += needed;
            }
        }

        return PresentCount(slots) == requiredTotal;
    }

    public static bool SameId(string left, string right)
    {
        if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            return false;
        return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static int CountOf(IReadOnlyList<Item> slots, string id)
    {
        if (slots == null || string.IsNullOrEmpty(id))
            return 0;

        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            Item slot = slots[i];
            if (slot != null && SameId(slot.Id, id))
                total += slot.Count;
        }

        return total;
    }

    static int PresentCount(IReadOnlyList<Item> slots)
    {
        if (slots == null)
            return 0;

        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            Item slot = slots[i];
            if (slot != null)
                total += slot.Count;
        }

        return total;
    }

    public static string UpgradedStats(EquipmentItem item)
    {
        if (item == null || item.Modifiers.Count == 0)
            return "";

        float factor = ItemUpgrade.Factor(item.Level + 1);
        StringBuilder text = new StringBuilder();
        for (int i = 0; i < item.Modifiers.Count; i++)
        {
            StatModifier modifier = item.Modifiers[i];
            if (modifier == null)
                continue;

            if (text.Length > 0)
                text.Append('\n');

            float value = modifier.value * factor;
            text.Append(StatLabel.Name(modifier.stat, value));
            text.Append(": ");
            text.Append(Format(value));
        }

        return text.ToString();
    }

    static string Format(float value)
    {
        float rounded = Mathf.Round(value * 100f) / 100f;
        if (Mathf.Abs(rounded - Mathf.Round(rounded)) < 0.001f)
            return Mathf.RoundToInt(rounded).ToString();
        return rounded.ToString("0.##");
    }
}
