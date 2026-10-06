using UnityEngine;

public static class ItemGenerator
{
    const float NextModifierChance = 0.2f;
    const int MaxModifiers = 3;

    public static Item GenerateRandom(ItemCatalog catalog)
    {
        if (catalog == null)
            return null;

        CatalogPick pick = catalog.RollPick();
        return Generate(pick, catalog.statRolls);
    }

    public static Item Generate(CatalogPick pick, StatRollTable rolls)
    {
        switch (pick.kind)
        {
            case CatalogPickKind.Equipment:
                return GenerateEquipment(pick.equipment, rolls, !pick.authored);
            case CatalogPickKind.Food:
                return GenerateFood(pick.food);
            case CatalogPickKind.Scroll:
                return GenerateScroll(pick.scroll);
            case CatalogPickKind.Rune:
                return GenerateRune(pick.rune);
            case CatalogPickKind.Resource:
                return GenerateResource(pick.resource);
            case CatalogPickKind.Ammunition:
                return GenerateAmmunition(pick.ammunition);
            default:
                return null;
        }
    }

    public static Item GenerateDefinition(ScriptableObject definition, StatRollTable rolls, bool authored)
    {
        if (definition is BaseItem equipment)
            return GenerateEquipment(equipment, rolls, !authored);
        if (definition is FoodDefinition food)
            return GenerateFood(food);
        if (definition is ScrollDefinition scroll)
            return GenerateScroll(scroll);
        if (definition is RuneDefinition rune)
            return GenerateRune(rune);
        if (definition is ResourceDefinition resource)
            return GenerateResource(resource);
        if (definition is AmmunitionDefinition ammunition)
            return GenerateAmmunition(ammunition);
        return null;
    }

    public static EquipmentItem GenerateEquipment(BaseItem definition, StatRollTable rolls, bool rollStats = true)
    {
        if (definition == null)
            return null;

        EquipmentItem item = definition.weaponKind == WeaponKind.Staff
            ? new StaffItem(definition.slot, definition.id, definition.displayName)
            : new EquipmentItem(definition.slot, definition.id, definition.displayName);
        item.ViewPrefab = definition.model;
        ApplyDurability(item, definition.maxDurability);
        CopyRequirements(item, definition);
        item.WeaponKind = definition.weaponKind;
        if (rollStats)
            RollModifiers(item, definition, rolls);
        else
            CopyFixed(item, definition);
        item.ThrownWeapon = definition.thrownWeapon;
        item.ThrowDamage = definition.throwDamage;
        item.Sharp = definition.sharp;
        item.Description = definition.description;
        if (item is StaffItem staff)
            RollStaffSpell(staff);
        if (rollStats)
            ItemUpgrade.Roll(item);
        return item;
    }

    public static Food GenerateFood(FoodDefinition definition)
    {
        if (definition == null)
            return null;

        Food item = new Food(definition.id, definition.displayName);
        ApplyFood(item, definition);
        return item;
    }

    public static Scroll GenerateScroll(ScrollDefinition definition)
    {
        if (definition == null)
            return null;

        Scroll item = new Scroll(definition.id, definition.displayName);
        ApplyScroll(item, definition);
        item.SpellLevel = RollSpellLevel(Skill.MaxLevel);
        return item;
    }

    public static RuneStone GenerateRune(RuneDefinition definition)
    {
        if (definition == null)
            return null;

        RuneStone item = new RuneStone(definition.id, definition.displayName);
        ApplyRune(item, definition);
        item.SetLevel(RollSpellLevel(RuneStone.MaxLevel));
        item.SpellLevel = RollSpellLevel(Skill.MaxLevel);
        item.Charge = item.MaxCharges;
        return item;
    }

    public static void RollStaffSpell(StaffItem item)
    {
        if (item == null || !TryFirebolt(out RuneDefinition definition))
            return;

        StaffSpell spell = new StaffSpell();
        ApplyStaffSpell(spell, definition);
        spell.SetRuneLevel(RollSpellLevel(RuneStone.MaxLevel));
        spell.SpellLevel = RollSpellLevel(Skill.MaxLevel);
        spell.Charge = spell.MaxCharges;
        item.Spell = spell;
    }

    public static void ApplyStaffSpell(StaffSpell spell, RuneDefinition definition)
    {
        if (spell == null || definition == null)
            return;

        spell.Effects.Clear();
        CopyEffects(spell.Effects, definition.effects);
        spell.SetBaseMaxCharges(definition.maxCharges);
    }

    static bool TryFirebolt(out RuneDefinition definition)
    {
        definition = null;
        ItemCatalog catalog = GameController.Instance != null ? GameController.Instance.baseItems : null;
        return catalog != null && catalog.TryGetRune("fireboltRune", out definition);
    }

    public static ResourceItem GenerateResource(ResourceDefinition definition)
    {
        if (definition == null)
            return null;

        ResourceItem item = new ResourceItem(definition.id, definition.displayName);
        ApplyResource(item, definition);
        return item;
    }

    public static void ApplyFood(Food item, FoodDefinition definition)
    {
        item.ViewPrefab = definition.model;
        item.Description = definition.description;
        item.Satiety = definition.satiety;
        item.Hydration = definition.hydration;
        item.Sanity = definition.sanity;
        item.TimeToEat = definition.timeToEat < 1 ? 1 : definition.timeToEat;
        item.Spoils = definition.spoils;
    }

