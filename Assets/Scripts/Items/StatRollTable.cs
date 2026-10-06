using UnityEngine;

[CreateAssetMenu(fileName = "StatRollTable", menuName = "Dungeon/Stat Roll Table")]
public class StatRollTable : ScriptableObject
{
    public StatRoll[] rolls;

    [System.Serializable]
    public class StatRoll
    {
        public StatId stat;
        public float min;
        public float max = 1f;
    }

    public bool TryPick(EquipmentItem item, out StatId stat, out float value)
    {
        stat = default;
        value = 0f;
        int available = CountAvailable(item);
        if (available == 0)
            return false;

        int roll = Random.Range(0, available);
        for (int i = 0; i < rolls.Length; i++)
        {
            StatRoll entry = rolls[i];
            if (!IsAvailable(item, entry))
                continue;
            if (roll > 0)
            {
                roll--;
                continue;
            }

            float min = entry.min;
            float max = entry.max;
            if (max < min)
            {
                float swap = min;
                min = max;
                max = swap;
            }

            stat = entry.stat;
            value = stat == StatId.WalkCost || stat == StatId.AttackCost
                ? Random.Range(min, max)
                : Random.Range(Mathf.RoundToInt(min), Mathf.RoundToInt(max) + 1);
            return true;
        }

        return false;
    }

    void OnValidate()
    {
        if (rolls == null)
            return;

        for (int i = 0; i < rolls.Length; i++)
        {
            StatRoll entry = rolls[i];
            if (entry == null || entry.max >= entry.min)
                continue;
            entry.max = entry.min;
        }
    }

    int CountAvailable(EquipmentItem item)
    {
        if (rolls == null)
            return 0;

        int count = 0;
        for (int i = 0; i < rolls.Length; i++)
        {
            if (IsAvailable(item, rolls[i]))
                count++;
        }

        return count;
    }

    static bool IsAvailable(EquipmentItem item, StatRoll entry)
    {
        if (entry == null)
            return false;
        if (entry.min == 0 && entry.max == 0)
            return false;
        if (!Fits(item, entry.stat))
            return false;
        return item == null || !HasStat(item, entry.stat);
    }

    static bool Fits(EquipmentItem item, StatId stat)
    {
        if (item == null || Excluded(stat))
            return item == null && !Excluded(stat);
        if (item.Slot == EquipmentSlot.Amulet)
            return true;
        if (stat == StatId.MaxHealth || stat == StatId.MaxMana || stat == StatId.Strength || stat == StatId.Knowledge || stat == StatId.CriticalChance)
            return true;
        if (item.Slot == EquipmentSlot.Weapon)
            return FitsWeapon(item.WeaponKind, stat);
        if (stat == StatId.HealthRegen || stat == StatId.ManaRegen || stat == StatId.Dodge || stat == StatId.WalkCost)
            return true;
        if (item.Slot == EquipmentSlot.Helmet)
            return stat == StatId.Defense || stat == StatId.ViewRange;
        if (item.Slot == EquipmentSlot.Armor)
            return stat == StatId.Defense;
        if (item.Slot == EquipmentSlot.Shield)
            return stat == StatId.Defense || stat == StatId.Block;
        return false;
    }

    static bool FitsWeapon(WeaponKind kind, StatId stat)
    {
        if (stat == StatId.Damage || stat == StatId.AttackCost || stat == StatId.CounterDodge)
            return true;
        if (kind == WeaponKind.Bow && stat == StatId.ArrowDamage)
            return true;
        if (kind == WeaponKind.Crossbow && stat == StatId.BoltDamage)
            return true;
        if (kind == WeaponKind.Blowgun && stat == StatId.DartDamage)
            return true;
        return kind == WeaponKind.Staff && stat == StatId.MagicAmplify;
    }

    static bool Excluded(StatId stat)
    {
        return stat == StatId.MaxSatiety || stat == StatId.MaxHydration || stat == StatId.MaxSanity
            || stat == StatId.SatietyBurn || stat == StatId.HydrationBurn || stat == StatId.SanityBurn;
    }

    static bool HasStat(EquipmentItem item, StatId stat)
    {
        for (int i = 0; i < item.Modifiers.Count; i++)
        {
            if (item.Modifiers[i] != null && item.Modifiers[i].stat == stat)
                return true;
        }

        return false;
    }
}
