using System.Collections.Generic;
using UnityEngine;

public enum ChestMaterial
{
    Wood,
    Glass,
    Steel
}

public class Chest : TileObject
{
    public const int BurnTurns = 3;

    public ChestMaterial Material { get; private set; }
    public Padlock Lock { get; private set; }
    public bool IsOpen { get; private set; }
    public bool IsBurning { get; private set; }
    public bool HasItems => contents.Count > 0;

    readonly List<Item> contents = new List<Item>();
    readonly List<Transform> lootMarks = new List<Transform>();
    int lootCount;
    int burnLeft;
    Transform body;
    Transform lid;

    public static ChestMaterial RollMaterial()
    {
        float roll = Random.value;
        if (roll < 0.1f)
            return ChestMaterial.Steel;
        if (roll < 0.2f)
            return ChestMaterial.Glass;
        return ChestMaterial.Wood;
    }

    public static Chest Create(TileInfo tile, ChestMaterial material, PadlockKind padlock)
    {
        GameObject root = new GameObject("Chest");
        Chest chest = root.AddComponent<Chest>();
        chest.Build(tile, material, padlock);
        return chest;
    }

    public void PrepareLoot(int count)
    {
        lootCount = count < 1 ? 1 : count;
    }

    public void Fill(ItemCatalog catalog)
    {
        while (contents.Count < lootCount)
        {
            Item item = NextLoot(catalog);
            if (item == null)
                break;
            contents.Add(item);
        }
    }

    Item NextLoot(ItemCatalog catalog)
    {
        if (Lock != null && Lock.Kind == PadlockKind.Gold)
            return ItemGenerator.GenerateRandom(catalog);
        if (Lock != null && Lock.Kind != PadlockKind.None)
            return ItemGenerator.GenerateAmong(catalog, ItemCatalog.DropCategory.Equipment);

        return ItemGenerator.GenerateAmong(catalog,
            ItemCatalog.DropCategory.Food,
            ItemCatalog.DropCategory.Scroll,
            ItemCatalog.DropCategory.Rune,
            ItemCatalog.DropCategory.Resource);
    }

    public void Open()
    {
        if (IsOpen || (Lock != null && Lock.BlocksPassage))
            return;

        IsOpen = true;
        ApplyPose();
    }

    public bool TryGoldKey()
    {
        return Lock != null && Lock.TryGoldKey();
    }

    public bool AcceptsLockpick()
    {
        return Lock != null && Lock.AcceptsLockpick();
    }

    public void SolvePuzzle()
    {
        if (Lock != null)
            Lock.Solve();
    }

    public bool Take(PlayerInventory inventory)
    {
        if (!IsOpen || inventory == null)
            return false;

        bool moved = false;
        for (int i = contents.Count - 1; i >= 0; i--)
        {
            if (!inventory.TryAdd(contents[i]))
                continue;
            contents.RemoveAt(i);
            moved = true;
        }

        ShowLoot();
        return moved;
    }

    public void Ignite()
    {
        if (Material != ChestMaterial.Wood || IsBurning)
            return;

        IsBurning = true;
        burnLeft = BurnTurns;
        Color fire = new Color(0.85f, 0.28f, 0.08f, 1f);
        if (body != null)
            Tint(body.GetComponent<Renderer>(), Tile, fire);
        if (lid != null)
            Tint(lid.GetComponent<Renderer>(), Tile, fire);
    }

    public void TickBurn()
    {
        if (!IsBurning)
            return;

        burnLeft--;
        if (burnLeft > 0)
            return;

        contents.Clear();
        if (Tile != null)
            Tile.isOccupied = false;
        Destroy(gameObject);
    }

