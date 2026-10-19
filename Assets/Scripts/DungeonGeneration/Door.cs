using UnityEngine;

public enum DoorMaterial
{
    Wood,
    Glass,
    Steel
}

public class Door : TileObject
{
    public const int BurnTurns = 3;

    public DoorMaterial Material { get; private set; }
    public Padlock Lock { get; private set; }
    public bool IsOpen { get; private set; }
    public bool IsBurning { get; private set; }
    int burnLeft;
    public bool BlocksPassage => Lock != null && Lock.BlocksPassage;

    Transform leaf;

    public static DoorMaterial RollMaterial()
    {
        float roll = Random.value;
        if (roll < 0.1f)
            return DoorMaterial.Steel;
        if (roll < 0.2f)
            return DoorMaterial.Glass;
        return DoorMaterial.Wood;
    }

    public static Door Create(TileInfo tile, DoorMaterial material, PadlockKind padlock)
    {
        GameObject root = new GameObject("Door");
        Door door = root.AddComponent<Door>();
        door.Build(tile, material, padlock);
        return door;
    }

    void Build(TileInfo tile, DoorMaterial material, PadlockKind padlock)
    {
        Material = material;
        Lock = Padlock.Create(padlock, transform, null);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "DoorLeaf";
        Collider bodyCollider = body.GetComponent<Collider>();
        if (bodyCollider != null)
            Destroy(bodyCollider);

        leaf = body.transform;
        leaf.SetParent(transform, false);
        Tint(body.GetComponent<Renderer>(), tile, LeafColor(material));
        Place(tile);
        Close();
    }

    public void Open()
    {
        if (BlocksPassage)
            return;

        IsOpen = true;
        ApplyPose();
    }

    public void Close()
    {
        IsOpen = false;
        ApplyPose();
    }

    public void Engage()
    {
        if (Lock == null || Lock.Kind == PadlockKind.None)
            return;

        Lock.Engage();
        Close();
    }

    public bool TrySteelKey()
    {
        return Lock != null && Lock.TrySteelKey();
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

    public bool TickTemporary()
    {
        return Lock != null && Lock.Tick();
    }

    public void Ignite()
    {
        if (Material != DoorMaterial.Wood || IsBurning)
            return;

        IsBurning = true;
        burnLeft = BurnTurns;
        if (leaf != null)
            Tint(leaf.GetComponent<Renderer>(), Tile, new Color(0.85f, 0.28f, 0.08f, 1f));
    }

    public void TickBurn()
    {
        if (!IsBurning)
            return;

        burnLeft--;
        if (burnLeft > 0)
            return;

        Destroy(gameObject);
    }

    public string Description()
    {
        string text = MaterialName() + " drzwi " + (IsOpen ? "są otwarte." : "są zamknięte.");
        if (IsBurning)
            text += " Płoną.";
        if (Lock == null || Lock.Kind == PadlockKind.None)
            return text;

        return text + " Wisi na nich " + Lock.Name() + " kłódka.";
    }

    void ApplyPose()
    {
        if (leaf == null)
            return;

        float size = Consts.TILE_SIZE;
        if (IsOpen)
        {
            leaf.localPosition = new Vector3(size * 0.35f, size * 0.06f, 0f);
            leaf.localRotation = Quaternion.Euler(0f, 0f, 90f);
            leaf.localScale = new Vector3(size * 0.9f, size * 0.12f, size * 0.15f);
        }
        else
        {
            leaf.localPosition = new Vector3(0f, size * 0.4f, 0f);
            leaf.localRotation = Quaternion.identity;
            leaf.localScale = new Vector3(size * 0.15f, size * 0.8f, size * 0.9f);
        }

        if (Lock == null)
            return;

        Lock.Fit(
            new Vector3(0f, size * 0.55f, size * 0.2f),
            Quaternion.identity,
            new Vector3(size * 0.18f, size * 0.18f, size * 0.08f));
    }

    string MaterialName()
    {
        switch (Material)
        {
            case DoorMaterial.Glass: return "Szklane";
            case DoorMaterial.Steel: return "Stalowe";
            default: return "Drewniane";
        }
    }

    static Color LeafColor(DoorMaterial material)
    {
        switch (material)
        {
            case DoorMaterial.Glass: return new Color(0.75f, 0.9f, 0.95f, 0.45f);
            case DoorMaterial.Steel: return new Color(0.62f, 0.66f, 0.7f, 1f);
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
