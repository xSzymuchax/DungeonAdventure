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
    MaxSanity
}

[Serializable]
public class StatModifier
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

        if (save.modifiers == null)
            return item;

        for (int i = 0; i < save.modifiers.Length; i++)
        {
            if (save.modifiers[i] != null)
                item.Modifiers.Add(save.modifiers[i]);
        }

        return item;
    }
}
