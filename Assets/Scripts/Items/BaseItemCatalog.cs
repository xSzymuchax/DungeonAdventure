using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseItemCatalog", menuName = "Dungeon/Base Item Catalog")]
public class BaseItemCatalog : ScriptableObject
{
    [Range(0f, 1f)] public float consumableChance;
    public StatRollTable statRolls;
    [HideInInspector] public EquipmentTier[] equipment;
    [HideInInspector] public BaseItem[] consumables;

    [System.Serializable]
    public class EquipmentTier
    {
        public int tier = 1;
        public BaseItem[] items;
    }

    public struct DropChance
    {
        public BaseItem item;
        public int tier;
        public bool consumable;
        public float chance;
    }

    public BaseItem Roll()
    {
        bool hasEquipment = EquipmentWeight() > 0f;
        bool hasConsumable = CountConsumables() > 0;
        if (!hasEquipment && !hasConsumable)
            return null;

        bool consumable = hasConsumable && (!hasEquipment || Random.value < consumableChance);
        if (consumable)
        {
            BaseItem picked = PickConsumable();
            if (picked != null)
                return picked;
        }

        return PickEquipment();
    }

    public bool TryGet(string id, out BaseItem item)
    {
        if (equipment != null)
        {
            for (int group = 0; group < equipment.Length; group++)
            {
                EquipmentTier tier = equipment[group];
                if (tier == null || tier.items == null)
                    continue;
                for (int i = 0; i < tier.items.Length; i++)
                {
                    if (tier.items[i] != null && tier.items[i].id == id)
                    {
                        item = tier.items[i];
                        return true;
                    }
                }
            }
        }

        if (consumables != null)
        {
            for (int i = 0; i < consumables.Length; i++)
            {
                if (consumables[i] != null && consumables[i].id == id)
                {
                    item = consumables[i];
                    return true;
                }
            }
        }

        item = null;
        return false;
    }

    public void CopyDropChances(List<DropChance> results)
    {
        results.Clear();
        float equipmentWeight = EquipmentWeight();
        int consumableCount = CountConsumables();
        bool hasEquipment = equipmentWeight > 0f;
        bool hasConsumable = consumableCount > 0;
        float equipmentShare = 0f;
        float consumableShare = 0f;
        if (hasEquipment && hasConsumable)
        {
            consumableShare = Mathf.Clamp01(consumableChance);
            equipmentShare = 1f - consumableShare;
        }
        else if (hasEquipment)
        {
            equipmentShare = 1f;
        }
        else if (hasConsumable)
        {
            consumableShare = 1f;
        }

        if (hasEquipment)
        {
            for (int group = 0; group < equipment.Length; group++)
            {
                EquipmentTier tierGroup = equipment[group];
                if (tierGroup == null || tierGroup.items == null)
                    continue;
                int tier = Mathf.Max(1, tierGroup.tier);
                float weight = 1f / tier;
                for (int i = 0; i < tierGroup.items.Length; i++)
                {
                    if (tierGroup.items[i] == null)
                        continue;
                    results.Add(new DropChance
                    {
                        item = tierGroup.items[i],
                        tier = tier,
                        consumable = false,
                        chance = equipmentShare * weight / equipmentWeight
                    });
                }
            }
        }

        if (!hasConsumable)
            return;

        float each = consumableShare / consumableCount;
        for (int i = 0; i < consumables.Length; i++)
        {
            if (consumables[i] == null)
                continue;
            results.Add(new DropChance
            {
                item = consumables[i],
                tier = 0,
                consumable = true,
                chance = each
            });
        }
    }

    void OnValidate()
    {
        consumableChance = Mathf.Clamp01(consumableChance);
        if (equipment == null)
            return;

        for (int group = 0; group < equipment.Length; group++)
        {
            EquipmentTier tierGroup = equipment[group];
            if (tierGroup == null)
                continue;
            if (tierGroup.tier < 1)
                tierGroup.tier = 1;
            if (tierGroup.items == null)
                continue;

            for (int i = 0; i < tierGroup.items.Length; i++)
            {
                BaseItem item = tierGroup.items[i];
                if (item == null || item.tier == tierGroup.tier)
                    continue;
                item.tier = tierGroup.tier;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(item);
#endif
            }
        }
    }

    BaseItem PickEquipment()
    {
        float total = EquipmentWeight();
        if (total <= 0f || equipment == null)
            return null;

        float roll = Random.value * total;
        float cursor = 0f;
        BaseItem last = null;
        for (int group = 0; group < equipment.Length; group++)
        {
            EquipmentTier tierGroup = equipment[group];
            if (tierGroup == null || tierGroup.items == null)
                continue;
            float weight = 1f / Mathf.Max(1, tierGroup.tier);
            for (int i = 0; i < tierGroup.items.Length; i++)
            {
                if (tierGroup.items[i] == null)
                    continue;
                last = tierGroup.items[i];
                cursor += weight;
                if (roll <= cursor)
                    return last;
            }
        }

        return last;
    }

    BaseItem PickConsumable()
    {
        int count = CountConsumables();
        if (count == 0)
            return null;

        int roll = Random.Range(0, count);
        for (int i = 0; i < consumables.Length; i++)
        {
            if (consumables[i] == null)
                continue;
            if (roll == 0)
                return consumables[i];
            roll--;
        }

        return null;
    }

    float EquipmentWeight()
    {
        if (equipment == null)
            return 0f;

        float total = 0f;
        for (int group = 0; group < equipment.Length; group++)
        {
            EquipmentTier tierGroup = equipment[group];
            if (tierGroup == null || tierGroup.items == null)
                continue;
            float weight = 1f / Mathf.Max(1, tierGroup.tier);
            for (int i = 0; i < tierGroup.items.Length; i++)
            {
                if (tierGroup.items[i] != null)
                    total += weight;
            }
        }

        return total;
    }

    int CountConsumables()
    {
        if (consumables == null)
            return 0;

        int count = 0;
        for (int i = 0; i < consumables.Length; i++)
        {
            if (consumables[i] != null)
                count++;
        }

        return count;
    }
}
