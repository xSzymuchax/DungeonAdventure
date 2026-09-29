using System;
using System.Collections.Generic;

public enum ItemKind
{
    Equipment,
    Consumable
}

public enum EquipmentSlot
{
    Helmet,
    Armor,
    Amulet,
    Sword,
    Shield
}

public enum StatId
{
    Damage,
    Defense,
    Block,
    WalkCost,
    AttackCost,
    Strength,
    Knowledge,
    MaxHealth,
    MaxMana,
    ViewRange,
    MaxSatiety,
    MaxHydration,
    MaxSanity,
    HealthRegen,
    ManaRegen,
    SatietyBurn,
    HydrationBurn,
    SanityBurn
}

[Serializable]
public class StatModifier
{
    public StatId stat;
    public int value;
}

[Serializable]
public class StatRequirement
{
    public StatId stat;
    public int value;
}

public abstract class Item
{
    public string Id { get; }
    public string DisplayName { get; }
    public abstract ItemKind Kind { get; }
    public List<StatModifier> Modifiers { get; } = new();
    public List<StatRequirement> Requirements { get; } = new();

    protected Item(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }
}

public class EquipmentItem : Item
{
    public override ItemKind Kind => ItemKind.Equipment;
    public EquipmentSlot Slot { get; }

    public EquipmentItem(EquipmentSlot slot, string id, string displayName) : base(id, displayName)
    {
        Slot = slot;
    }
}

public class ConsumableItem : Item
{
    public override ItemKind Kind => ItemKind.Consumable;

    public ConsumableItem(string id, string displayName) : base(id, displayName)
    {
    }
}

public static class StatBonus
{
    public static int Sum(IEnumerable<StatModifier> modifiers, StatId stat)
    {
        if (modifiers == null)
            return 0;

        int sum = 0;
        foreach (StatModifier modifier in modifiers)
        {
            if (modifier != null && modifier.stat == stat)
                sum += modifier.value;
        }

        return sum;
    }
}

public static class ItemFactory
{
    public static Item Create(ItemSave save)
    {
        if (save == null)
            return null;

        Item item = save.kind == (int)ItemKind.Consumable
            ? new ConsumableItem(save.id, save.displayName)
            : new EquipmentItem((EquipmentSlot)save.slot, save.id, save.displayName);

        if (save.modifiers != null)
        {
            for (int i = 0; i < save.modifiers.Length; i++)
            {
                if (save.modifiers[i] != null)
                    item.Modifiers.Add(save.modifiers[i]);
            }
        }

        if (save.requirements != null)
        {
            for (int i = 0; i < save.requirements.Length; i++)
            {
                if (save.requirements[i] != null)
                    item.Requirements.Add(save.requirements[i]);
            }
        }

        return item;
    }
}

public static class ItemRequirements
{
    public static bool Met(Item item, PlayerStats stats)
    {
        if (item == null)
            return false;
        if (item.Requirements.Count == 0)
            return true;
        if (stats == null)
            return false;

        for (int i = 0; i < item.Requirements.Count; i++)
        {
            StatRequirement requirement = item.Requirements[i];
            if (requirement == null)
                continue;
            if (Value(stats, requirement.stat) < requirement.value)
                return false;
        }

        return true;
    }

    public static float Value(PlayerStats stats, StatId stat)
    {
        if (stats == null)
            return 0;

        switch (stat)
        {
            case StatId.Damage: return stats.Damage;
            case StatId.Defense: return stats.Defense;
            case StatId.Block: return stats.Block;
            case StatId.WalkCost: return stats.WalkCost;
            case StatId.AttackCost: return stats.AttackCost;
            case StatId.Strength: return stats.Strength;
            case StatId.Knowledge: return stats.Knowledge;
            case StatId.MaxHealth: return stats.MaxHealth;
            case StatId.MaxMana: return stats.MaxMana;
            case StatId.ViewRange: return stats.ViewRange;
            case StatId.MaxSatiety: return stats.MaxSatiety;
            case StatId.MaxHydration: return stats.MaxHydration;
            case StatId.MaxSanity: return stats.MaxSanity;
            case StatId.HealthRegen: return stats.HealthRegen;
            case StatId.ManaRegen: return stats.ManaRegen;
            case StatId.SatietyBurn: return stats.SatietyBurn;
            case StatId.HydrationBurn: return stats.HydrationBurn;
            case StatId.SanityBurn: return stats.SanityBurn;
            default: return 0;
        }
    }
}
