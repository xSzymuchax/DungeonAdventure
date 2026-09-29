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

    public bool TryPick(Item item, out StatId stat, out float value)
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

    int CountAvailable(Item item)
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

    static bool IsAvailable(Item item, StatRoll entry)
    {
        if (entry == null)
            return false;
        if (entry.min == 0 && entry.max == 0)
            return false;
        return item == null || !HasStat(item, entry.stat);
    }

    static bool HasStat(Item item, StatId stat)
    {
        for (int i = 0; i < item.Modifiers.Count; i++)
        {
            if (item.Modifiers[i] != null && item.Modifiers[i].stat == stat)
                return true;
        }

        return false;
    }
}
