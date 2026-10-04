public class StaffItem : EquipmentItem
{
    public StaffSpell Spell { get; set; }

    int level;

    public StaffItem(EquipmentSlot slot, string id, string displayName) : base(slot, id, displayName)
    {
        WeaponKind = WeaponKind.Staff;
    }

    public override int Level
    {
        get => level;
        set
        {
            level = value;
            if (Spell != null)
                Spell.SetExtraCharges(level);
        }
    }

    public override Item Copy()
    {
        StaffItem copy = new StaffItem(Slot, Id, DisplayName);
        CopyOnto(copy);
        copy.Spell = Spell != null ? Spell.Copy() : null;
        copy.Level = Level;
        return copy;
    }
}
