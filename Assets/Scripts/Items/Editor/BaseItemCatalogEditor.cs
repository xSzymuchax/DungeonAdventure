using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseItemCatalog))]
public class BaseItemCatalogEditor : Editor
{
    readonly List<BaseItemCatalog.DropChance> chances = new();

    void OnEnable()
    {
        BaseItemCatalogLoader.Load((BaseItemCatalog)target);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BaseItemCatalog catalog = (BaseItemCatalog)target;
        EditorGUILayout.HelpBox("Ekwipunek wczytuje się z folderów Tier1, Tier2, … a przedmioty używane z folderu Consumables.", MessageType.Info);
        if (GUILayout.Button("Wczytaj przedmioty"))
            BaseItemCatalogLoader.Load(catalog);
        catalog.CopyDropChances(chances);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Szanse", EditorStyles.boldLabel);

        int shownTier = int.MinValue;
        bool shownConsumables = false;
        for (int i = 0; i < chances.Count; i++)
        {
            BaseItemCatalog.DropChance entry = chances[i];
            if (entry.consumable)
            {
                if (!shownConsumables)
                {
                    EditorGUILayout.LabelField("Zużywalne", EditorStyles.miniBoldLabel);
                    shownConsumables = true;
                }
            }
            else if (entry.tier != shownTier)
            {
                shownTier = entry.tier;
                EditorGUILayout.LabelField("Tier " + shownTier, EditorStyles.miniBoldLabel);
            }

            string name = entry.item != null && !string.IsNullOrEmpty(entry.item.displayName)
                ? entry.item.displayName
                : entry.item != null ? entry.item.name : "?";
            EditorGUILayout.LabelField(name, (entry.chance * 100f).ToString("0.#") + "%");
        }
    }
}
