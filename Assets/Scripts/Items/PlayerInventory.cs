using System;
using System.Collections.Generic;

public class PlayerInventory
{
    public const int Capacity = 12;

    readonly List<Item> bag = new();
    readonly Dictionary<EquipmentSlot, Item> equipped = new();

    public event Action Changed;

    public IReadOnlyList<Item> Bag => bag;

    public Item Equipped(EquipmentSlot slot)
    {
        equipped.TryGetValue(slot, out Item item);
        return item;
    }

    public bool TryAdd(Item item)
    {
        if (item == null || bag.Count >= Capacity)
            return false;

        bag.Add(item);
        Changed?.Invoke();
        return true;
    }

    public bool TryEquipFromBag(int index)
    {
        if (index < 0 || index >= bag.Count || bag[index] is not EquipmentItem equipment)
            return false;

        equipped.TryGetValue(equipment.Slot, out Item previous);
        equipped[equipment.Slot] = equipment;
        bag.RemoveAt(index);
        if (previous != null)
            bag.Insert(index, previous);

        Changed?.Invoke();
        return true;
    }

    public bool TryUnequip(EquipmentSlot slot)
    {
        if (!equipped.TryGetValue(slot, out Item item) || item == null)
            return false;
        if (bag.Count >= Capacity)
            return false;

        equipped.Remove(slot);
        bag.Add(item);
        Changed?.Invoke();
        return true;
    }

    public IEnumerable<StatModifier> Modifiers()
    {
        foreach (Item item in equipped.Values)
        {
            if (item == null)
                continue;

            for (int i = 0; i < item.Modifiers.Count; i++)
                yield return item.Modifiers[i];
        }
    }

    public void Replace(ItemSave[] savedBag, ItemSave[] savedEquipped)
    {
        bag.Clear();
        equipped.Clear();

        if (savedBag != null)
        {
            for (int i = 0; i < savedBag.Length && bag.Count < Capacity; i++)
            {
                Item item = ItemFactory.Create(savedBag[i]);
                if (item != null)
                    bag.Add(item);
            }
        }

        if (savedEquipped != null)
        {
            for (int i = 0; i < savedEquipped.Length; i++)
            {
                if (ItemFactory.Create(savedEquipped[i]) is not EquipmentItem equipment)
                    continue;
                equipped[equipment.Slot] = equipment;
            }
        }

        Changed?.Invoke();
    }
}
