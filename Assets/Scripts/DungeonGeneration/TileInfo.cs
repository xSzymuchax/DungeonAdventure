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

    public bool BlocksVision()
    {
        if (type == FloorFieldType.SIDE_FIELD)
            return true;

        return tileObject is Door door && door.Material != DoorMaterial.Glass && !door.IsOpen;
    }

    public string Title()
    {
        if (tileObject is Door)
            return "Drzwi";

        switch (type)
        {
            case FloorFieldType.BASE_FIELD: return "Posadzka";
            case FloorFieldType.SIDE_FIELD: return "Ściana";
            case FloorFieldType.POSSIBLE_DOOR_FIELD: return "Próg";
            case FloorFieldType.CORRIDOR_FIELD: return "Korytarz";
            case FloorFieldType.DOOR_FIELD: return "Drzwi";
            case FloorFieldType.SPAWN_FIELD: return "Wejście";
            case FloorFieldType.EXIT_FIELD: return "Wyjście";
            default: return "Pusto";
        }
    }

    public string Description(IReadOnlyList<Item> items)
    {
        string text = tileObject is Door door
            ? door.Description()
            : FieldText();
        if (items == null || items.Count == 0)
            return text;

        text += "\n";
        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            if (item == null)
                continue;

            text += "\n" + item.DisplayName;
            if (!string.IsNullOrEmpty(item.Description))
                text += "\n" + item.Description;
        }

        return text;
    }

    public Sprite Picture()
    {
        return Portrait.Of(this);
    }

    string FieldText()
    {
        switch (type)
        {
            case FloorFieldType.BASE_FIELD:
                return "Kamienna podłoga komnaty.";
            case FloorFieldType.SIDE_FIELD:
                return "Ściana. Nie da się na nią wejść.";
            case FloorFieldType.POSSIBLE_DOOR_FIELD:
                return "Próg między komnatą a korytarzem.";
            case FloorFieldType.CORRIDOR_FIELD:
                return "Wąskie przejście między komnatami.";
            case FloorFieldType.DOOR_FIELD:
                return "Drzwi zamykające przejście.";
            case FloorFieldType.SPAWN_FIELD:
                return "Pole, na którym zaczyna się zejście.";
            case FloorFieldType.EXIT_FIELD:
                return "Zejście na niższy poziom.";
            default:
                return "Tu nie ma podłogi.";
        }
    }

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
