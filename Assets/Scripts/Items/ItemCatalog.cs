using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCatalog", menuName = "Dungeon/Item Catalog")]
public class ItemCatalog : ScriptableObject
{
    [Header("Wagi kategorii")]
    [Min(0f)] public float equipmentWeight = 50f;
    [Min(0f)] public float foodWeight = 25f;
    [Min(0f)] public float scrollWeight = 5f;
    [Min(0f)] public float runeWeight = 5f;
    [Min(0f)] public float resourceWeight = 5f;
    [Min(0f)] public float ammunitionWeight = 15f;
    [Min(0f)] public float uniqueWeight = 5f;
    public StatRollTable statRolls;
    [HideInInspector] public EquipmentTier[] equipment;
    [HideInInspector] public FoodDefinition[] foods;
    [HideInInspector] public ScrollDefinition[] scrolls;
    [HideInInspector] public RuneDefinition[] runes;
    [HideInInspector] public ResourceDefinition[] resources;
    [HideInInspector] public AmmunitionDefinition[] ammunitions;
    [HideInInspector] public ScriptableObject[] uniques;

    [System.Serializable]
    public class EquipmentTier
    {
        public int tier = 1;
        public BaseItem[] items;
    }

    public struct DropChance
    {
        public string label;
        public int tier;
        public DropCategory category;
        public float chance;
    }

    public enum DropCategory
    {
        Equipment,
        Food,
        Scroll,
        Rune,
        Resource,
        Ammunition,
        Unique
    }

    static readonly DropCategory[] Categories =
    {
        DropCategory.Equipment,
        DropCategory.Food,
        DropCategory.Scroll,
        DropCategory.Rune,
        DropCategory.Resource,
        DropCategory.Ammunition,
        DropCategory.Unique
    };

    public CatalogPick RollPick()
    {
        float total = CategoryTotal();
        if (total <= 0f)
            return default;

        float roll = Random.value * total;
        float cursor = 0f;
        DropCategory last = DropCategory.Equipment;
        for (int i = 0; i < Categories.Length; i++)
        {
            float weight = CategoryWeight(Categories[i]);
            if (weight <= 0f)
                continue;

            last = Categories[i];
            cursor += weight;
            if (roll <= cursor)
                return PickCategory(last);
        }

        return PickCategory(last);
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

        return FindUnique(id, out item);
    }

    public bool TryGetFood(string id, out FoodDefinition item)
    {
        return TryFind(foods, id, out item) || FindUnique(id, out item);
    }

    public bool TryGetScroll(string id, out ScrollDefinition item)
    {
        return TryFind(scrolls, id, out item) || FindUnique(id, out item);
    }

    public bool TryGetRune(string id, out RuneDefinition item)
    {
        return TryFind(runes, id, out item) || FindUnique(id, out item);
    }

    public bool TryGetResource(string id, out ResourceDefinition item)
    {
        return TryFind(resources, id, out item) || FindUnique(id, out item);
    }

    public bool TryGetAmmunition(string id, out AmmunitionDefinition item)
    {
        return TryFind(ammunitions, id, out item) || FindUnique(id, out item);
    }

    public void CopyDropChances(List<DropChance> results)
    {
        results.Clear();
        float total = CategoryTotal();
        if (total <= 0f)
            return;

        for (int i = 0; i < Categories.Length; i++)
        {
            DropCategory category = Categories[i];
            float share = CategoryWeight(category) / total;
            if (share <= 0f)
                continue;

            if (category == DropCategory.Equipment)
                AddEquipmentChances(results, share, TierWeight());
            else if (category == DropCategory.Food)
                AddWeightedChances(results, foods, category, share);
            else if (category == DropCategory.Scroll)
                AddWeightedChances(results, scrolls, category, share);
            else if (category == DropCategory.Rune)
                AddWeightedChances(results, runes, category, share);
            else if (category == DropCategory.Resource)
                AddWeightedChances(results, resources, category, share);
            else if (category == DropCategory.Ammunition)
                AddWeightedChances(results, ammunitions, category, share);
            else if (category == DropCategory.Unique)
                AddWeightedChances(results, uniques, category, share);
        }
    }

