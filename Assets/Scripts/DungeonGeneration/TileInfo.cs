using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public Position2D position;
    public FloorFieldType type;
    public bool isOccupied;
    public bool wasSeen;
    public TileObject tileObject;

    float shownAlpha = 1f;

    public void SetTileObject(TileObject occupant)
    {
        tileObject = occupant;
        PlaceHeldObject();
    }

    public void PlaceHeldObject()
    {
        if (tileObject == null)
            return;

        tileObject.transform.SetParent(transform, false);
        ApplyAlpha(tileObject.GetComponentsInChildren<Renderer>(true), shownAlpha);
    }

    public void ClearTileObject(TileObject occupant)
    {
        if (tileObject == occupant)
            tileObject = null;
    }

    private const float DimmedAlpha = 0.2f;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private MeshRenderer meshRenderer;

    public void Dim()
    {
        SetAlpha(DimmedAlpha);
    }

    public void Brighten()
    {
        SetAlpha(1f);
    }

    private void SetAlpha(float alpha)
    {
        shownAlpha = alpha;
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        ApplyAlpha(meshRenderer, alpha);
        if (tileObject != null)
            ApplyAlpha(tileObject.GetComponentsInChildren<Renderer>(true), alpha);
    }

    static void ApplyAlpha(Renderer[] renderers, float alpha)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
            ApplyAlpha(renderers[i], alpha);
    }

    static void ApplyAlpha(Renderer renderer, float alpha)
    {
        if (renderer == null)
            return;

        Material mat = renderer.material;
        int colorId = mat.HasProperty(BaseColorId) ? BaseColorId : ColorId;
        Color c = mat.GetColor(colorId);
        c.a = alpha;
        mat.SetColor(colorId, c);
    }
}
