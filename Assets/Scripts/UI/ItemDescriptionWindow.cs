using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionWindow : MonoBehaviour
{
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text description;
    [SerializeField] Image equipButton;
    [SerializeField] Image unequipButton;
    [SerializeField] Image useButton;
    [SerializeField] Image throwButton;
    [SerializeField] Image dropButton;
    [SerializeField] Image backButton;

    PlayerInventory inventory;
    bool fromBag;
    int bagIndex;
    EquipmentSlot equippedSlot;
    bool open;

    void Awake()
    {
        Wire(equipButton, Equip);
        Wire(unequipButton, Unequip);
        Wire(useButton, Use);
        Wire(throwButton, Throw);
        Wire(dropButton, Drop);
        Wire(backButton, Hide);
        gameObject.SetActive(false);
    }

    public void Bind(PlayerInventory inventory)
    {
        if (this.inventory != null)
            this.inventory.Changed -= Refresh;

        this.inventory = inventory;
        if (inventory != null)
            inventory.Changed += Refresh;
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.Changed -= Refresh;
    }

    public void ShowBag(int index)
    {
        if (open && fromBag && bagIndex == index)
        {
            Hide();
            return;
        }

        fromBag = true;
        bagIndex = index;
        open = true;
        gameObject.SetActive(true);
        Refresh();
    }

    public void ShowEquipped(EquipmentSlot slot)
    {
        if (open && !fromBag && equippedSlot == slot)
        {
            Hide();
            return;
        }

        fromBag = false;
        equippedSlot = slot;
        open = true;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        open = false;
        gameObject.SetActive(false);
    }

    void Refresh()
    {
        if (!open)
            return;

        Item item = Current();
        if (item == null)
        {
            Hide();
            return;
        }

        if (itemName != null)
            itemName.text = item.DisplayName;
        if (description != null)
            description.text = Describe(item);

        ShowButton(equipButton, CanEquip(item) && PlayerCanAct());
        ShowButton(unequipButton, CanUnequip(item) && PlayerCanAct());
        ShowButton(useButton, item.Kind == ItemKind.Consumable && fromBag && PlayerCanAct());
        ShowButton(throwButton, PlayerCanAct());
        ShowButton(dropButton, PlayerCanAct() && GameController.Instance != null && GameController.Instance.CanDropItem());
        ShowButton(backButton, true);
        if (equipButton != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(equipButton.transform.parent as RectTransform);
    }

    void Equip()
    {
        if (!PlayerCanAct() || !CanEquip(Current()))
            return;

        int index = bagIndex;
        PlayerCharacter player = GameController.Instance.Player;
        Hide();
        GameController.Instance.BeginPlayerAction(new EquipAction(player, index));
    }

    void Unequip()
    {
        if (!PlayerCanAct() || !CanUnequip(Current()))
            return;

        EquipmentSlot slot = equippedSlot;
        PlayerCharacter player = GameController.Instance.Player;
        Hide();
        GameController.Instance.BeginPlayerAction(new UnequipAction(player, slot));
    }

    void Use()
    {
        Item item = Current();
        if (!PlayerCanAct() || item == null || item.Kind != ItemKind.Consumable || !fromBag)
            return;

        int index = bagIndex;
        PlayerCharacter player = GameController.Instance.Player;
        Hide();
        GameController.Instance.BeginPlayerAction(new UseItemAction(player, index));
    }

    void Throw()
    {
        if (!PlayerCanAct() || Current() == null)
            return;

        if (fromBag)
            GameController.Instance.ArmThrowFromBag(bagIndex);
        else
            GameController.Instance.ArmThrowFromSlot(equippedSlot);
        Hide();
    }

    void Drop()
    {
        if (!PlayerCanAct() || Current() == null || !GameController.Instance.CanDropItem())
            return;

        PlayerCharacter player = GameController.Instance.Player;
        IAction action = fromBag
            ? new DropAction(player, bagIndex)
            : new DropAction(player, equippedSlot);
        Hide();
        GameController.Instance.BeginPlayerAction(action);
    }

    static bool PlayerCanAct()
    {
        return GameController.Instance != null
            && GameController.Instance.Player != null
            && GameController.Instance.Player.Energy > 0;
    }

    Item Current()
    {
        if (inventory == null)
            return null;

        if (fromBag)
            return bagIndex >= 0 && bagIndex < inventory.Bag.Count ? inventory.Bag[bagIndex] : null;

        return inventory.Equipped(equippedSlot);
    }

    bool CanEquip(Item item)
    {
        if (!fromBag || item is not EquipmentItem)
            return false;

        PlayerStats stats = GameController.Instance != null ? GameController.Instance.Player?.PlayerStats : null;
        return ItemRequirements.Met(item, stats);
    }

    bool CanUnequip(Item item)
    {
        return !fromBag
            && item is EquipmentItem
            && inventory != null
            && inventory.Bag.Count < PlayerInventory.Capacity;
    }

    static void Wire(Image image, UnityEngine.Events.UnityAction action)
    {
        if (image == null)
            return;

        Button button = image.GetComponent<Button>();
        if (button == null)
            button = image.gameObject.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.55f);
        button.colors = colors;
        button.onClick.AddListener(action);
    }

    static void ShowButton(Image image, bool visible)
    {
        if (image == null)
            return;

        image.gameObject.SetActive(visible);
    }

    static string Describe(Item item)
    {
        string text = "";
        for (int i = 0; i < item.Modifiers.Count; i++)
        {
            StatModifier modifier = item.Modifiers[i];
            if (modifier == null)
                continue;

            if (text.Length > 0)
                text += "\n";

            string sign = modifier.value >= 0 ? "+" : "";
            text += StatLabel(modifier.stat) + " " + sign + modifier.value.ToString("0.##");
        }

        for (int i = 0; i < item.Requirements.Count; i++)
        {
            StatRequirement requirement = item.Requirements[i];
            if (requirement == null)
                continue;

            if (text.Length > 0)
                text += "\n";

            text += "Wymaga: " + StatLabel(requirement.stat) + " " + requirement.value;
        }

        return text.Length == 0 ? "Brak bonusów." : text;
    }

    static string StatLabel(StatId stat)
    {
        switch (stat)
        {
            case StatId.Damage: return "Atak";
            case StatId.Defense: return "Obrona";
            case StatId.Block: return "Blok";
            case StatId.WalkCost: return "Koszt ruchu";
            case StatId.AttackCost: return "Koszt ataku";
            case StatId.Strength: return "Siła";
            case StatId.Knowledge: return "Wiedza";
            case StatId.MaxHealth: return "Zdrowie";
            case StatId.MaxMana: return "Mana";
            case StatId.ViewRange: return "Zasięg widzenia";
            case StatId.MaxSatiety: return "Sytość";
            case StatId.MaxHydration: return "Nawodnienie";
            case StatId.MaxSanity: return "Poczytalność";
            case StatId.HealthRegen: return "Regeneracja zdrowia";
            case StatId.ManaRegen: return "Regeneracja many";
            case StatId.SatietyBurn: return "Spalanie najedzenia";
            case StatId.HydrationBurn: return "Spalanie napicia";
            case StatId.SanityBurn: return "Spalanie poczytalności";
            default: return stat.ToString();
        }
    }
}
