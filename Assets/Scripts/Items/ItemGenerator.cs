using UnityEngine;

public static class ItemGenerator
{
    const float NextModifierChance = 0.2f;
    const int MaxModifiers = 3;

    public static Item GenerateRandom(BaseItemCatalog catalog)
    {
        if (catalog == null)
            return null;

        BaseItem definition = catalog.Roll();
        if (definition == null)
            return null;

        Item item = definition.kind == ItemKind.Consumable
            ? new ConsumableItem(definition.id, definition.displayName)
            : new EquipmentItem(definition.slot, definition.id, definition.displayName);

        item.ViewPrefab = definition.model;
        CopyRequirements(item, definition);
        RollModifiers(item, definition, catalog.statRolls);
        item.ThrownWeapon = definition.thrownWeapon;
        item.ThrowDamage = definition.throwDamage;
        item.Sharp = definition.sharp;
        return item;
    }

    static void CopyRequirements(Item item, BaseItem definition)
    {
        if (definition.requirements == null)
            return;

        for (int i = 0; i < definition.requirements.Length; i++)
        {
            StatRequirement requirement = definition.requirements[i];
            if (requirement == null)
                continue;
            item.Requirements.Add(new StatRequirement { stat = requirement.stat, value = requirement.value });
        }
    }

    static void RollModifiers(Item item, BaseItem definition, StatRollTable rolls)
    {
        int fixedCount = CopyFixed(item, definition);
        int rolled = 0;
        if (fixedCount == 0)
        {
            if (!TryAddBonus(item, definition, rolls))
                return;
            rolled = 1;
        }

        while (rolled < MaxModifiers && Random.value < NextModifierChance)
        {
            if (!TryAddBonus(item, definition, rolls))
                return;
            rolled++;
        }
    }

    static int CopyFixed(Item item, BaseItem definition)
    {
        if (definition.modifiers == null)
            return 0;

        int count = 0;
        for (int i = 0; i < definition.modifiers.Length; i++)
        {
            BaseModifier modifier = definition.modifiers[i];
            if (modifier == null)
                continue;
            item.Modifiers.Add(new StatModifier { stat = modifier.stat, value = modifier.value });
            count++;
        }

        return count;
    }

    static bool TryAddBonus(Item item, BaseItem definition, StatRollTable rolls)
    {
        if (rolls == null || !rolls.TryPick(item, out StatId stat, out float value))
            return false;

        item.Modifiers.Add(new StatModifier { stat = stat, value = Scale(stat, value, definition.tier) });
        return true;
    }

    static float Scale(StatId stat, float value, int tier)
    {
        if (stat == StatId.Strength || stat == StatId.Knowledge || stat == StatId.WalkCost || stat == StatId.AttackCost || stat == StatId.ViewRange)
            return value;
        return value * Mathf.Max(1, tier);
    }
}
