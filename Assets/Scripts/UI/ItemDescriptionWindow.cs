using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionWindow : MonoBehaviour
{
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text descriptionDisplay;
    [SerializeField] Image equipButton;
    [SerializeField] Image unequipButton;
    [SerializeField] Image useButton;
    [SerializeField] Image repairButton;
    [SerializeField] Image throwButton;
    [SerializeField] Image dropButton;
    [SerializeField] Image backButton;

    PlayerInventory inventory;
    ToggleCard sheet;
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

    public void Bind(PlayerInventory inventory, ToggleCard sheet)
    {
        if (this.inventory != null)
            this.inventory.Changed -= Refresh;

        this.inventory = inventory;
        this.sheet = sheet;
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
        if (descriptionDisplay != null)
            descriptionDisplay.text = Describe(item, inventory);

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
        if (item is StaffItem)
            return !fromBag;
        return fromBag;
    }

    void Use()
    {
        Item item = Current();
        if (!PlayerCanAct() || !CanUseHere(item))
            return;

        int index = bagIndex;
        PlayerCharacter player = GameController.Instance.Player;
        Hide();
        FoldSheet();
        if (ItemUse.NeedsTarget(item))
        {
            if (item is StaffItem)
                GameController.Instance.ArmItemUse(equippedSlot);
            else
                GameController.Instance.ArmItemUse(index);
            return;
        }

        if (item is Food)
            GameController.Instance.BeginPlayerAction(new UseFoodAction(player, index));
        else if (item is Scroll)
            GameController.Instance.BeginPlayerAction(new UseScrollAction(player, index, player.Position));
        else if (item is RuneStone)
            GameController.Instance.BeginPlayerAction(new UseRuneAction(player, index, player.Position));
        else if (item is StaffItem)
            GameController.Instance.BeginPlayerAction(new UseStaffAction(player, equippedSlot, player.Position));
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
        FoldSheet();
    }

    void FoldSheet()
    {
        if (sheet != null)
            sheet.Toggle();
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

    const string MetColor = "#2E7D32";
    const string UnmetColor = "#C62828";
    const string BonusColor = "#1565C0";
    const string BodyColor = "#000000";
    const string ManaColor = "#F9A825";

    static string Describe(Item item, PlayerInventory inventory)
    {
        string requirements = "";
        string bonuses = "";
        if (item is EquipmentItem equipment)
        {
            requirements = Requirements(equipment);
            bonuses = Bonuses(equipment);
        }

        string body = "";
        string strike = "";
        string charges = "";
        if (item is StaffItem staff)
            DescribeStaff(staff, inventory, ref body, ref strike, ref charges);
        else if (item is EquipmentItem weapon && weapon.Slot == EquipmentSlot.Weapon)
            body = Append(body, StrikeLine(weapon, inventory, ItemText.Damage));

        if (item is Ammunition ammo)
            body = Append(body, AmmunitionShot(ammo, inventory));
        if (item is Food food)
            body = Append(body, FoodBody(food));
        if (item is RuneStone rune)
            DescribeRune(rune, inventory, ref body, ref charges);
        if (item is Scroll scroll)
            body = Append(body, ScrollBody(scroll, inventory));
        if (item is not StaffItem && item is not RuneStone && item is not Scroll)
            body = Append(body, LooseEffects(item));

        string prose = item.Description != null ? item.Description.Trim() : "";
        if (prose.Length > 0)
            prose = Paint(prose, BodyColor);
        if (body.Length > 0)
            body = Paint(body, BodyColor);
        if (strike.Length > 0)
            strike = Paint(strike, BodyColor);
        if (charges.Length > 0)
            charges = Paint(charges, BodyColor);
        string durability = Durability(item);
        if (durability.Length > 0)
            durability = Paint(durability, BodyColor);

        string text = requirements;
        text = Section(text, bonuses);
        text = Section(text, prose);
        text = Section(text, body);
        text = Section(text, strike);
        text = Section(text, charges);
        text = Section(text, durability);
        return text.Length == 0 ? Paint(ItemText.Empty, BodyColor) : text;
    }

    static string Requirements(EquipmentItem equipment)
    {
        PlayerStats stats = LiveStats();
        string text = "";
        for (int i = 0; i < equipment.Requirements.Count; i++)
        {
            StatRequirement requirement = equipment.Requirements[i];
            if (requirement == null)
                continue;
            bool met = stats != null && ItemRequirements.Value(stats, requirement.stat) >= requirement.value;
            string line = ItemText.Requirement(StatLabel.Name(requirement.stat), requirement.value);
            text = Append(text, Paint(line, met ? MetColor : UnmetColor));
        }
        return text;
    }

    static string Bonuses(EquipmentItem equipment)
    {
        string text = "";
        float scale = ItemUpgrade.Factor(equipment.Level);
        for (int i = 0; i < equipment.Modifiers.Count; i++)
        {
            StatModifier modifier = equipment.Modifiers[i];
            if (modifier == null)
                continue;
            float shown = modifier.value * scale;
            text = Append(text, Paint(ItemText.Bonus(StatLabel.Name(modifier.stat, shown), shown), BonusColor));
        }
        return text;
    }

    static void DescribeStaff(StaffItem staff, PlayerInventory inventory, ref string body, ref string strike, ref string charges)
    {
        strike = StrikeLine(staff, inventory, ItemText.Strike);
        if (staff.Spell == null)
            return;

        body = Append(body, ItemText.HoldsRune(staff.Spell.RuneLevel, SpellName(staff), staff.Spell.SpellLevel, PaintedMana(staff)));
        body = Append(body, SpellStrength(staff, staff.Spell.SpellLevel, inventory));
        charges = ChargeBlock(staff.Spell.Charge, staff.Spell.MaxCharges, staff.Spell.RechargePerTurn);
    }

    static void DescribeRune(RuneStone rune, PlayerInventory inventory, ref string body, ref string charges)
    {
        body = Append(body, ItemText.HoldsRune(rune.Level, SpellName(rune), rune.SpellLevel, PaintedMana(rune)));
        body = Append(body, SpellStrength(rune, rune.SpellLevel, inventory));
        charges = ChargeBlock(rune.Charge, rune.MaxCharges, rune.RechargePerTurn);
    }

    static string ScrollBody(Scroll scroll, PlayerInventory inventory)
    {
        string text = ItemText.HoldsScroll(SpellName(scroll), scroll.SpellLevel, PaintedMana(scroll));
        return Append(text, SpellStrength(scroll, scroll.SpellLevel, inventory));
    }

    static string FoodBody(Food food)
    {
        string text = "";
        if (food.Spoils)
            text = Append(text, FoodLabel.Line(food));
        float quality = food.Quality;
        if (food.Satiety != 0)
            text = Append(text, ItemText.Satiety(food.Satiety * quality));
        if (food.Hydration != 0)
            text = Append(text, ItemText.Hydration(food.Hydration * quality));
        if (food.Sanity != 0)
            text = Append(text, ItemText.Sanity(food.Sanity * quality));
        return text;
    }

    static string AmmunitionShot(Ammunition ammo, PlayerInventory inventory)
    {
        int shot = ammo.Damage + ItemProjectile.LauncherBonus(inventory, ammo);
        return ItemText.Amount(ItemText.Damage, Paint(shot.ToString(), ManaColor));
    }

    static string Durability(Item item)
    {
        if (item is Ammunition ammo)
            return ItemText.Durability(ammo.Durability, ammo.TotalMax);
        if (item.TracksDurability)
            return ItemText.Durability(item.Durability, item.MaxDurability);
        return "";
    }

    static string ChargeBlock(float charge, int max, float recharge)
    {
        string text = ItemText.Charge(charge, max);
        return Append(text, ItemText.Recharge(recharge));
    }

    static string LooseEffects(Item item)
    {
        string text = "";
        float mana = SpellMana.Cost(item);
        if (mana > 0f)
            text = Append(text, ItemText.Mana(mana));
        System.Collections.Generic.IList<Effect> effects = ItemUse.EffectsOf(item);
        if (effects == null)
            return text;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null)
                text = Append(text, effects[i].Label);
        }
        return text;
    }

    static string PaintedMana(Item item)
    {
        return Paint(SpellMana.Cost(item).ToString("0.##"), ManaColor);
    }

    static string Section(string text, string block)
    {
        if (block.Length == 0)
            return text;
        if (text.Length == 0)
            return block;
        return text + "\n\n" + block;
    }

    static string StrikeLine(EquipmentItem weapon, PlayerInventory inventory, string label)
    {
        int attack = PreviewRounded(Live(stats => stats.Damage, 0), inventory, weapon, StatId.Damage);
        return RangeLine(label, attack);
    }

    static string SpellStrength(Item item, int spellLevel, PlayerInventory inventory)
    {
        AttackSkill skill = CastSkill(item);
        if (skill == null)
            return "";

        PlayerStats stats = LiveStats();
        int knowledge = stats != null ? stats.Knowledge : 0;
        float amplify = stats != null ? stats.MagicAmplify : 0f;
        int attack = stats != null ? stats.Damage : 0;
        if (item is EquipmentItem weapon)
        {
            knowledge = PreviewRounded(knowledge, inventory, weapon, StatId.Knowledge);
            amplify = PreviewAmplify(stats, inventory, weapon);
            attack = PreviewRounded(attack, inventory, weapon, StatId.Damage);
        }

        float power = skill.Power(spellLevel, knowledge);
        float amount = DamageCalculator.AttackBase(skill.HitDamageType, power, attack, amplify);
        return RangeLine(ItemText.SpellPower, amount);
    }

    static string RangeLine(string label, float amount)
    {
        int min = DamageCalculator.MinPotential(amount);
        int max = DamageCalculator.MaxPotential(amount);
        return ItemText.Amount(label, Paint(min + "–" + max, ManaColor));
    }

    static AttackSkill CastSkill(Item item)
    {
        System.Collections.Generic.IList<Effect> effects = ItemUse.EffectsOf(item);
        if (effects == null)
            return null;

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] is SkillEffect effect && effect.skill is AttackSkill skill)
                return skill;
        }

        return null;
    }

    static int PreviewRounded(int current, PlayerInventory inventory, EquipmentItem weapon, StatId stat)
    {
        float now = Bonus(inventory, stat);
        float next = now - Scaled(EquippedWeapon(inventory), stat) + Scaled(weapon, stat);
        return current - Mathf.RoundToInt(now) + Mathf.RoundToInt(next);
    }

    static float PreviewAmplify(PlayerStats stats, PlayerInventory inventory, EquipmentItem weapon)
    {
        float current = stats != null ? stats.MagicAmplify : 0f;
        float now = Bonus(inventory, StatId.MagicAmplify);
        float next = now - Scaled(EquippedWeapon(inventory), StatId.MagicAmplify) + Scaled(weapon, StatId.MagicAmplify);
        return Mathf.Max(0f, current - now + next);
    }

    static float Bonus(PlayerInventory inventory, StatId stat)
    {
        return inventory != null ? StatBonus.Sum(inventory.Modifiers(), stat) : 0f;
    }

    static float Scaled(EquipmentItem item, StatId stat)
    {
        if (item == null)
            return 0f;
        return StatBonus.Sum(item.Modifiers, stat) * ItemUpgrade.Factor(item.Level);
    }

    static EquipmentItem EquippedWeapon(PlayerInventory inventory)
    {
        Item equipped = inventory != null ? inventory.Equipped(EquipmentSlot.Weapon) : null;
        return equipped as EquipmentItem;
    }

    static int Live(System.Func<PlayerStats, int> read, int fallback)
    {
        PlayerStats stats = LiveStats();
        return stats != null ? read(stats) : fallback;
    }

    static PlayerStats LiveStats()
    {
        return GameController.Instance != null ? GameController.Instance.Player?.PlayerStats : null;
    }

    static string SpellName(Item item)
    {
        System.Collections.Generic.IList<Effect> effects = ItemUse.EffectsOf(item);
        if (effects == null)
            return ItemText.Spell;
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] is SkillEffect spell)
                return string.IsNullOrEmpty(spell.Label) ? ItemText.Spell : spell.Label;
        }
        return ItemText.Spell;
    }

    static string Paint(string line, string color)
    {
        return "<color=" + color + ">" + line + "</color>";
    }

    static string Append(string text, string line)
    {
        if (line.Length == 0)
            return text;
        if (text.Length > 0)
            text += "\n";
        return text + line;
    }
}
