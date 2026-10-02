using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ItemCatalog", menuName = "Dungeon/Item Catalog")]
public class ItemCatalog : ScriptableObject
{
    [FormerlySerializedAs("consumableChance")]
    [Range(0f, 1f)] public float restChance;
    [Range(0f, 1f)] public float foodChance = 0.5f;
    public StatRollTable statRolls;
    [HideInInspector] public EquipmentTier[] equipment;
    [HideInInspector] public FoodDefinition[] foods;
    [HideInInspector] public ScrollDefinition[] scrolls;
    [HideInInspector] public RuneDefinition[] runes;
    [HideInInspector] public ResourceDefinition[] resources;
    [HideInInspector] public AmmunitionDefinition[] ammunitions;

    [System.Serializable]
    public class EquipmentTier
    {
        public int tier = 1;
        public BaseItem[] items;
    }

    public enum DropGroup
    {
        Equipment,
        Food,
        Other
    }

    public struct DropChance
    {
        public string label;
        public int tier;
        public DropGroup group;
        public float chance;
    }

    public CatalogPick RollPick()
    {
        bool hasEquipment = EquipmentWeight() > 0f;
        bool hasFood = Count(foods) > 0;
        bool hasOther = OtherCount() > 0;
        bool hasRest = hasFood || hasOther;
        if (!hasEquipment && !hasRest)
            return default;

        bool rest = hasRest && (!hasEquipment || Random.value < restChance);
        if (rest)
        {
            CatalogPick picked = PickRest(hasFood, hasOther);
            if (picked.kind != CatalogPickKind.None)
                return picked;
        }

        BaseItem pickedEquipment = PickEquipment();
        return pickedEquipment == null
            ? default
            : new CatalogPick { kind = CatalogPickKind.Equipment, equipment = pickedEquipment };
    }

    public bool TryGetEquipment(string id, out BaseItem item)
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

