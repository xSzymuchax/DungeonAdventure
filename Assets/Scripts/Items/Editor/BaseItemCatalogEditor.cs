using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemCatalog))]
public class ItemCatalogEditor : Editor
{
    readonly List<ItemCatalog.DropChance> chances = new();

    void OnEnable()
    {
        ItemCatalogLoader.Load((ItemCatalog)target);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemCatalog catalog = (ItemCatalog)target;
        EditorGUILayout.HelpBox("Ekwipunek wczytuje się z BaseItems/Tier1, Tier2, …. Jedzenie, zwoje i runy z Consumables, zasoby z Resources, amunicja z Ammunition. restChance to szansa reszty zamiast ekwipunku. foodChance to szansa jedzenia w tej reszcie. Pozostała część reszty dzieli się równo między zwoje, runy, zasoby i amunicję.", MessageType.Info);
        if (GUILayout.Button("Wczytaj przedmioty"))
            ItemCatalogLoader.Load(catalog);
        catalog.CopyDropChances(chances);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Szanse", EditorStyles.boldLabel);

        int shownTier = int.MinValue;
        bool shownFood = false;
        bool shownOther = false;
        for (int i = 0; i < chances.Count; i++)
        {
            ItemCatalog.DropChance entry = chances[i];
            if (entry.group == ItemCatalog.DropGroup.Food)
            {
                if (!shownFood)
                {
                    EditorGUILayout.LabelField("Jedzenie", EditorStyles.miniBoldLabel);
                    shownFood = true;
                }
            }
            else if (entry.group == ItemCatalog.DropGroup.Other)
            {
                if (!shownOther)
                {
                    EditorGUILayout.LabelField("Reszta", EditorStyles.miniBoldLabel);
                    shownOther = true;
                }
            }
            else if (entry.tier != shownTier)
            {
                shownTier = entry.tier;
                EditorGUILayout.LabelField("Tier " + shownTier, EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.LabelField(string.IsNullOrEmpty(entry.label) ? "?" : entry.label, (entry.chance * 100f).ToString("0.#") + "%");
        }
    }
}
