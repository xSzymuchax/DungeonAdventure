using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickBar : MonoBehaviour
{
    const float AssignHold = 1f;

    [SerializeField] ItemDisplayer[] slots;

    static readonly Color Highlight = new Color(1f, 0.85f, 0.4f, 1f);

    Item[] assigned;
    int picking = -1;
    int aiming = -1;
    int holding = -1;
    float holdStart;
    bool suppressClick;
    PlayerInventory inventory;
    InventoryDisplay inventoryDisplay;
    GameObject itemModel;
    ToggleCard panel;
    bool wired;

    public void Bind(PlayerInventory inventory, InventoryDisplay display, GameObject itemModel)
    {
        if (this.inventory != null)
            this.inventory.Changed -= Refresh;

        this.inventory = inventory;
        inventoryDisplay = display;
        this.itemModel = itemModel;
        panel = display != null ? display.GetComponent<ToggleCard>() : null;
        if (slots == null)
            assigned = null;
        else if (assigned == null || assigned.Length != slots.Length)
            assigned = new Item[slots.Length];

        Wire();
        if (inventory != null)
            inventory.Changed += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.Changed -= Refresh;
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].Release();
        }
    }

    void Update()
    {
        if (holding < 0)
            return;
        if (Time.unscaledTime - holdStart < AssignHold)
            return;

        int index = holding;
        holding = -1;
        suppressClick = true;
        BeginPick(index);
    }

    void LateUpdate()
    {
        if (aiming < 0 || StillAiming())
            return;

        aiming = -1;
        Refresh();
    }

    public void WaitAction()
    {
        PlayerCharacter player = GameController.Instance.Player;
        if (player == null)
            return;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null)
            return;

        controller.StartCoroutine(controller.PlayerWait());
    }

    public void PressDown(int index)
    {
        holding = index;
        holdStart = Time.unscaledTime;
        suppressClick = false;
    }

    public void PressUp(int index, bool stillOnSlot)
    {
        bool finished = stillOnSlot && holding == index && Time.unscaledTime - holdStart >= AssignHold;
        if (holding == index)
            holding = -1;
        if (!stillOnSlot || suppressClick || finished)
        {
            suppressClick = false;
            if (finished)
                BeginPick(index);
            return;
        }

        ShortClick(index);
    }

    public void PressLeave(int index)
    {
        if (holding != index)
            return;

        holding = -1;
        suppressClick = true;
    }

    void Wire()
    {
        if (wired || slots == null)
            return;

        wired = true;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || slots[i].icon == null)
                continue;

            int index = i;
            QuickSlotPress press = slots[i].icon.GetComponent<QuickSlotPress>();
            if (press == null)
                press = slots[i].icon.gameObject.AddComponent<QuickSlotPress>();
            press.bar = this;
            press.index = index;
        }
    }

    void ShortClick(int index)
    {
        if (assigned == null || index < 0 || index >= assigned.Length)
            return;

        if (picking == index)
        {
            CancelPick();
            return;
        }

        Item live = Live(assigned[index]);
        if (live == null)
            return;

        if (picking >= 0)
            CancelPick();

        assigned[index] = live;
        Activate(index, live);
    }

    void BeginPick(int index)
    {
        if (assigned == null || index < 0 || index >= assigned.Length)
            return;

        if (picking == index)
        {
            CancelPick();
            return;
        }

        picking = index;
        if (inventoryDisplay != null)
            inventoryDisplay.BeginAssign(item => Place(index, item));
        if (panel != null)
            panel.Toggle();
        Refresh();
    }

    void CancelPick()
    {
        picking = -1;
        if (inventoryDisplay != null)
            inventoryDisplay.EndAssign();
        Refresh();
    }

    void Place(int index, Item item)
    {
        picking = -1;
        if (assigned == null || index < 0 || index >= assigned.Length || item == null)
            return;

        assigned[index] = item;
        if (panel != null)
            panel.Toggle();
        Refresh();
    }

    void Activate(int index, Item item)
    {
        if (!PlayerCanAct() || inventory == null || !TryFind(item, out bool inBag, out int bagIndex, out EquipmentSlot slot))
            return;

        PlayerCharacter player = GameController.Instance.Player;
        if (OffersUse(item, inBag))
        {
            if (ItemUse.NeedsTarget(item))
            {
                if (item is StaffItem)
                    GameController.Instance.ArmItemUse(slot);
                else
                    GameController.Instance.ArmItemUse(bagIndex);
                MarkAiming(index);
                return;
            }

            if (item is Food)
                GameController.Instance.BeginPlayerAction(new UseFoodAction(player, bagIndex));
            else if (item is Scroll)
                GameController.Instance.BeginPlayerAction(new UseScrollAction(player, bagIndex, player.Position));
            else if (item is RuneStone)
                GameController.Instance.BeginPlayerAction(new UseRuneAction(player, bagIndex, player.Position));
            else if (item is StaffItem)
                GameController.Instance.BeginPlayerAction(new UseStaffAction(player, slot, player.Position));
            return;
        }

        if (inBag)
            GameController.Instance.ArmThrowFromBag(bagIndex);
        else
            GameController.Instance.ArmThrowFromSlot(slot);
        MarkAiming(index);
    }

    void MarkAiming(int index)
    {
        aiming = index;
        Refresh();
    }

    static bool StillAiming()
    {
        return GameController.Instance != null && GameController.Instance.IsAiming;
    }

    static bool OffersUse(Item item, bool inBag)
    {
        if (item == null)
            return false;
        if (item is StaffItem staff)
            return !inBag && staff.Spell != null;
        return inBag && (item is Food || item is Scroll || item is RuneStone);
    }

    static bool PlayerCanAct()
    {
        return GameController.Instance != null
            && GameController.Instance.Player != null
            && GameController.Instance.Player.Energy > 0;
    }

    Item Live(Item item)
    {
        if (item == null || inventory == null)
            return null;
        if (TryFind(item, out _, out _, out _))
            return item;

        return FindSame(item.Id);
    }

    Item FindSame(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        for (int i = 0; i < inventory.Bag.Count; i++)
        {
            Item bagItem = inventory.Bag[i];
            if (bagItem != null && bagItem.Id == id)
                return bagItem;
        }

        EquipmentSlot[] equipped = { EquipmentSlot.Helmet, EquipmentSlot.Armor, EquipmentSlot.Amulet, EquipmentSlot.Weapon, EquipmentSlot.Shield };
        for (int i = 0; i < equipped.Length; i++)
        {
            Item worn = inventory.Equipped(equipped[i]);
            if (worn != null && worn.Id == id)
                return worn;
        }

        return null;
    }

    bool TryFind(Item item, out bool inBag, out int bagIndex, out EquipmentSlot slot)
    {
        inBag = false;
        bagIndex = -1;
        slot = EquipmentSlot.Weapon;
        if (item == null || inventory == null)
            return false;

        for (int i = 0; i < inventory.Bag.Count; i++)
        {
            if (!ReferenceEquals(inventory.Bag[i], item))
                continue;

            inBag = true;
            bagIndex = i;
            return true;
        }

        EquipmentSlot[] equipped = { EquipmentSlot.Helmet, EquipmentSlot.Armor, EquipmentSlot.Amulet, EquipmentSlot.Weapon, EquipmentSlot.Shield };
        for (int i = 0; i < equipped.Length; i++)
        {
            if (!ReferenceEquals(inventory.Equipped(equipped[i]), item))
                continue;

            slot = equipped[i];
            return true;
        }

        return false;
    }

    void Refresh()
    {
        if (slots == null || assigned == null)
            return;

        ItemPreview preview = inventoryDisplay != null ? inventoryDisplay.previewPrefab : null;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || i >= assigned.Length)
                continue;

            Item ghost = assigned[i];
            Item live = Live(ghost);
            bool available = live != null;
            if (available)
                assigned[i] = live;

            Item shown = available ? live : ghost;
            bool lit = i == picking || i == aiming;
            slots[i].Show(shown, true, itemModel, preview);
            if (slots[i].preview != null)
            {
                if (lit && available)
                    slots[i].preview.color = Highlight;
                else if (!available && shown != null)
                    slots[i].preview.color = new Color(0.22f, 0.22f, 0.22f, 1f);
                else
                    slots[i].preview.color = Color.white;
            }
            if (!available && shown != null)
            {
                Mute(slots[i].label);
                Mute(slots[i].level);
                Mute(slots[i].detail);
            }

            if (lit && slots[i].icon != null)
                slots[i].icon.color = Highlight;
        }
    }

    static void Mute(Text text)
    {
        if (text == null)
            return;

        text.text = "";
        text.gameObject.SetActive(false);
    }
}

public class QuickSlotPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public QuickBar bar;
    public int index;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (bar != null)
            bar.PressDown(index);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (bar != null)
            bar.PressUp(index, StillOnSlot(eventData));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bar == null || StillOnSlot(eventData))
            return;

        bar.PressLeave(index);
    }

    bool StillOnSlot(PointerEventData eventData)
    {
        GameObject under = eventData.pointerCurrentRaycast.gameObject;
        return under == gameObject || (under != null && under.transform.IsChildOf(transform));
    }
}