    void OnValidate()
    {
        equipmentWeight = Mathf.Max(0f, equipmentWeight);
        foodWeight = Mathf.Max(0f, foodWeight);
        scrollWeight = Mathf.Max(0f, scrollWeight);
        runeWeight = Mathf.Max(0f, runeWeight);
        resourceWeight = Mathf.Max(0f, resourceWeight);
        ammunitionWeight = Mathf.Max(0f, ammunitionWeight);
        uniqueWeight = Mathf.Max(0f, uniqueWeight);
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

    CatalogPick PickCategory(DropCategory category)
    {
        if (category == DropCategory.Equipment)
        {
            BaseItem picked = PickEquipment();
            return picked == null
                ? default
                : new CatalogPick { kind = CatalogPickKind.Equipment, equipment = picked };
        }

        if (category == DropCategory.Food && PickWeighted(foods) is FoodDefinition food)
            return new CatalogPick { kind = CatalogPickKind.Food, food = food };
        if (category == DropCategory.Scroll && PickWeighted(scrolls) is ScrollDefinition scroll)
            return new CatalogPick { kind = CatalogPickKind.Scroll, scroll = scroll };
        if (category == DropCategory.Rune && PickWeighted(runes) is RuneDefinition rune)
            return new CatalogPick { kind = CatalogPickKind.Rune, rune = rune };
        if (category == DropCategory.Resource && PickWeighted(resources) is ResourceDefinition resource)
            return new CatalogPick { kind = CatalogPickKind.Resource, resource = resource };
        if (category == DropCategory.Ammunition && PickWeighted(ammunitions) is AmmunitionDefinition ammunition)
            return new CatalogPick { kind = CatalogPickKind.Ammunition, ammunition = ammunition };
        if (category == DropCategory.Unique)
            return Authored(PickWeighted(uniques));
        return default;
    }

    static CatalogPick Authored(ScriptableObject item)
    {
        if (item is BaseItem equipment)
            return new CatalogPick { kind = CatalogPickKind.Equipment, equipment = equipment, authored = true };
        if (item is FoodDefinition food)
            return new CatalogPick { kind = CatalogPickKind.Food, food = food, authored = true };
        if (item is ScrollDefinition scroll)
            return new CatalogPick { kind = CatalogPickKind.Scroll, scroll = scroll, authored = true };
        if (item is RuneDefinition rune)
            return new CatalogPick { kind = CatalogPickKind.Rune, rune = rune, authored = true };
        if (item is ResourceDefinition resource)
            return new CatalogPick { kind = CatalogPickKind.Resource, resource = resource, authored = true };
        if (item is AmmunitionDefinition ammunition)
            return new CatalogPick { kind = CatalogPickKind.Ammunition, ammunition = ammunition, authored = true };
        return default;
    }

    float CategoryTotal()
    {
        float total = 0f;
        for (int i = 0; i < Categories.Length; i++)
            total += CategoryWeight(Categories[i]);
        return total;
    }

    float CategoryWeight(DropCategory category)
    {
        if (!HasItems(category))
            return 0f;

        float weight = category == DropCategory.Equipment ? equipmentWeight
            : category == DropCategory.Food ? foodWeight
            : category == DropCategory.Scroll ? scrollWeight
            : category == DropCategory.Rune ? runeWeight
            : category == DropCategory.Resource ? resourceWeight
            : category == DropCategory.Ammunition ? ammunitionWeight
            : category == DropCategory.Unique ? uniqueWeight
            : 0f;
        return Mathf.Max(0f, weight);
    }

    bool HasItems(DropCategory category)
    {
        if (category == DropCategory.Equipment)
            return TierWeight() > 0f;
        if (category == DropCategory.Food)
            return PoolWeight(foods) > 0f;
        if (category == DropCategory.Scroll)
            return PoolWeight(scrolls) > 0f;
        if (category == DropCategory.Rune)
            return PoolWeight(runes) > 0f;
        if (category == DropCategory.Resource)
            return PoolWeight(resources) > 0f;
        if (category == DropCategory.Ammunition)
            return PoolWeight(ammunitions) > 0f;
        if (category == DropCategory.Unique)
            return PoolWeight(uniques) > 0f;
        return false;
    }

    void AddEquipmentChances(List<DropChance> results, float equipmentShare, float tierWeight)
    {
        if (equipment == null || tierWeight <= 0f)
            return;

        for (int group = 0; group < equipment.Length; group++)
        {
            EquipmentTier tierGroup = equipment[group];
            if (!HasTierItems(tierGroup))
                continue;

            int tier = Mathf.Max(1, tierGroup.tier);
            float weight = 1f / tier;
            results.Add(new DropChance
            {
                label = "Tier " + tier,
                tier = tier,
                category = DropCategory.Equipment,
                chance = equipmentShare * weight / tierWeight
            });
        }
    }

    static void AddWeightedChances<T>(List<DropChance> results, T[] items, DropCategory category, float share) where T : ScriptableObject
    {
        float total = PoolWeight(items);
        if (items == null || total <= 0f || share <= 0f)
            return;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                continue;
            float weight = ItemWeight(items[i]);
            if (weight <= 0f)
                continue;
            results.Add(new DropChance
            {
                label = Label(DisplayName(items[i]), items[i].name),
                tier = 0,
                category = category,
                chance = share * weight / total
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
        if (item is BaseItem equipment)
            return equipment.displayName;
        return item.name;
    }

    static string Label(string displayName, string assetName)
    {
        return !string.IsNullOrEmpty(displayName) ? displayName : assetName;
    }

    BaseItem PickEquipment()
    {
        float total = TierWeight();
        if (total <= 0f || equipment == null)
            return null;

        float roll = Random.value * total;
        float cursor = 0f;
        EquipmentTier chosen = null;
        for (int group = 0; group < equipment.Length; group++)
        {
            EquipmentTier tierGroup = equipment[group];
            if (!HasTierItems(tierGroup))
                continue;

            chosen = tierGroup;
            cursor += 1f / Mathf.Max(1, tierGroup.tier);
            if (roll <= cursor)
                break;
        }

        return PickInTier(chosen);
    }

    static BaseItem PickInTier(EquipmentTier tierGroup)
    {
        if (tierGroup == null || tierGroup.items == null)
            return null;

        float total = 0f;
        for (int i = 0; i < tierGroup.items.Length; i++)
        {
            if (tierGroup.items[i] != null)
                total += ItemWeight(tierGroup.items[i]);
        }

        if (total <= 0f)
            return null;

        float roll = Random.value * total;
        float cursor = 0f;
        BaseItem last = null;
        for (int i = 0; i < tierGroup.items.Length; i++)
        {
            if (tierGroup.items[i] == null)
                continue;
            float weight = ItemWeight(tierGroup.items[i]);
            if (weight <= 0f)
                continue;
            last = tierGroup.items[i];
            cursor += weight;
            if (roll <= cursor)
                return last;
        }

        return last;
    }

    static T PickWeighted<T>(T[] items) where T : ScriptableObject
    {
        float total = PoolWeight(items);
        if (total <= 0f || items == null)
            return null;

        float roll = Random.value * total;
        float cursor = 0f;
        T last = null;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                continue;
            float weight = ItemWeight(items[i]);
            if (weight <= 0f)
                continue;
            last = items[i];
            cursor += weight;
            if (roll <= cursor)
                return last;
        }

        return last;
    }

    static float PoolWeight<T>(T[] items) where T : ScriptableObject
    {
        if (items == null)
            return 0f;

        float total = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
                total += ItemWeight(items[i]);
        }

        return total;
    }

    static float ItemWeight(ScriptableObject item)
    {
        float weight = item is FoodDefinition food ? food.weight
            : item is ScrollDefinition scroll ? scroll.weight
            : item is RuneDefinition rune ? rune.weight
            : item is ResourceDefinition resource ? resource.weight
            : item is AmmunitionDefinition ammunition ? ammunition.weight
            : item is BaseItem equipment ? equipment.weight
            : 0f;
        return Mathf.Max(0f, weight);
    }

    float TierWeight()
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
            if (HasTierItems(tierGroup))
                total += weight;
        }

