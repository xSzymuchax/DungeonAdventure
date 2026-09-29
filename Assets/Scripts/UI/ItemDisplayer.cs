using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemDisplayer
{
    public Image icon;
    public Text label;
    public RawImage preview;

    Item shownItem;
    ItemPreview stage;
    bool keepSquare;
    bool squareFitted;

    public void Tick()
    {
        if (keepSquare && !squareFitted)
            FitSquare();
    }

    public void Show(Item item, bool bagSlot, GameObject itemModel, ItemPreview previewPrefab)
    {
        if (icon == null)
            return;

        if (item == null)
        {
            Clear();
            icon.sprite = null;
            icon.color = bagSlot ? Color.white : Color.black;
            if (label != null)
                label.text = "";
            return;
        }

        if (label != null)
            label.text = item.DisplayName;

        GameObject shownModel = item.ViewPrefab != null ? item.ViewPrefab : itemModel;
        if (shownModel == null || preview == null || previewPrefab == null)
            return;

        icon.sprite = null;
        icon.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        if (shownItem != item)
        {
            shownItem = item;
            if (stage == null)
            {
                stage = UnityEngine.Object.Instantiate(previewPrefab);
                preview.texture = stage.Texture;
            }

            stage.SetModel(shownModel, ItemColors.For(item), item.ViewPrefab == null);
        }

        preview.enabled = true;
        keepSquare = !bagSlot;
        if (bagSlot)
            squareFitted = true;
        else if (!squareFitted)
            FitSquare();
    }

    public void Clear()
    {
        shownItem = null;
        keepSquare = false;
        if (stage != null)
            stage.ClearModel();
        if (preview != null)
            preview.enabled = false;
    }

    public void Release()
    {
        Clear();
        if (stage != null)
            UnityEngine.Object.Destroy(stage.gameObject);
        stage = null;
        if (preview != null)
            preview.texture = null;
    }

    void FitSquare()
    {
        if (preview == null)
            return;

        RectTransform rect = preview.rectTransform;
        RectTransform parent = rect.parent as RectTransform;
        if (parent == null)
            return;

        float width = parent.rect.width;
        float height = parent.rect.height;
        if (width < 1f || height < 1f)
            return;

        float size = Mathf.Min(width, height);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(size, size);
        squareFitted = true;
    }
}
