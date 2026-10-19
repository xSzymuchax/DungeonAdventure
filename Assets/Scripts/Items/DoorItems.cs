public static class DoorItems
{
    public const string SteelKeyId = "steel_key";
    public const string GoldKeyId = "gold_key";
    public const string LockpickId = "lockpick";

    public static bool IsSteelKey(Item item)
    {
        return item != null && item.Id == SteelKeyId;
    }

    public static bool IsGoldKey(Item item)
    {
        return item != null && item.Id == GoldKeyId;
    }

    public static bool IsLockpick(Item item)
    {
        return item != null && item.Id == LockpickId;
    }

    public static bool IsTool(Item item)
    {
        return IsSteelKey(item) || IsGoldKey(item) || IsLockpick(item);
    }

    public static Item SteelKey()
    {
        ResourceItem item = new ResourceItem(SteelKeyId, "Stalowy klucz");
        item.Description = "Otwiera stalową kłódkę. Zużywa się.";
        return item;
    }

    public static Item GoldKey()
    {
        ResourceItem item = new ResourceItem(GoldKeyId, "Złoty klucz");
        item.Description = "Otwiera złotą kłódkę. Zużywa się.";
        return item;
    }

    public static Item Lockpick()
    {
        ResourceItem item = new ResourceItem(LockpickId, "Wytrych");
        item.Description = "Otwiera słabą albo wredną kłódkę. Zużywa się.";
        return item;
    }
}
