using System;
using System.Collections.Generic;

public class PlayerInventory
{
    public const int Capacity = 12;

    readonly List<Item> bag = new();
    readonly Dictionary<EquipmentSlot, Item> equipped = new();
    readonly PlayerStats stats;

    public event Action Changed;

    public PlayerInventory(PlayerStats stats)
    {
        this.stats = stats;
    }

    public IReadOnlyList<Item> Bag => bag;

    public Item Equipped(EquipmentSlot slot)
    {
        equipped.TryGetValue(slot, out Item item);
        return item;
    }

    public IEnumerable<EquipmentItem> EquippedGear()
    {
        foreach (Item item in equipped.Values)
        {
            if (item is EquipmentItem equipment)
                yield return equipment;
        }
    }

    public bool TryAdd(Item item)
    {
        if (item == null || item.Count < 1)
            return false;

        if (item is not EquipmentItem)
        {
            for (int i = 0; i < bag.Count; i++)
            {
                if (!SameStack(bag[i], item))
                    continue;
                if (bag[i] is Ammunition heldAmmo && item is Ammunition addedAmmo)
                {
                    heldAmmo.Merge(addedAmmo);
                    if (heldAmmo.Count < 1)
                        bag.RemoveAt(i);
                    Changed?.Invoke();
                    return true;
                }
                if (bag[i] is Food held && item is Food added && added.Age > held.Age)
                    held.Age = added.Age;
                bag[i].Count += item.Count;
                Changed?.Invoke();
                return true;
            }
        }

        if (bag.Count >= Capacity)
            return false;

        bag.Add(item);
        Changed?.Invoke();
        return true;
    }

    public bool TryTakeOne(int index, out Item item)
    {
        if (index < 0 || index >= bag.Count)
        {
            item = null;
            return false;
        }

        Item stack = bag[index];
        if (stack.Count <= 1)
        {
            bag.RemoveAt(index);
            stack.Count = 1;
            item = stack;
            Changed?.Invoke();
            return true;
        }

        if (stack is Ammunition ammo)
        {
            item = ammo.SplitOne();
            Changed?.Invoke();
            return item != null;
        }

        stack.Count--;
        item = stack.Copy();
        item.Count = 1;
        Changed?.Invoke();
        return true;
    }

    public void Touch()
    {
        Changed?.Invoke();
    }

    public void RemoveBagItem(Item item)
    {
        if (item != null && bag.Remove(item))
            Changed?.Invoke();
    }

    public void DestroyEquipped(EquipmentSlot slot)
    {
        if (equipped.Remove(slot))
            Changed?.Invoke();
    }

    public Item RandomBagPiece()
    {
        int count = 0;
        for (int i = 0; i < bag.Count; i++)
        {
            if (bag[i] != null && bag[i] is not Ammunition && bag[i].TracksDurability && bag[i].Durability > 0)
                count++;
        }

        if (count == 0)
            return null;

        int roll = UnityEngine.Random.Range(0, count);
        for (int i = 0; i < bag.Count; i++)
        {
            if (bag[i] == null || bag[i] is Ammunition || !bag[i].TracksDurability || bag[i].Durability <= 0)
                continue;
            if (roll == 0)
                return bag[i];
            roll--;
        }

        return null;
    }

    public void RechargeRunes()
    {
        bool changed = false;
        for (int i = 0; i < bag.Count; i++)
            RechargeOne(bag[i], ref changed);
        foreach (Item item in equipped.Values)
            RechargeOne(item, ref changed);

        if (changed)
            Changed?.Invoke();
    }

    static void RechargeOne(Item item, ref bool changed)
    {
        if (item is RuneStone rune && rune.Charge < rune.MaxCharges)
        {
            rune.Recharge();
            changed = true;
        }

        if (item is StaffItem staff && staff.Spell != null && staff.Spell.Charge < staff.Spell.MaxCharges)
        {
            staff.Spell.Recharge();
            changed = true;
        }
    }

    public void AgeFood()
    {
        bool changed = false;
        for (int i = 0; i < bag.Count; i++)
        {
            if (bag[i] is not Food food)
                continue;
            food.AgeOneTurn();
            changed = true;
        }

        if (changed)
            Changed?.Invoke();
    }

    static bool SameStack(Item stack, Item incoming)
    {
        if (stack == null || incoming == null || stack is RuneStone || incoming is RuneStone)
            return false;
        if (stack.GetType() != incoming.GetType() || stack.Id != incoming.Id)
            return false;
        if (stack is Food food && incoming is Food other)
            return food.Freshness == other.Freshness;
        if (stack is Ammunition)
            return true;
        if (!stack.TracksDurability)
            return true;
        return stack.Durability == incoming.Durability;
    }

    public bool TryEquipFromBag(int index)
    {
        if (index < 0 || index >= bag.Count || bag[index] is not EquipmentItem equipment)
            return false;
        if (!ItemRequirements.Met(equipment, stats))
            return false;

        equipped.TryGetValue(equipment.Slot, out Item previous);
        equipped[equipment.Slot] = equipment;
        bag.RemoveAt(index);
        if (previous != null)
            bag.Insert(index, previous);

        Changed?.Invoke();
        return true;
    }

    bool CanHold(Item item)
    {
        if (item is not EquipmentItem)
        {
            for (int i = 0; i < bag.Count; i++)
            {
                if (SameStack(bag[i], item))
                    return true;
            }
        }

        return bag.Count < Capacity;
    }

    public bool TryTakeBag(int index, out Item item)
    {
        if (index < 0 || index >= bag.Count)
        {
            item = null;
            return false;
        }

        item = bag[index];
        bag.RemoveAt(index);
        Changed?.Invoke();
        return true;
    }

    public bool TryTakeEquipped(EquipmentSlot slot, out Item item)
    {
        if (!equipped.TryGetValue(slot, out item) || item == null)
        {
            item = null;
            return false;
        }

        equipped.Remove(slot);
        Changed?.Invoke();
        return true;
    }

    public void PutEquipped(EquipmentItem equipment)
    {
        if (equipment == null)
            return;

        equipped[equipment.Slot] = equipment;
        Changed?.Invoke();
    }

    public bool TryUnequip(EquipmentSlot slot)
    {
        if (!equipped.TryGetValue(slot, out Item item) || item == null)
            return false;
        if (!CanHold(item))
            return false;

        equipped.Remove(slot);
        bag.Add(item);
        Changed?.Invoke();
        return true;
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
