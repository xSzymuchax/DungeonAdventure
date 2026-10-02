using UnityEngine;

public static class ItemFactory
{
    public static Item Create(ItemSave save)
    {
        if (save == null)
            return null;

        Item item = CreateBody(save);
        if (item == null)
            return null;

        if (item.TracksDurability && item is not Ammunition)
        {
            if (save.maxDurability <= 0)
            {
                if (item.MaxDurability < 1)
                    item.MaxDurability = Consts.DEFAULT_DURABILITY;
                item.Durability = item.MaxDurability;
            }
            else
            {
                item.MaxDurability = save.maxDurability;
                item.Durability = Mathf.Clamp(save.durability, 0, item.MaxDurability);
            }
        }

        if (item is Food food)
            food.Age = Mathf.Max(0, save.age);

        if (save.count > 0)
            item.Count = save.count;
        if (item is Ammunition ammunition)
            ammunition.RestoreDurability(save.durability, save.maxDurability);
        if (item is RuneStone rune)
            rune.Charge = Mathf.Clamp(save.charge, 0f, rune.MaxCharges);
        if (item is EquipmentItem equipment)
            equipment.Level = Mathf.Clamp(save.upgradeLevel, 0, Consts.MAX_UPGRADE_LEVEL);

        return item;
    }

    static Item CreateBody(ItemSave save)
    {
        switch ((ItemKind)save.kind)
        {
            case ItemKind.Food:
                return CreateFood(save);
            case ItemKind.Scroll:
                return CreateScroll(save);
            case ItemKind.Rune:
                return CreateRune(save);
            case ItemKind.Resource:
                return CreateResource(save);
            case ItemKind.Ammunition:
                return CreateAmmunition(save);
            default:
                return CreateEquipment(save);
        }
    }

    static EquipmentItem CreateEquipment(ItemSave save)
    {
        EquipmentItem item = new EquipmentItem((EquipmentSlot)save.slot, save.id, save.displayName);
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

        item.ThrownWeapon = save.thrownWeapon;
        item.ThrowDamage = save.throwDamage;
        item.Sharp = save.sharp;
        if (Catalog() != null && Catalog().TryGetEquipment(item.Id, out BaseItem definition))
        {
            item.ViewPrefab = definition.model;
            item.WeaponKind = definition.weaponKind;
        }

        return item;
    }

    static Food CreateFood(ItemSave save)
    {
        Food item = new Food(save.id, save.displayName);
        if (Catalog() != null && Catalog().TryGetFood(save.id, out FoodDefinition definition))
            ItemGenerator.ApplyFood(item, definition);
        return item;
    }

    static Scroll CreateScroll(ItemSave save)
    {
        Scroll item = new Scroll(save.id, save.displayName);
        if (Catalog() != null && Catalog().TryGetScroll(save.id, out ScrollDefinition definition))
            ItemGenerator.ApplyScroll(item, definition);
        return item;
    }

    static RuneStone CreateRune(ItemSave save)
    {
        RuneStone item = new RuneStone(save.id, save.displayName);
        if (Catalog() != null && Catalog().TryGetRune(save.id, out RuneDefinition definition))
            ItemGenerator.ApplyRune(item, definition);
        return item;
    }

    static ResourceItem CreateResource(ItemSave save)
    {
        ResourceItem item = new ResourceItem(save.id, save.displayName);
        if (Catalog() != null && Catalog().TryGetResource(save.id, out ResourceDefinition definition))
            ItemGenerator.ApplyResource(item, definition);
        return item;
    }

    static Ammunition CreateAmmunition(ItemSave save)
    {
        Ammunition item = new Ammunition(save.id, save.displayName);
        if (Catalog() != null && Catalog().TryGetAmmunition(save.id, out AmmunitionDefinition definition))
            ItemGenerator.ApplyAmmunition(item, definition);
        return item;
    }

    static ItemCatalog Catalog()
    {
        return GameController.Instance != null ? GameController.Instance.baseItems : null;
    }
}