        item = null;
        return false;
    }

    public bool TryGetFood(string id, out FoodDefinition item)
    {
        return TryFind(foods, id, out item);
    }

    public bool TryGetScroll(string id, out ScrollDefinition item)
    {
        return TryFind(scrolls, id, out item);
    }

    public bool TryGetRune(string id, out RuneDefinition item)
    {
        return TryFind(runes, id, out item);
    }

    public bool TryGetResource(string id, out ResourceDefinition item)
    {
        return TryFind(resources, id, out item);
    }

    public bool TryGetAmmunition(string id, out AmmunitionDefinition item)
    {
        return TryFind(ammunitions, id, out item);
    }

    public void CopyDropChances(List<DropChance> results)
    {
        results.Clear();
        float equipmentWeight = EquipmentWeight();
        bool hasEquipment = equipmentWeight > 0f;
        bool hasFood = Count(foods) > 0;
        bool hasOther = OtherCount() > 0;
        bool hasRest = hasFood || hasOther;
        float equipmentShare = 0f;
        float restShare = 0f;
        if (hasEquipment && hasRest)
        {
            restShare = Mathf.Clamp01(restChance);
            equipmentShare = 1f - restShare;
        }
        else if (hasEquipment)
        {
            equipmentShare = 1f;
        }
        else if (hasRest)
        {
            restShare = 1f;
        }

        if (hasEquipment)
            AddEquipmentChances(results, equipmentShare, equipmentWeight);

        float foodShare = 0f;
        float otherShare = 0f;
        if (hasFood && hasOther)
        {
            foodShare = restShare * Mathf.Clamp01(foodChance);
            otherShare = restShare - foodShare;
        }
        else if (hasFood)
        {
            foodShare = restShare;
        }
        else if (hasOther)
        {
            otherShare = restShare;
        }

        AddNamedChances(results, foods, DropGroup.Food, ShareEach(foodShare, Count(foods)));
        float otherEach = ShareEach(otherShare, OtherCount());
        AddNamedChances(results, scrolls, DropGroup.Other, otherEach);
        AddNamedChances(results, runes, DropGroup.Other, otherEach);
        AddNamedChances(results, resources, DropGroup.Other, otherEach);
        AddNamedChances(results, ammunitions, DropGroup.Other, otherEach);
    }

    void OnValidate()
    {
        restChance = Mathf.Clamp01(restChance);
        foodChance = Mathf.Clamp01(foodChance);
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

    CatalogPick PickRest(bool hasFood, bool hasOther)
    {
        bool food = hasFood && (!hasOther || Random.value < foodChance);
        if (food && Take(foods, Random.Range(0, Count(foods)), out FoodDefinition pickedFood))
            return new CatalogPick { kind = CatalogPickKind.Food, food = pickedFood };

        CatalogPick other = PickOther();
        if (other.kind != CatalogPickKind.None)
            return other;
        if (Take(foods, Random.Range(0, Mathf.Max(1, Count(foods))), out FoodDefinition fallback) && fallback != null)
            return new CatalogPick { kind = CatalogPickKind.Food, food = fallback };
        return default;
    }

    CatalogPick PickOther()
    {
        int count = OtherCount();
        if (count == 0)
            return default;

        int roll = Random.Range(0, count);
        if (Take(scrolls, ref roll, out ScrollDefinition scroll))
            return new CatalogPick { kind = CatalogPickKind.Scroll, scroll = scroll };
        if (Take(runes, ref roll, out RuneDefinition rune))
            return new CatalogPick { kind = CatalogPickKind.Rune, rune = rune };
        if (Take(resources, ref roll, out ResourceDefinition resource))
            return new CatalogPick { kind = CatalogPickKind.Resource, resource = resource };
        if (Take(ammunitions, ref roll, out AmmunitionDefinition ammunition))
            return new CatalogPick { kind = CatalogPickKind.Ammunition, ammunition = ammunition };
        return default;
    }

    void AddEquipmentChances(List<DropChance> results, float equipmentShare, float equipmentWeight)
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
                    label = Label(tierGroup.items[i].displayName, tierGroup.items[i].name),
                    tier = tier,
                    group = DropGroup.Equipment,
                    chance = equipmentShare * weight / equipmentWeight
                });
            }
        }
    }

    static void AddNamedChances<T>(List<DropChance> results, T[] items, DropGroup group, float each) where T : ScriptableObject
    {
        if (items == null || each <= 0f)
            return;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                continue;
            results.Add(new DropChance
            {
                label = Label(DisplayName(items[i]), items[i].name),
                tier = 0,
                group = group,
                chance = each
            });
        }
    }

    static string DisplayName(ScriptableObject item)
    {
        if (item is FoodDefinition food)
            return food.displayName;
        if (item is ScrollDefinition scroll)
            return scroll.displayName;
        if (item is RuneDefinition rune)
            return rune.displayName;
        if (item is ResourceDefinition resource)
            return resource.displayName;
        if (item is AmmunitionDefinition ammunition)
            return ammunition.displayName;
        return item.name;
    }

    static string Label(string displayName, string assetName)
    {
        return !string.IsNullOrEmpty(displayName) ? displayName : assetName;
    }

    static float ShareEach(float share, int count)
    {
        return count <= 0 ? 0f : share / count;
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

    static bool Take<T>(T[] items, int roll, out T item) where T : class
    {
        return Take(items, ref roll, out item);
    }

    static bool Take<T>(T[] items, ref int roll, out T item) where T : class
    {
        item = null;
        if (items == null)
            return false;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                continue;
            if (roll == 0)
            {
                item = items[i];
                return true;
            }

            roll--;
        }

        return false;
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

    int OtherCount()
    {
        return Count(scrolls) + Count(runes) + Count(resources) + Count(ammunitions);
    }

    static int Count<T>(T[] items) where T : class
    {
        if (items == null)
            return 0;

        int count = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                count++;
        }

        return count;
    }

    static bool TryFind<T>(T[] items, string id, out T item) where T : ScriptableObject
    {
        item = null;
        if (items == null || string.IsNullOrEmpty(id))
            return false;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                continue;
            string itemId = items[i] is FoodDefinition food ? food.id
                : items[i] is ScrollDefinition scroll ? scroll.id
                : items[i] is RuneDefinition rune ? rune.id
                : items[i] is ResourceDefinition resource ? resource.id
                : items[i] is AmmunitionDefinition ammunition ? ammunition.id
                : null;
            if (itemId != id)
                continue;
            item = items[i];
            return true;
        }

        return false;
    }
}

public enum CatalogPickKind
{
    None,
    Equipment,
    Food,
    Scroll,
    Rune,
    Resource,
    Ammunition
}

public struct CatalogPick
{
    public CatalogPickKind kind;
    public BaseItem equipment;
    public FoodDefinition food;
    public ScrollDefinition scroll;
    public RuneDefinition rune;
    public ResourceDefinition resource;
    public AmmunitionDefinition ammunition;
}