    public string Description()
    {
        string text = MaterialName() + " skrzynia " + (IsOpen ? "jest otwarta." : "jest zamknięta.");
        if (IsBurning)
            text += " Płonie.";
        if (Lock != null && Lock.Kind != PadlockKind.None)
            text += " Wisi na niej " + Lock.Name() + " kłódka.";
        if ((IsOpen || Material == ChestMaterial.Glass) && contents.Count > 0)
        {
            text += " W środku:";
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i] == null)
                    continue;
                text += " " + contents[i].DisplayName + ".";
            }
        }

        return text;
    }

    void Build(TileInfo tile, ChestMaterial material, PadlockKind padlock)
    {
        Material = material;
        Lock = Padlock.Create(padlock, transform, null);
        body = Part("ChestBody");
        lid = Part("ChestLid");
        Tint(body.GetComponent<Renderer>(), tile, BodyColor(material));
        Tint(lid.GetComponent<Renderer>(), tile, BodyColor(material));
        Place(tile);
        transform.localRotation = Quaternion.Euler(0f, 225f, 0f);
        if (Tile != null)
            Tile.isOccupied = true;
        ApplyPose();
    }

    Transform Part(string partName)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
        part.transform.SetParent(transform, false);
        return part.transform;
    }

    void ApplyPose()
    {
        float size = Consts.TILE_SIZE;
        if (body != null)
        {
            body.localPosition = new Vector3(0f, size * 0.18f, 0f);
            body.localRotation = Quaternion.identity;
            body.localScale = new Vector3(size * 0.55f, size * 0.32f, size * 0.4f);
        }

        if (lid != null)
        {
            if (IsOpen)
            {
                lid.localPosition = new Vector3(0f, size * 0.4f, -size * 0.16f);
                lid.localRotation = Quaternion.Euler(-70f, 0f, 0f);
            }
            else
            {
                lid.localPosition = new Vector3(0f, size * 0.36f, 0f);
                lid.localRotation = Quaternion.identity;
            }

            lid.localScale = new Vector3(size * 0.58f, size * 0.06f, size * 0.42f);
        }

        if (Lock != null)
        {
            Lock.Fit(
                new Vector3(0f, size * 0.2f, size * 0.22f),
                Quaternion.identity,
                new Vector3(size * 0.12f, size * 0.12f, size * 0.06f));
        }

        ShowLoot();
    }

    void ShowLoot()
    {
        int shown = IsOpen ? contents.Count : 0;
        float size = Consts.TILE_SIZE;
        while (lootMarks.Count < shown)
        {
            Transform mark = Part("ChestLoot");
            Tint(mark.GetComponent<Renderer>(), Tile, new Color(0.95f, 0.85f, 0.25f, 1f));
            lootMarks.Add(mark);
        }

        float span = size * 0.14f;
        for (int i = 0; i < lootMarks.Count; i++)
        {
            bool visible = i < shown;
            lootMarks[i].gameObject.SetActive(visible);
            if (!visible)
                continue;

            float x = shown == 1 ? 0f : (i - (shown - 1) * 0.5f) * span;
            lootMarks[i].localPosition = new Vector3(x, size * 0.42f, size * 0.02f);
            lootMarks[i].localRotation = Quaternion.identity;
            lootMarks[i].localScale = Vector3.one * (size * 0.12f);
        }
    }

    string MaterialName()
    {
        switch (Material)
        {
            case ChestMaterial.Glass: return "Szklana";
            case ChestMaterial.Steel: return "Stalowa";
            default: return "Drewniana";
        }
    }

    static Color BodyColor(ChestMaterial material)
    {
        switch (material)
        {
            case ChestMaterial.Glass: return new Color(0.75f, 0.9f, 0.95f, 0.45f);
            case ChestMaterial.Steel: return new Color(0.62f, 0.66f, 0.7f, 1f);
            default: return new Color(0.45f, 0.28f, 0.12f, 1f);
        }
    }

    static void Tint(Renderer renderer, TileInfo tile, Color color)
    {
        if (renderer == null)
            return;

        MeshRenderer tileRenderer = tile != null ? tile.GetComponent<MeshRenderer>() : null;
        Material mat = tileRenderer != null && tileRenderer.sharedMaterial != null
            ? new Material(tileRenderer.sharedMaterial)
            : renderer.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        renderer.material = mat;
    }
}
