using UnityEngine;

public enum PadlockKind
{
    None,
    Steel,
    Weak,
    Nasty,
    Temporary,
    Gold
}

public class Padlock
{
    public const int TemporaryTurns = 20;

    public PadlockKind Kind { get; private set; }
    public bool Engaged { get; private set; }
    public int TurnsLeft { get; private set; }
    public bool BlocksPassage => Kind != PadlockKind.None && Engaged;

    Transform visual;

    public static PadlockKind RollLocked()
    {
        return Random.value < 0.5f ? PadlockKind.Steel : PadlockKind.Weak;
    }

    public static PadlockKind RollChest()
    {
        float roll = Random.value;
        if (roll < 0.6f)
            return PadlockKind.None;
        if (roll < 0.95f)
            return PadlockKind.Weak;
        return PadlockKind.Gold;
    }

    public static Padlock Create(PadlockKind kind, Transform parent, TileInfo tile)
    {
        if (kind == PadlockKind.None || parent == null)
            return null;

        Padlock padlock = new Padlock();
        padlock.Kind = kind;
        padlock.Engaged = true;
        padlock.TurnsLeft = kind == PadlockKind.Temporary ? TemporaryTurns : 0;
        padlock.Build(parent, tile);
        return padlock;
    }

    public void Engage()
    {
        if (Kind == PadlockKind.None)
            return;

        Engaged = true;
    }

    public bool TrySteelKey()
    {
        if (Kind != PadlockKind.Steel || !Engaged)
            return false;

        Clear();
        return true;
    }

    public bool TryGoldKey()
    {
        if (Kind != PadlockKind.Gold || !Engaged)
            return false;

        Clear();
        return true;
    }

    public bool AcceptsLockpick()
    {
        return Engaged && (Kind == PadlockKind.Weak || Kind == PadlockKind.Nasty);
    }

    public void Solve()
    {
        if (Kind == PadlockKind.Weak)
            Clear();
        else if (Kind == PadlockKind.Nasty)
            Engaged = false;
    }

    public bool Tick()
    {
        if (Kind != PadlockKind.Temporary)
            return false;

        TurnsLeft--;
        if (TurnsLeft > 0)
            return false;

        Clear();
        return true;
    }

    public void Fit(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        if (visual == null)
            return;

        visual.gameObject.SetActive(Engaged);
        visual.localPosition = localPosition;
        visual.localRotation = localRotation;
        visual.localScale = localScale;
    }

    public string Name()
    {
        switch (Kind)
        {
            case PadlockKind.Steel: return "stalowa";
            case PadlockKind.Weak: return "słaba";
            case PadlockKind.Nasty: return "wredna";
            case PadlockKind.Temporary: return "tymczasowa";
            case PadlockKind.Gold: return "złota";
            default: return "";
        }
    }

    void Clear()
    {
        Kind = PadlockKind.None;
        Engaged = false;
        TurnsLeft = 0;
        if (visual != null)
            Object.Destroy(visual.gameObject);
        visual = null;
    }

    void Build(Transform parent, TileInfo tile)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Padlock";
        Collider bodyCollider = body.GetComponent<Collider>();
        if (bodyCollider != null)
            Object.Destroy(bodyCollider);

        visual = body.transform;
        visual.SetParent(parent, false);
        Tint(body.GetComponent<Renderer>(), tile, LockColor(Kind));
    }

    static Color LockColor(PadlockKind padlock)
    {
        switch (padlock)
        {
            case PadlockKind.Weak: return new Color(0.85f, 0.75f, 0.2f, 1f);
            case PadlockKind.Nasty: return new Color(0.55f, 0.2f, 0.65f, 1f);
            case PadlockKind.Temporary: return new Color(0.9f, 0.45f, 0.15f, 1f);
            case PadlockKind.Gold: return new Color(0.95f, 0.78f, 0.15f, 1f);
            default: return new Color(0.45f, 0.5f, 0.55f, 1f);
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
