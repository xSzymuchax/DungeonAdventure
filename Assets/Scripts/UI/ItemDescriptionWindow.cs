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
    [SerializeField] Image repairButton;
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
        Wire(repairButton, Repair);
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
        ShowButton(useButton, CanUseHere(item) && PlayerCanAct());
        ShowButton(repairButton, ItemUse.CanRepair(item) && PlayerCanAct());
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

    bool CanUseHere(Item item)
    {
        if (!ItemUse.CanUse(item) || !SpellMana.CanPay(GameController.Instance != null ? GameController.Instance.Player : null, item))
            return false;
        if (fromBag)
            return true;
        return item is StaffItem;
    }

    void Use()
    {
        Item item = Current();
        if (!PlayerCanAct() || !CanUseHere(item))
            return;

        int index = bagIndex;
        PlayerCharacter player = GameController.Instance.Player;
        Hide();
        if (ItemUse.NeedsTarget(item))
        {
            if (fromBag)
                GameController.Instance.ArmItemUse(index);
            else
                GameController.Instance.ArmItemUse(equippedSlot);
            return;
        }

        if (item is Food)
            GameController.Instance.BeginPlayerAction(new UseFoodAction(player, index));
        else if (item is Scroll)
            GameController.Instance.BeginPlayerAction(new UseScrollAction(player, index, player.Position));
        else if (item is RuneStone)
            GameController.Instance.BeginPlayerAction(new UseRuneAction(player, index, player.Position));
        else if (item is StaffItem)
            GameController.Instance.BeginPlayerAction(fromBag
                ? new UseStaffAction(player, index, player.Position)
                : new UseStaffAction(player, equippedSlot, player.Position));
    }

    void Repair()
    {
        Item item = Current();
        if (!PlayerCanAct() || !ItemUse.CanRepair(item))
            return;

        PlayerCharacter player = GameController.Instance.Player;
        IAction action = fromBag
            ? new RepairAction(player, bagIndex)
            : new RepairAction(player, equippedSlot);
        Hide();
        GameController.Instance.BeginPlayerAction(action);
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
        if (!fromBag || item is not EquipmentItem equipment)
            return false;

        PlayerStats stats = GameController.Instance != null ? GameController.Instance.Player?.PlayerStats : null;
        return ItemRequirements.Met(equipment, stats);
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
        if (item is Ammunition ammo)
            text = Append(text, "Trwałość " + ammo.Durability + "/" + ammo.TotalMax);
        else if (item.TracksDurability)
            text = Append(text, "Trwałość " + item.Durability + "/" + item.MaxDurability);
        if (item is EquipmentItem equipment)
        {
            float scale = ItemUpgrade.Factor(equipment.Level);
            for (int i = 0; i < equipment.Modifiers.Count; i++)
            {
                StatModifier modifier = equipment.Modifiers[i];
                if (modifier == null)
                    continue;
                float shown = modifier.value * scale;
                string sign = shown >= 0 ? "+" : "";
                text = Append(text, StatLabel(modifier.stat, shown) + " " + sign + shown.ToString("0.##"));
            }

            for (int i = 0; i < equipment.Requirements.Count; i++)
            {
                StatRequirement requirement = equipment.Requirements[i];
                if (requirement == null)
                    continue;
                text = Append(text, "Wymaga: " + StatLabel(requirement.stat) + " " + requirement.value);
            }

            if (equipment is StaffItem staffItem && staffItem.Spell != null)
            {
                text = Append(text, "Poziom runy " + staffItem.Spell.RuneLevel);
                text = Append(text, "Ładunek " + staffItem.Spell.Charge.ToString("0.0") + "/" + staffItem.Spell.MaxCharges);
                text = Append(text, "Regeneracja " + staffItem.Spell.RechargePerTurn.ToString("0.##"));
            }
        }

        if (item is Food food)
        {
            if (food.Spoils)
                text = Append(text, FreshnessLabel(food));
            float quality = food.Quality;
            if (food.Satiety != 0)
                text = Append(text, "Najedzenie " + Signed(food.Satiety * quality));
            if (food.Hydration != 0)
                text = Append(text, "Nawodnienie " + Signed(food.Hydration * quality));
            if (food.Sanity != 0)
                text = Append(text, "Poczytalność " + Signed(food.Sanity * quality));
        }

        if (item is RuneStone rune)
        {
            text = Append(text, "Poziom runy " + rune.Level);
            text = Append(text, "Ładunek " + rune.Charge.ToString("0.0") + "/" + rune.MaxCharges);
            text = Append(text, "Regeneracja " + rune.RechargePerTurn.ToString("0.##"));
        }

        System.Collections.Generic.IList<Effect> effects = ItemUse.EffectsOf(item);
        float mana = SpellMana.Cost(item);
        if (mana > 0f)
            text = Append(text, "Mana " + mana.ToString("0.##"));
        if (effects != null)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] == null)
                    continue;
                string line = effects[i].Label;
                if (effects[i] is SkillEffect spell && spell.skill != null)
                {
                    int level = item is Scroll scroll ? scroll.SpellLevel
                        : item is RuneStone runeStone ? runeStone.SpellLevel
                        : item is StaffItem casting && casting.Spell != null ? casting.Spell.SpellLevel
                        : spell.skill.Level;
                    line += ", poziom " + level;
                }
                text = Append(text, line);
            }
        }

        return text.Length == 0 ? "Brak bonusów." : text;
    }

    static string FreshnessLabel(Food food)
    {
        string stage;
        switch (food.Freshness)
        {
            case FoodFreshness.Fresh: stage = "Świeże"; break;
            case FoodFreshness.Stale: stage = "Nieświeże"; break;
            case FoodFreshness.Spoiled: stage = "Zepsute"; break;
            default: stage = "Zgnite"; break;
        }

        if (food.Freshness == FoodFreshness.Rotten)
            return stage;
        return stage + ", zostało " + food.TurnsUntilNextStage + " tur";
    }

    static string Signed(float value)
    {
        return (value >= 0 ? "+" : "") + value.ToString("0.##");
    }

    static string Append(string text, string line)
    {
        if (text.Length > 0)
            text += "\n";
        return text + line;
    }

    static string StatLabel(StatId stat)
    {
        return StatLabel(stat, 0f);
    }

    static string StatLabel(StatId stat, float value)
    {
        if (value < 0f)
        {
            switch (stat)
            {
                case StatId.FireResistance: return "Wrażliwość na ogień";
                case StatId.ColdResistance: return "Wrażliwość na zimno";
                case StatId.PoisonResistance: return "Wrażliwość na truciznę";
                case StatId.ElectricityResistance: return "Wrażliwość na elektryczność";
            }
        }

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
            case StatId.ArrowDamage: return "Obrażenia strzał";
            case StatId.BoltDamage: return "Obrażenia bełtów";
            case StatId.DartDamage: return "Obrażenia rzutek";
            case StatId.MagicResistance: return "Odporność magiczna";
            case StatId.FireResistance: return "Odporność na ogień";
            case StatId.ColdResistance: return "Odporność na zimno";
            case StatId.PoisonResistance: return "Odporność na truciznę";
            case StatId.ElectricityResistance: return "Odporność na elektryczność";
            case StatId.Dodge: return "Unik";
            case StatId.CounterDodge: return "Kontra uniku";
            case StatId.MagicAmplify: return "Wzmocnienie magii";
            default: return stat.ToString();
        }
    }
}
