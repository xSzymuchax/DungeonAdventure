using System.Collections.Generic;

public enum EquipmentSlot
{
    Helmet,
    Armor,
    Amulet,
    Weapon,
    Shield
}

public enum WeaponKind
{
    None,
    Bow,
    Crossbow,
    Blowgun
}

public class EquipmentItem : Item
{
    public override ItemKind Kind => ItemKind.Equipment;
    public EquipmentSlot Slot { get; }
    public List<StatModifier> Modifiers { get; } = new();
    public List<StatRequirement> Requirements { get; } = new();
    public bool ThrownWeapon { get; set; }
    public int ThrowDamage { get; set; }
    public bool Sharp { get; set; }
    public WeaponKind WeaponKind { get; set; }
    public int Level { get; set; }

    public EquipmentItem(EquipmentSlot slot, string id, string displayName) : base(id, displayName)
    {
        Slot = slot;
    }

    public override Item Copy()
    {
        EquipmentItem copy = new EquipmentItem(Slot, Id, DisplayName);
        copy.FillFrom(this);
        copy.ThrownWeapon = ThrownWeapon;
        copy.ThrowDamage = ThrowDamage;
        copy.Sharp = Sharp;
        copy.WeaponKind = WeaponKind;
        copy.Level = Level;
        for (int i = 0; i < Modifiers.Count; i++)
        {
            StatModifier modifier = Modifiers[i];
            if (modifier != null)
                copy.Modifiers.Add(new StatModifier { stat = modifier.stat, value = modifier.value });
        }

        for (int i = 0; i < Requirements.Count; i++)
        {
            StatRequirement requirement = Requirements[i];
            if (requirement != null)
                copy.Requirements.Add(new StatRequirement { stat = requirement.stat, value = requirement.value });
        }

        return copy;
    }
}
