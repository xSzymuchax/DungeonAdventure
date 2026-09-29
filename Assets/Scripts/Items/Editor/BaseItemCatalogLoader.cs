using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BaseItemCatalogLoader
{
    static readonly Regex TierFolder = new Regex("^Tier(\\d+)$", RegexOptions.IgnoreCase);
    static readonly HashSet<string> warnedIds = new();
    static bool loading;

    static BaseItemCatalogLoader()
    {
        EditorApplication.delayCall += LoadAll;
    }

    public static void LoadAll()
    {
        if (loading || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        string[] catalogs = AssetDatabase.FindAssets("t:BaseItemCatalog");
        for (int i = 0; i < catalogs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(catalogs[i]);
            BaseItemCatalog catalog = AssetDatabase.LoadAssetAtPath<BaseItemCatalog>(path);
            if (catalog != null)
                Load(catalog);
        }
    }

    public static void Load(BaseItemCatalog catalog)
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
            var groups = new List<BaseItemCatalog.EquipmentTier>();
            string[] folders = AssetDatabase.GetSubFolders(directory);
            var found = new List<(int tier, string folder)>();
            for (int i = 0; i < folders.Length; i++)
            {
                string name = Path.GetFileName(folders[i].Replace('\\', '/'));
                Match match = TierFolder.Match(name);
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out int tier) || tier < 1)
                    continue;
                found.Add((tier, folders[i].Replace('\\', '/')));
            }

            found.Sort((a, b) => a.tier.CompareTo(b.tier));
            var seenIds = new Dictionary<string, string>();
            for (int i = 0; i < found.Count; i++)
            {
                BaseItem[] items = LoadItems(found[i].folder, found[i].tier, seenIds);
                if (items.Length == 0)
                    continue;
                groups.Add(new BaseItemCatalog.EquipmentTier { tier = found[i].tier, items = items });
            }

            BaseItem[] consumables = LoadConsumables(directory + "/Consumables", seenIds);
            if (Same(catalog.equipment, groups) && SameItems(catalog.consumables, consumables))
                return;

            Undo.RecordObject(catalog, "Wczytaj przedmioty");
            catalog.equipment = groups.ToArray();
            catalog.consumables = consumables;
            EditorUtility.SetDirty(catalog);
        }
        finally
        {
            loading = false;
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

    static bool Same(BaseItemCatalog.EquipmentTier[] current, List<BaseItemCatalog.EquipmentTier> next)
    {
        if (current == null || current.Length != next.Count)
            return false;

        for (int i = 0; i < current.Length; i++)
        {
            BaseItemCatalog.EquipmentTier group = current[i];
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

    static BaseItem[] LoadConsumables(string folder, Dictionary<string, string> seenIds)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return new BaseItem[0];

        BaseItem[] items = LoadItems(folder, 0, seenIds);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].kind == ItemKind.Consumable)
                continue;
            items[i].kind = ItemKind.Consumable;
            EditorUtility.SetDirty(items[i]);
        }

        return items;
    }

    static bool SameItems(BaseItem[] current, BaseItem[] next)
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

public class BaseItemCatalogPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (Relevant(imported) || Relevant(deleted) || Relevant(moved) || Relevant(movedFrom))
            EditorApplication.delayCall += BaseItemCatalogLoader.LoadAll;
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
            if (path.IndexOf("/Tier", System.StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/Consumables", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
}
