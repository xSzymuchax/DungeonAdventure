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
        EditorGUILayout.HelpBox("Ekwipunek wczytuje się z BaseItems/Tier1, Tier2, …. Jedzenie, zwoje i runy z Consumables, zasoby z Resources, amunicja z Ammunition, unikalne z Uniques. Unikalny może być dowolnym przedmiotem. Waga kategorii wybiera grupę. Pusta kategoria i waga 0 nie wchodzą. Ekwipunek losuje tier wagą 1/tier, a w środku przedmiot według jego wagi. Szansa ekwipunku jest na tier. Każdy przedmiot ma wagę, bazowo 10. Unikalny bierze wpisane statystyki, bez losowych modyfikatorów i bez ulepszenia.", MessageType.Info);
        if (GUILayout.Button("Wczytaj przedmioty"))
            ItemCatalogLoader.Load(catalog);
        catalog.CopyDropChances(chances);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Szanse", EditorStyles.boldLabel);

        ItemCatalog.DropCategory shown = (ItemCatalog.DropCategory)(-1);
        for (int i = 0; i < chances.Count; i++)
        {
            ItemCatalog.DropChance entry = chances[i];
            if (entry.category != shown)
            {
                shown = entry.category;
                EditorGUILayout.LabelField(CategoryLabel(entry.category), EditorStyles.miniBoldLabel);
            }

            if (entry.category == ItemCatalog.DropCategory.Equipment)
            {
                EditorGUILayout.LabelField("Tier " + entry.tier, (entry.chance * 100f).ToString("0.#") + "%");
                continue;
            }

            EditorGUILayout.LabelField(string.IsNullOrEmpty(entry.label) ? "?" : entry.label, (entry.chance * 100f).ToString("0.#") + "%");
        }
    }

    static string CategoryLabel(ItemCatalog.DropCategory category)
    {
        if (category == ItemCatalog.DropCategory.Equipment)
            return "Ekwipunek";
        if (category == ItemCatalog.DropCategory.Food)
            return "Jedzenie";
        if (category == ItemCatalog.DropCategory.Scroll)
            return "Zwoje";
        if (category == ItemCatalog.DropCategory.Rune)
            return "Runy";
        if (category == ItemCatalog.DropCategory.Resource)
            return "Zasoby";
        if (category == ItemCatalog.DropCategory.Ammunition)
            return "Amunicja";
        if (category == ItemCatalog.DropCategory.Unique)
            return "Unikalne";
        return category.ToString();
    }
}
