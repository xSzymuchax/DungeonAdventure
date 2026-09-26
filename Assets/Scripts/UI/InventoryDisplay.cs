using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
    public ItemDisplayer[] bagSlots;
    public ItemDisplayer helmetSlot;
    public ItemDisplayer armorSlot;
    public ItemDisplayer amuletSlot;
    public ItemDisplayer weaponSlot;
    public ItemDisplayer shieldSlot;
    public ItemPreview previewPrefab;

    PlayerInventory inventory;
    GameObject itemModel;
    bool wired;

    public void Bind(PlayerInventory inventory, GameObject itemModel)
    {
        if (this.inventory != null)
            this.inventory.Changed -= Refresh;

        this.inventory = inventory;
        this.itemModel = itemModel;
        Wire();
        inventory.Changed += Refresh;
        Refresh();
    }

    void Update()
    {
        Each(slot => slot.Tick());
    }

    void OnDestroy()
    {
        Each(slot => slot.Release());
    }

    void Wire()
    {
        if (wired)
            return;

        wired = true;
        if (bagSlots != null)
        {
            for (int i = 0; i < bagSlots.Length; i++)
            {
                int index = i;
                Button button = ButtonOn(bagSlots[i]);
                if (button != null)
                    button.onClick.AddListener(() => inventory.TryEquipFromBag(index));
            }
        }

        WireEquipment(helmetSlot, EquipmentSlot.Helmet);
        WireEquipment(armorSlot, EquipmentSlot.Armor);
        WireEquipment(amuletSlot, EquipmentSlot.Amulet);
        WireEquipment(weaponSlot, EquipmentSlot.Sword);
        WireEquipment(shieldSlot, EquipmentSlot.Shield);
    }

    void WireEquipment(ItemDisplayer slot, EquipmentSlot equipment)
    {
        Button button = ButtonOn(slot);
        if (button != null)
            button.onClick.AddListener(() => inventory.TryUnequip(equipment));
    }

    static Button ButtonOn(ItemDisplayer slot)
    {
        if (slot == null || slot.icon == null)
            return null;

        Button button = slot.icon.GetComponent<Button>();
        if (button == null)
            button = slot.icon.gameObject.AddComponent<Button>();
        return button;
    }

    void Refresh()
    {
        if (inventory == null)
            return;

        if (bagSlots != null)
        {
            for (int i = 0; i < bagSlots.Length; i++)
            {
                Item item = i < inventory.Bag.Count ? inventory.Bag[i] : null;
                if (bagSlots[i] != null)
                    bagSlots[i].Show(item, true, itemModel, previewPrefab);
            }
        }

        ShowEquipment(helmetSlot, EquipmentSlot.Helmet);
        ShowEquipment(armorSlot, EquipmentSlot.Armor);
        ShowEquipment(amuletSlot, EquipmentSlot.Amulet);
        ShowEquipment(weaponSlot, EquipmentSlot.Sword);
        ShowEquipment(shieldSlot, EquipmentSlot.Shield);
    }

    void ShowEquipment(ItemDisplayer slot, EquipmentSlot equipment)
    {
        if (slot != null)
            slot.Show(inventory.Equipped(equipment), false, itemModel, previewPrefab);
    }

    void Each(Action<ItemDisplayer> action)
    {
        if (bagSlots != null)
        {
            for (int i = 0; i < bagSlots.Length; i++)
            {
                if (bagSlots[i] != null)
                    action(bagSlots[i]);
            }
        }

        if (helmetSlot != null)
            action(helmetSlot);
        if (armorSlot != null)
            action(armorSlot);
        if (amuletSlot != null)
            action(amuletSlot);
        if (weaponSlot != null)
            action(weaponSlot);
        if (shieldSlot != null)
            action(shieldSlot);
    }
}
