using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnSystem
{
    public const float ViewSize = 2f;

    readonly Transform itemHolder;
    readonly GameObject model;
    readonly ItemCatalog catalog;
    Dungeon dungeon;

    public ItemSpawnSystem(Transform itemHolder, GameObject model, ItemCatalog catalog)
    {
        this.itemHolder = itemHolder;
        this.model = model;
        this.catalog = catalog;
    }

    public void Bind(Dungeon dungeon)
    {
        this.dungeon = dungeon;
    }

    public void SpawnAll(Position2D reserved)
    {
        if (dungeon == null || catalog == null)
            return;

        List<Item> items = new();
        CollectCatalog(items);
        FloorFieldType[,] fields = dungeon.GetFieldTypes();
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        Position2D player = dungeon.GetSpawnPoint();
        int index = 0;
        for (int y = 0; y < height && index < items.Count; y++)
        {
            for (int x = 0; x < width && index < items.Count; x++)
            {
                if (fields[x, y] != FloorFieldType.BASE_FIELD)
                    continue;
                if (x == player.x && y == player.y)
                    continue;
                if (x == reserved.x && y == reserved.y)
                    continue;

                Place(items[index], new Position2D { x = x, y = y });
                index++;
            }
        }
    }

    void CollectCatalog(List<Item> items)
    {
        if (catalog.equipment != null)
        {
            for (int group = 0; group < catalog.equipment.Length; group++)
            {
                ItemCatalog.EquipmentTier tier = catalog.equipment[group];
                if (tier == null || tier.items == null)
                    continue;
                for (int i = 0; i < tier.items.Length; i++)
                {
                    Item item = ItemGenerator.GenerateEquipment(tier.items[i], catalog.statRolls);
                    if (item != null)
                        items.Add(item);
                }
            }
        }

        AddFood(items);
        AddScrolls(items);
        AddRunes(items);
        AddResources(items);
        AddAmmunition(items);
        AddUniques(items);
    }

    void AddFood(List<Item> items)
    {
        if (catalog.foods == null)
            return;
        for (int i = 0; i < catalog.foods.Length; i++)
        {
            Item item = ItemGenerator.GenerateFood(catalog.foods[i]);
            if (item != null)
                items.Add(item);
        }
    }

    void AddScrolls(List<Item> items)
    {
        if (catalog.scrolls == null)
            return;
        for (int i = 0; i < catalog.scrolls.Length; i++)
        {
            Item item = ItemGenerator.GenerateScroll(catalog.scrolls[i]);
            if (item != null)
                items.Add(item);
        }
    }

    void AddRunes(List<Item> items)
    {
        if (catalog.runes == null)
            return;
        for (int i = 0; i < catalog.runes.Length; i++)
        {
            Item item = ItemGenerator.GenerateRune(catalog.runes[i]);
            if (item != null)
                items.Add(item);
        }
    }

    void AddResources(List<Item> items)
    {
        if (catalog.resources == null)
            return;
        for (int i = 0; i < catalog.resources.Length; i++)
        {
            Item item = ItemGenerator.GenerateResource(catalog.resources[i]);
            if (item != null)
                items.Add(item);
        }
    }

    void AddAmmunition(List<Item> items)
    {
        if (catalog.ammunitions == null)
            return;
        for (int i = 0; i < catalog.ammunitions.Length; i++)
        {
            Item item = ItemGenerator.GenerateAmmunition(catalog.ammunitions[i]);
            if (item != null)
                items.Add(item);
        }
    }

    void AddUniques(List<Item> items)
    {
        if (catalog.uniques == null)
            return;
        for (int i = 0; i < catalog.uniques.Length; i++)
        {
            Item item = ItemGenerator.GenerateDefinition(catalog.uniques[i], catalog.statRolls, true);
            if (item != null)
                items.Add(item);
        }
    }

    public void SpawnInitial()
    {
        if (dungeon == null)
            return;

        int count = dungeon.RoomCount / 2;
        for (int i = 0; i < count; i++)
        {
            if (!dungeon.TryGetRandomFreeBaseField(out Position2D tile) || dungeon.HasGroundItem(tile))
            {
                if (!TryFindEmptyBase(out tile))
                    break;
            }

            Item item = ItemGenerator.GenerateRandom(catalog);
            if (item == null)
                continue;
            Place(item, tile);
        }
    }

    public void SpawnSaved(ItemSave[] items)
    {
        if (dungeon == null || items == null)
            return;

        for (int i = 0; i < items.Length; i++)
        {
            ItemSave save = items[i];
            if (save == null || !dungeon.TileExists(save.x, save.y))
                continue;

            Item item = ItemFactory.Create(save);
            if (item == null)
                continue;

            Place(item, new Position2D { x = save.x, y = save.y });
        }
    }

    public void Clear()
    {
        if (dungeon != null)
            dungeon.ClearGroundItems();
    }

    void Place(Item item, Position2D position)
    {
        if (ViewSource(item) == null)
        {
            Debug.Log("cant place item " + item.DisplayName);
            return;
        }

        dungeon.AddGroundItem(item, position, CreateView(item, position));
    }

    public bool PlaceExisting(Item item, Position2D position)
    {
        if (item == null || dungeon == null || ViewSource(item) == null)
            return false;

        Place(item, position);
        return dungeon.HasGroundItem(position);
    }

    public GameObject CreateView(Item item, Position2D position)
    {
        GameObject source = ViewSource(item);
        if (item == null || source == null || dungeon == null)
            return null;

        GameObject view = Object.Instantiate(source, itemHolder);
        view.name = item.DisplayName;
        view.transform.localScale = Vector3.one * ViewSize;
        view.transform.rotation = Quaternion.identity;
        view.transform.position = dungeon.GetTileWorldPosition(position) + Vector3.up * (ViewSize * 0.5f);

        if (item.ViewPrefab == null)
        {
            Renderer renderer = view.GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.material.color = ItemColors.For(item);
        }

        return view;
    }

    GameObject ViewSource(Item item)
    {
        if (item != null && item.ViewPrefab != null)
            return item.ViewPrefab;
        return model;
    }

    bool TryFindEmptyBase(out Position2D position)
    {
        List<Position2D> free = new();
        FloorFieldType[,] fields = dungeon.GetFieldTypes();
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Position2D tile = new() { x = x, y = y };
                if (fields[x, y] != FloorFieldType.BASE_FIELD)
                    continue;
                if (dungeon.GetTileInfos()[x, y] != null && dungeon.GetTileInfos()[x, y].isOccupied)
                    continue;
                if (dungeon.HasGroundItem(tile))
                    continue;
                free.Add(tile);
            }
        }

        if (free.Count == 0)
        {
            position = default;
            return false;
        }

        position = free[Random.Range(0, free.Count)];
        return true;
    }
}

public static class ItemColors
{
    public static Color For(Item item)
    {
        if (item is not EquipmentItem equipment)
            return Color.white;

        switch (equipment.Slot)
        {
            case EquipmentSlot.Helmet: return new Color(0.75f, 0.75f, 0.8f);
            case EquipmentSlot.Armor: return new Color(0.55f, 0.35f, 0.2f);
            case EquipmentSlot.Amulet: return new Color(0.35f, 0.55f, 0.95f);
            case EquipmentSlot.Weapon: return new Color(0.9f, 0.8f, 0.25f);
            case EquipmentSlot.Shield: return new Color(0.3f, 0.7f, 0.4f);
            default: return Color.white;
        }
    }
}
