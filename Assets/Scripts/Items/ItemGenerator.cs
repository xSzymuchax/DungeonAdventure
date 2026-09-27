public static class ItemGenerator
{
    static readonly (EquipmentSlot slot, string id, string name)[] Equipment =
    {
        (EquipmentSlot.Helmet, "helmet", "Hełm"),
        (EquipmentSlot.Armor, "armor", "Zbroja"),
        (EquipmentSlot.Amulet, "amulet", "Amulet"),
        (EquipmentSlot.Sword, "sword", "Miecz"),
        (EquipmentSlot.Shield, "shield", "Tarcza")
    };

    public static EquipmentItem Generate(EquipmentSlot slot)
    {
        EquipmentItem item = null;
        for (int i = 0; i < Equipment.Length; i++)
        {
            if (Equipment[i].slot != slot)
                continue;

            item = new EquipmentItem(slot, Equipment[i].id, Equipment[i].name);
            break;
        }

        if (item.Slot == EquipmentSlot.Armor)
            item.Modifiers.Add(new StatModifier { stat = StatId.Defense, value = 2 });
            
        if (item.Slot == EquipmentSlot.Sword)
        {
            item.Modifiers.Add(new StatModifier { stat = StatId.Damage, value = 2 });
            item.Requirements.Add(new StatRequirement { stat = StatId.Strength, value = 10 });
        }

        return item;
    }

    public static EquipmentItem GenerateRandom()
    {
        int index = UnityEngine.Random.Range(0, Equipment.Length);
        return Generate(Equipment[index].slot);
    }
}
