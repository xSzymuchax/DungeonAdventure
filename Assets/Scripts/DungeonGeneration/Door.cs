using UnityEngine;

//TODO - change graphics to something more appropriate
public class Door : TileObject
{
    public bool IsOpen { get; private set; }

    Transform leaf;

    public static Door Create(TileInfo tile)
    {
        GameObject root = new GameObject("Door");
        Door door = root.AddComponent<Door>();
        door.Build(tile);
        return door;
    }

    void Build(TileInfo tile)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "DoorLeaf";
        Collider bodyCollider = body.GetComponent<Collider>();
        if (bodyCollider != null)
            Destroy(bodyCollider);

        leaf = body.transform;
        leaf.SetParent(transform, false);
        Tint(body.GetComponent<Renderer>(), tile);
        Place(tile);
        Close();
    }

    public void Open()
    {
        IsOpen = true;
        ApplyPose();
    }

    public void Close()
    {
        IsOpen = false;
        ApplyPose();
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
            return;
        }

        leaf.localPosition = new Vector3(0f, size * 0.4f, 0f);
        leaf.localRotation = Quaternion.identity;
        leaf.localScale = new Vector3(size * 0.15f, size * 0.8f, size * 0.9f);
    }

    static void Tint(Renderer renderer, TileInfo tile)
    {
        if (renderer == null)
            return;

        MeshRenderer tileRenderer = tile != null ? tile.GetComponent<MeshRenderer>() : null;
        Material mat = tileRenderer != null && tileRenderer.sharedMaterial != null
            ? new Material(tileRenderer.sharedMaterial)
            : renderer.material;
        Color color = new Color(0.45f, 0.28f, 0.12f, 1f);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        renderer.material = mat;
    }
}
