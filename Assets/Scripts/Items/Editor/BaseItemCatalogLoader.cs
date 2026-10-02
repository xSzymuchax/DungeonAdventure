using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ItemCatalogLoader
{
    static readonly Regex TierFolder = new Regex("^Tier(\\d+)$", RegexOptions.IgnoreCase);
    static readonly HashSet<string> warnedIds = new();
    static bool loading;

    static ItemCatalogLoader()
    {
        EditorApplication.delayCall += LoadAll;
    }

    public static void LoadAll()
    {
        if (loading || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        string[] catalogs = AssetDatabase.FindAssets("t:ItemCatalog");
        for (int i = 0; i < catalogs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(catalogs[i]);
            ItemCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>(path);
            if (catalog != null)
                Load(catalog);
        }
    }

    public static void Load(ItemCatalog catalog)
    {
        if (catalog == null || loading)
            return;

        string catalogPath = AssetDatabase.GetAssetPath(catalog);
        string directory = Path.GetDirectoryName(catalogPath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(directory))
            return;

        loading = true;
        try
        {
            var groups = new List<ItemCatalog.EquipmentTier>();
            var found = new List<(int tier, string folder)>();
            CollectTiers(directory + "/BaseItems", found);

            found.Sort((a, b) => a.tier.CompareTo(b.tier));
            var seenIds = new Dictionary<string, string>();
            for (int i = 0; i < found.Count; i++)
            {
                BaseItem[] items = LoadItems(found[i].folder, found[i].tier, seenIds);
                if (items.Length == 0)
                    continue;
                groups.Add(new ItemCatalog.EquipmentTier { tier = found[i].tier, items = items });
            }

            FoodDefinition[] foods = LoadDefinitions<FoodDefinition>(directory + "/Consumables/Food", seenIds);
            ScrollDefinition[] scrolls = LoadDefinitions<ScrollDefinition>(directory + "/Consumables/Scrolls", seenIds);
            RuneDefinition[] runes = LoadDefinitions<RuneDefinition>(directory + "/Consumables/Runes", seenIds);
            ResourceDefinition[] resources = LoadDefinitions<ResourceDefinition>(directory + "/Resources", seenIds);
            AmmunitionDefinition[] ammunitions = LoadDefinitions<AmmunitionDefinition>(directory + "/Ammunition", seenIds);
            if (Same(catalog.equipment, groups)
                && SameItems(catalog.foods, foods)
                && SameItems(catalog.scrolls, scrolls)
                && SameItems(catalog.runes, runes)
                && SameItems(catalog.resources, resources)
                && SameItems(catalog.ammunitions, ammunitions))
                return;

            Undo.RecordObject(catalog, "Wczytaj przedmioty");
            catalog.equipment = groups.ToArray();
            catalog.foods = foods;
            catalog.scrolls = scrolls;
            catalog.runes = runes;
            catalog.resources = resources;
            catalog.ammunitions = ammunitions;
            EditorUtility.SetDirty(catalog);
        }
        finally
        {
            loading = false;
        }
    }

    static void CollectTiers(string root, List<(int tier, string folder)> found)
    {
        if (!AssetDatabase.IsValidFolder(root))
            return;

        string[] folders = AssetDatabase.GetSubFolders(root);
        for (int i = 0; i < folders.Length; i++)
        {
            string path = folders[i].Replace('\\', '/');
            string name = Path.GetFileName(path);
            Match match = TierFolder.Match(name);
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out int tier) || tier < 1)
                continue;
            found.Add((tier, path));
        }
    }

    static BaseItem[] LoadItems(string folder, int tier, Dictionary<string, string> seenIds)
    {
        string[] guids = AssetDatabase.FindAssets("t:BaseItem", new[] { folder });
        var items = new List<BaseItem>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (Path.GetDirectoryName(path)?.Replace('\\', '/') != folder)
                continue;
            BaseItem item = AssetDatabase.LoadAssetAtPath<BaseItem>(path);
            if (item == null)
                continue;
            if (tier >= 1 && item.tier != tier)
            {
                item.tier = tier;
                EditorUtility.SetDirty(item);
            }

            string tierId = WithTierSuffix(item.id, tier);
            if (tier >= 1 && item.id != tierId)
            {
                item.id = tierId;
                EditorUtility.SetDirty(item);
            }

            if (!string.IsNullOrEmpty(item.id))
            {
                if (seenIds.TryGetValue(item.id, out string first) && first != path)
                {
                    if (warnedIds.Add(item.id))
                        Debug.LogWarning("Przedmioty o id \"" + item.id + "\" są w więcej niż jednym miejscu: " + first + " oraz " + path);
                }
                else
                    seenIds[item.id] = path;
            }

            items.Add(item);
        }

        items.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return items.ToArray();
    }

    static bool Same(ItemCatalog.EquipmentTier[] current, List<ItemCatalog.EquipmentTier> next)
    {
        if (current == null || current.Length != next.Count)
            return false;

        for (int i = 0; i < current.Length; i++)
        {
            ItemCatalog.EquipmentTier group = current[i];
            if (group == null || group.tier != next[i].tier || group.items == null || group.items.Length != next[i].items.Length)
                return false;
            for (int j = 0; j < group.items.Length; j++)
            {
                if (group.items[j] != next[i].items[j])
                    return false;
            }
        }

        return true;
    }

    static T[] LoadDefinitions<T>(string folder, Dictionary<string, string> seenIds) where T : ScriptableObject
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return new T[0];

        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
        var items = new List<T>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            T item = AssetDatabase.LoadAssetAtPath<T>(path);
            if (item == null)
                continue;

            string id = ItemId(item);
            if (!string.IsNullOrEmpty(id))
            {
                if (seenIds.TryGetValue(id, out string first) && first != path)
                {
                    if (warnedIds.Add(id))
                        Debug.LogWarning("Przedmioty o id \"" + id + "\" są w więcej niż jednym miejscu: " + first + " oraz " + path);
                }
                else
                    seenIds[id] = path;
            }

            items.Add(item);
        }

        items.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return items.ToArray();
    }

    static string WithTierSuffix(string id, int tier)
    {
        if (string.IsNullOrEmpty(id) || tier < 1)
            return id;

        string suffix = "_tier" + tier;
        int marker = id.LastIndexOf("_tier", System.StringComparison.Ordinal);
        if (marker >= 0 && int.TryParse(id.Substring(marker + 5), out _))
            id = id.Substring(0, marker);
        return id + suffix;
    }

    static string ItemId(ScriptableObject item)
    {
        if (item is FoodDefinition food)
            return food.id;
        if (item is ScrollDefinition scroll)
            return scroll.id;
        if (item is RuneDefinition rune)
            return rune.id;
        if (item is ResourceDefinition resource)
            return resource.id;
        if (item is BaseItem equipment)
            return equipment.id;
        return null;
    }

    static bool SameItems<T>(T[] current, T[] next) where T : class
    {
        if (current == null || current.Length != next.Length)
            return false;
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] != next[i])
                return false;
        }

        return true;
    }
}

public class ItemCatalogPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (Relevant(imported) || Relevant(deleted) || Relevant(moved) || Relevant(movedFrom))
            EditorApplication.delayCall += ItemCatalogLoader.LoadAll;
    }

    static bool Relevant(string[] paths)
    {
        if (paths == null)
            return false;
        for (int i = 0; i < paths.Length; i++)
        {
            string path = paths[i]?.Replace('\\', '/');
            if (string.IsNullOrEmpty(path))
                continue;
            if (path.IndexOf("/BaseItems", System.StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/Consumables", System.StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/Resources", System.StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/Ammunition", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
}