        return total;
    }

    static bool HasTierItems(EquipmentTier tierGroup)
    {
        if (tierGroup == null || tierGroup.items == null)
            return false;

        for (int i = 0; i < tierGroup.items.Length; i++)
        {
            if (tierGroup.items[i] != null && ItemWeight(tierGroup.items[i]) > 0f)
                return true;
        }

        return false;
    }

    bool FindUnique<T>(string id, out T item) where T : ScriptableObject
    {
        item = null;
        if (uniques == null || string.IsNullOrEmpty(id))
            return false;

        for (int i = 0; i < uniques.Length; i++)
        {
            if (uniques[i] is T found && DefinitionId(found) == id)
            {
                item = found;
                return true;
            }
        }

        return false;
    }

    static string DefinitionId(ScriptableObject item)
    {
        if (item is FoodDefinition food)
            return food.id;
        if (item is ScrollDefinition scroll)
            return scroll.id;
        if (item is RuneDefinition rune)
            return rune.id;
        if (item is ResourceDefinition resource)
            return resource.id;
        if (item is AmmunitionDefinition ammunition)
            return ammunition.id;
        if (item is BaseItem equipment)
            return equipment.id;
        return null;
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
                : items[i] is BaseItem equipment ? equipment.id
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
    public bool authored;
}
