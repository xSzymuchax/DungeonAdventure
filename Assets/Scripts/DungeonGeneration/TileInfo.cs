using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public Position2D position;
    public FloorFieldType type;
    public bool isOccupied;
    public bool wasSeen;

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
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        Material mat = meshRenderer.material;
        int colorId = mat.HasProperty(BaseColorId) ? BaseColorId : ColorId;
        Color c = mat.GetColor(colorId);
        c.a = alpha;
        mat.SetColor(colorId, c);
    }
}
