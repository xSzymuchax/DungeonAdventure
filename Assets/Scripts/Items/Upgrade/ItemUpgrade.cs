using UnityEngine;

public static class ItemUpgrade
{
    public static float Factor(int level)
    {
        return 1f + 0.2f * Mathf.Clamp(level, 0, Consts.MAX_UPGRADE_LEVEL);
    }

    public static void Roll(EquipmentItem item)
    {
        if (item == null)
            return;

        int level = 0;
        while (level < Consts.MAX_UPGRADE_LEVEL && Random.value < Consts.UPGRADE_CHANCE)
            level++;
        item.Level = level;
    }
}
