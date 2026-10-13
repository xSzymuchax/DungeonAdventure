using System.Collections.Generic;
using UnityEngine;

// temporary class for displaying character portraits
// TODO: replace with a individual character portrait
public static class Portrait
{
    static readonly Dictionary<int, Sprite> solids = new();

    public static Sprite Of(Component body)
    {
        if (body == null)
            return null;

        Renderer renderer = body.GetComponentInChildren<Renderer>();
        if (renderer == null || renderer.sharedMaterial == null)
            return null;

        Material material = renderer.sharedMaterial;
        Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
        color.a = 1f;
        return Solid(color);
    }

    public static Sprite Solid(Color color)
    {
        int key = ((int)(color.r * 255f) << 16) | ((int)(color.g * 255f) << 8) | (int)(color.b * 255f);
        if (solids.TryGetValue(key, out Sprite sprite) && sprite != null)
            return sprite;

        Texture2D texture = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        solids[key] = sprite;
        return sprite;
    }
}
