using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ItemDisplayer
{
    public Image icon;
    public Text label;
    public Text level;
    public Text detail;
    public RawImage preview;

    Item shownItem;
    ItemPreview stage;

    public void Show(Item item, bool bagSlot, GameObject itemModel, ItemPreview previewPrefab)
    {
        if (icon == null)
            return;

        if (item == null)
        {
            Clear();
            icon.sprite = null;
            icon.color = bagSlot ? Color.white : Color.black;
            Hide(label);
            Hide(level);
            Hide(detail);
            return;
        }

        ShowCount(item);
        ShowLevel(item);
        ShowDetail(item);

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
    }

    public void Clear()
    {
        shownItem = null;
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

    void ShowCount(Item item)
    {
        if (item is RuneStone rune)
        {
            SetText(label, rune.Charge.ToString("0.0") + "/" + rune.MaxCharges);
            return;
        }

        if (item is StaffItem staff && staff.Spell != null)
        {
            SetText(label, staff.Spell.Charge.ToString("0.0") + "/" + staff.Spell.MaxCharges);
            return;
        }

        if (item.Count > 1)
        {
            SetText(label, item.Count.ToString());
            return;
        }

        Hide(label);
    }

    void ShowLevel(Item item)
    {
        if (item is EquipmentItem equipment && equipment.Level > 0)
            SetText(level, "Lv. " + equipment.Level);
        else
            Hide(level);
    }

    void ShowDetail(Item item)
    {
        if (item is Ammunition ammo)
            SetText(detail, ammo.Durability + "/" + ammo.TotalMax);
        else if (item.TracksDurability)
            SetText(detail, item.Durability + "/" + item.MaxDurability);
        else
            Hide(detail);
    }

    static void Hide(Text text)
    {
        if (text == null)
            return;

        text.text = "";
        text.gameObject.SetActive(false);
    }

    static void SetText(Text text, string value)
    {
        if (text == null)
            return;

        text.gameObject.SetActive(true);
        text.text = value;
    }
}
