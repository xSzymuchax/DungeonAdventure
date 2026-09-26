using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnSystem
{
    readonly Transform itemHolder;
    readonly GameObject model;
    Dungeon dungeon;

    public ItemSpawnSystem(Transform itemHolder, GameObject model)
    {
        this.itemHolder = itemHolder;
        this.model = model;
    }

    public void Bind(Dungeon dungeon)
    {
        this.dungeon = dungeon;
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

            Place(ItemGenerator.GenerateRandom(), tile);
        }
    }

    public void SpawnSaved(ItemSave[] items)
    {
        if (dungeon == null || items == null)
            return;

        for (int i = 0; i < items.Length; i++)
        {
            ItemSave save = items[i];
            if (save == null || !dungeon.TileExists(save.x, save.y) || dungeon.HasGroundItem(new Position2D { x = save.x, y = save.y }))
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
        if (model == null)
        {
            Debug.Log("cant place item " + item.DisplayName);
            return;
        }

        GameObject view = Object.Instantiate(model, itemHolder);
        view.name = item.DisplayName;
        float size = 2f;
        view.transform.localScale = Vector3.one * size;
        view.transform.rotation = Quaternion.identity;
        view.transform.position = dungeon.GetTileWorldPosition(position) + Vector3.up * (size * 0.5f);

        Renderer renderer = view.GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.material.color = ItemColors.For(item);

        dungeon.AddGroundItem(item, position, view);
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
            case EquipmentSlot.Sword: return new Color(0.9f, 0.8f, 0.25f);
            case EquipmentSlot.Shield: return new Color(0.3f, 0.7f, 0.4f);
            default: return Color.white;
        }
    }
}
