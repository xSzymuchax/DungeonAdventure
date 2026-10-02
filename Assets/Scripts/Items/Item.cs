using UnityEngine;

public enum ItemKind
{
    Equipment = 0,
    Food = 1,
    Scroll = 2,
    Rune = 3,
    Resource = 4,
    Ammunition = 5
}

public abstract class Item
{
    public string Id { get; }
    public string DisplayName { get; }
    public abstract ItemKind Kind { get; }
    public virtual bool IsConsumable => false;
    public virtual bool TracksDurability => true;
    public GameObject ViewPrefab { get; set; }
    public int Count { get; set; } = 1;
    public int MaxDurability { get; set; } = 10;
    public int Durability { get; set; } = 10;

    protected Item(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public abstract Item Copy();

    public void FillFrom(Item source)
    {
        ViewPrefab = source.ViewPrefab;
        MaxDurability = source.MaxDurability;
        Durability = source.Durability;
        Count = 1;
    }
}
