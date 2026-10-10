using System.Collections.Generic;

public class StatsProvider : IStatsProvider
{
    readonly CharacterStats stats;
    readonly PlayerInventory inventory;
    readonly EquipmentItem instead;

    public StatsProvider(CharacterStats stats)
    {
        this.stats = stats;
    }

    public StatsProvider(CharacterStats stats, PlayerInventory inventory, EquipmentItem instead = null)
    {
        this.stats = stats;
        this.inventory = inventory;
        this.instead = instead;
    }

    public CharacterStats GetStats()
    {
        if (stats == null)
            return null;

        if (stats is PlayerStats player)
            player.Apply(Sum());
        else
            stats.Recalculate();

        return stats;
    }

    public static float Total(EquipmentItem equipment, StatId stat)
    {
        float sum = 0f;
        foreach (StatModifier modifier in Scaled(equipment))
        {
            if (modifier.stat == stat)
                sum += modifier.value;
        }
        return sum;
    }

    StatTotals Sum()
    {
        StatTotals totals = new StatTotals();
        if (inventory != null)
        {
            foreach (EquipmentItem equipment in inventory.EquippedGear())
            {
                if (instead != null && equipment.Slot == instead.Slot)
                    continue;
                Add(totals, equipment);
            }
        }

        Add(totals, instead);
        return totals;
    }

    static void Add(StatTotals totals, EquipmentItem equipment)
    {
        foreach (StatModifier modifier in Scaled(equipment))
            totals.Add(modifier.stat, modifier.value);
    }

    static IEnumerable<StatModifier> Scaled(EquipmentItem equipment)
    {
        if (equipment == null)
            yield break;

        float scale = ItemUpgrade.Factor(equipment.Level);
        for (int i = 0; i < equipment.Modifiers.Count; i++)
        {
            StatModifier modifier = equipment.Modifiers[i];
            if (modifier == null)
                continue;
            yield return new StatModifier { stat = modifier.stat, value = modifier.value * scale };
        }
    }
}

public class StatTotals
{
    readonly Dictionary<StatId, float> values = new Dictionary<StatId, float>();

    public float Get(StatId stat)
    {
        values.TryGetValue(stat, out float value);
        return value;
    }

    public void Add(StatId stat, float value)
    {
        values.TryGetValue(stat, out float current);
        values[stat] = current + value;
    }
}