    public static void ApplyScroll(Scroll item, ScrollDefinition definition)
    {
        item.ViewPrefab = definition.model;
        item.Description = definition.description;
        item.Effects.Clear();
        CopyEffects(item.Effects, definition.effects);
    }

    public static void ApplyRune(RuneStone item, RuneDefinition definition)
    {
        item.ViewPrefab = definition.model;
        item.Description = definition.description;
        item.SetBaseMaxCharges(definition.maxCharges);
        item.Charge = item.MaxCharges;
        item.ThrowDamage = definition.throwDamage;
        item.Effects.Clear();
        CopyEffects(item.Effects, definition.effects);
    }

    public static void ApplyResource(ResourceItem item, ResourceDefinition definition)
    {
        item.ViewPrefab = definition.model;
        item.Description = definition.description;
    }

    public static Ammunition GenerateAmmunition(AmmunitionDefinition definition)
    {
        if (definition == null)
            return null;

        Ammunition item = new Ammunition(definition.id, definition.displayName);
        ApplyAmmunition(item, definition);
        return item;
    }

    public static void ApplyAmmunition(Ammunition item, AmmunitionDefinition definition)
    {
        item.ViewPrefab = definition.model;
        item.Description = definition.description;
        item.Damage = definition.damage;
        item.Launcher = definition.launcher;
        ApplyDurability(item, definition.maxDurability);
    }

    static void ApplyDurability(Item item, int max)
    {
        item.MaxDurability = max < 1 ? Consts.DEFAULT_DURABILITY : max;
        item.Durability = item.MaxDurability;
    }

    public static void EnsureEffects(Item item)
    {
        if (item is Scroll scroll)
            Ensure(scroll.Effects, scroll.Id, true);
        else if (item is RuneStone rune)
            Ensure(rune.Effects, rune.Id, false);
        else if (item is StaffItem staff)
            EnsureStaffEffects(staff.Spell);
    }

    public static void EnsureStaffEffects(StaffSpell spell)
    {
        if (spell == null)
            return;

        Ensure(spell.Effects, "fireboltRune", false);
    }

    static void Ensure(System.Collections.Generic.List<Effect> target, string id, bool scroll)
    {
        if (target == null || HasLiveEffect(target))
            return;

        ItemCatalog catalog = GameController.Instance != null ? GameController.Instance.baseItems : null;
        if (catalog == null)
            return;

        if (scroll)
        {
            if (catalog.TryGetScroll(id, out ScrollDefinition scrollDefinition))
                CopyEffects(target, scrollDefinition.effects);
            return;
        }

        if (catalog.TryGetRune(id, out RuneDefinition runeDefinition))
            CopyEffects(target, runeDefinition.effects);
    }

    static bool HasLiveEffect(System.Collections.Generic.List<Effect> effects)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null)
                return true;
        }

        return false;
    }

    static int RollSpellLevel(int maxLevel)
    {
        int level = 1;
        while (level < maxLevel && UnityEngine.Random.value < Consts.UPGRADE_CHANCE)
            level++;
        return level;
    }

    static void CopyEffects(System.Collections.Generic.List<Effect> target, Effect[] effects)
    {
        if (effects == null)
            return;

        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] != null)
                target.Add(effects[i]);
        }
    }

    static void CopyRequirements(EquipmentItem item, BaseItem definition)
    {
        if (definition.requirements == null)
            return;

        for (int i = 0; i < definition.requirements.Length; i++)
        {
            StatRequirement requirement = definition.requirements[i];
            if (requirement == null)
                continue;
            item.Requirements.Add(new StatRequirement { stat = requirement.stat, value = requirement.value });
        }
    }

    static void RollModifiers(EquipmentItem item, BaseItem definition, StatRollTable rolls)
    {
        int fixedCount = CopyFixed(item, definition);
        int rolled = 0;
        if (fixedCount == 0)
        {
            if (!TryAddBonus(item, definition, rolls))
                return;
            rolled = 1;
        }

        while (rolled < MaxModifiers && Random.value < NextModifierChance)
        {
            if (!TryAddBonus(item, definition, rolls))
                return;
            rolled++;
        }
    }

    static int CopyFixed(EquipmentItem item, BaseItem definition)
    {
        if (definition.modifiers == null)
            return 0;

        int count = 0;
        for (int i = 0; i < definition.modifiers.Length; i++)
        {
            BaseModifier modifier = definition.modifiers[i];
            if (modifier == null)
                continue;
            item.Modifiers.Add(new StatModifier { stat = modifier.stat, value = modifier.value });
            count++;
        }

        return count;
    }

    static bool TryAddBonus(EquipmentItem item, BaseItem definition, StatRollTable rolls)
    {
        if (rolls == null || !rolls.TryPick(item, out StatId stat, out float value))
            return false;

        item.Modifiers.Add(new StatModifier { stat = stat, value = Scale(stat, value, definition.tier) });
        return true;
    }

    static float Scale(StatId stat, float value, int tier)
    {
        if (stat == StatId.Strength || stat == StatId.Knowledge || stat == StatId.WalkCost || stat == StatId.AttackCost || stat == StatId.ViewRange)
            return value;
        return value * Mathf.Max(1, tier);
    }
}
