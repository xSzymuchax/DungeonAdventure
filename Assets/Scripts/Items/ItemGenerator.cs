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
        for (int i = 0; i < Equipment.Length; i++)
        {
            if (Equipment[i].slot == slot)
                return new EquipmentItem(slot, Equipment[i].id, Equipment[i].name);
        }

        return new EquipmentItem(slot, "helmet", "Hełm");
    }

    public static EquipmentItem GenerateRandom()
    {
        int index = UnityEngine.Random.Range(0, Equipment.Length);
        return Generate(Equipment[index].slot);
    }
}
