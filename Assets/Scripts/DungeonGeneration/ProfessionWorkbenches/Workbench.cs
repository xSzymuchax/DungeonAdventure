using System;
using System.Collections.Generic;
using UnityEngine;

public class Workbench : CraftBench
{
    public const int ResourceSlotCount = 3;

    readonly Item[] resources = new Item[ResourceSlotCount];
    UpgradeRecipe[] recipes = System.Array.Empty<UpgradeRecipe>();
    UpgradeWorkbenchPanel panel;
    Item subject;
    Item result;
    Item forecast;
    EquipmentItem forecastFrom;
    int forecastLevel;

    public WorkbenchPenalty Penalty { get; set; }
    public Item Subject => subject;
    public Item Result => result;

    public Item ShownResult()
    {
        if (result != null)
            return result;
        if (FindSatisfied() == null || subject is not EquipmentItem equipment)
        {
            forecast = null;
            forecastFrom = null;
            return null;
        }

        int level = NextLevel(equipment);
        if (forecastFrom != equipment || forecast == null || forecastLevel != level)
        {
            forecastFrom = equipment;
            forecastLevel = level;
            forecast = UpgradedCopy(equipment, level);
        }

        return forecast;
    }

    public void Setup(TileInfo tile, UpgradeWorkbenchPanel view, UpgradeRecipe[] known)
    {
        Place(tile);
        panel = view;
        recipes = known ?? System.Array.Empty<UpgradeRecipe>();
        subject = null;
        result = null;
        for (int i = 0; i < resources.Length; i++)
            resources[i] = null;
    }

    public Item ResourceAt(int index)
    {
        if (index < 0 || index >= resources.Length)
            return null;
        return resources[index];
    }

    public override void Open(PlayerCharacter player)
    {
        if (GameController.Instance != null)
            GameController.Instance.CloseBenchesExcept(this);
        if (panel != null)
            panel.Open(player, this);
    }

    public override bool CanCraft()
    {
        return FindSatisfied() != null && result == null && subject is EquipmentItem;
    }

    public override void ClosePanel()
    {
        if (panel != null)
            panel.Close();
    }

    public bool TryPlaceSubject(PlayerInventory inventory, Item item)
    {
        if (subject != null || item is not EquipmentItem equipment)
            return false;
        return PlaceSubject(inventory, equipment);
    }

    public bool TryPlaceResource(PlayerInventory inventory, int index, Item item)
    {
        if (item is not ResourceItem || index < 0 || index >= resources.Length || resources[index] != null)
            return false;
        if (!inventory.TrySplitOne(item, out Item one) || one == null)
            return false;

        one.Count = 1;
        resources[index] = one;
        if (panel != null)
            panel.Refresh();
        return true;
    }

    public bool ReturnSubject(PlayerInventory inventory)
    {
        return Return(inventory, subject, value => subject = value);
    }

    public bool ReturnResource(PlayerInventory inventory, int index)
    {
        if (index < 0 || index >= resources.Length)
            return false;
        return Return(inventory, resources[index], value => resources[index] = value);
    }

    public bool TakeResult(PlayerInventory inventory)
    {
        return Return(inventory, result, value => result = value);
    }

    public override void ReturnAll(PlayerInventory inventory)
    {
        ReturnSubject(inventory);
        for (int i = 0; i < resources.Length; i++)
            ReturnResource(inventory, i);
        TakeResult(inventory);
    }

    public UpgradeRecipe FindSatisfied()
    {
        UpgradeRecipe[] known = KnownRecipes();
        for (int i = 0; i < known.Length; i++)
        {
            UpgradeRecipe recipe = known[i];
            if (recipe != null && recipe.Accepts(subject) && recipe.HasIngredients(resources))
                return recipe;
        }

        return null;
    }

    UpgradeRecipe[] KnownRecipes()
    {
        if (HasAny(recipes))
            return recipes;

        GameController controller = GameController.Instance;
        if (controller != null && HasAny(controller.upgradeRecipes))
            return controller.upgradeRecipes;

        return Array.Empty<UpgradeRecipe>();
    }

    static bool HasAny(UpgradeRecipe[] list)
    {
        if (list == null)
            return false;

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] != null)
                return true;
        }

        return false;
    }

    public string Preview()
    {
        if (FindSatisfied() == null || subject is not EquipmentItem equipment)
            return "";
        return UpgradeRecipe.UpgradedStats(equipment);
    }

    public override bool Craft()
    {
        UpgradeRecipe recipe = FindSatisfied();
        if (recipe == null || result != null || subject is not EquipmentItem equipment)
            return false;

        Item produced = UpgradedCopy(equipment, NextLevel(equipment));
        if (!Consume(recipe))
            return false;

        subject = null;
        result = produced;
        if (panel != null)
            panel.Refresh();
        return true;
    }

    public void DiscardSubject()
    {
        subject = null;
        if (panel != null)
            panel.Refresh();
    }

    public void DiscardPlaced()
    {
        subject = null;
        for (int i = 0; i < resources.Length; i++)
            resources[i] = null;
        if (panel != null)
            panel.Refresh();
    }

    bool PlaceSubject(PlayerInventory inventory, EquipmentItem equipment)
    {
        if (!inventory.TrySplitOne(equipment, out Item taken) || taken is not EquipmentItem piece)
            return false;

        Item previous = subject;
        subject = piece;
        if (previous != null && !inventory.TryAdd(previous))
        {
            subject = previous;
            inventory.TryAdd(piece);
            return false;
        }

        if (panel != null)
            panel.Refresh();
        return true;
    }

    bool Return(PlayerInventory inventory, Item item, System.Action<Item> clear)
    {
        if (inventory == null || item == null)
            return false;
        if (!inventory.TryAdd(item))
            return false;

        clear(null);
        if (panel != null)
            panel.Refresh();
        return true;
    }

    static int NextLevel(EquipmentItem equipment)
    {
        int level = equipment.Level + 1;
        if (level > Consts.MAX_UPGRADE_LEVEL)
            level = Consts.MAX_UPGRADE_LEVEL;
        return level;
    }

    static Item UpgradedCopy(EquipmentItem equipment, int level)
    {
        Item produced = equipment.Copy();
        if (produced is EquipmentItem upgraded)
            upgraded.Level = level;
        return produced;
    }

    bool Consume(UpgradeRecipe recipe)
    {
        if (recipe.ingredients == null)
            return true;

        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            RecipeIngredient ingredient = recipe.ingredients[i];
            if (ingredient == null || ingredient.count <= 0)
                continue;
            if (!Spend(ingredient.itemId, ingredient.count))
                return false;
        }

        return true;
    }

    bool Spend(string id, int count)
    {
        for (int i = 0; i < resources.Length && count > 0; i++)
        {
            Item slot = resources[i];
            if (slot == null || !UpgradeRecipe.SameId(slot.Id, id))
                continue;

            int taken = slot.Count < count ? slot.Count : count;
            slot.Count -= taken;
            count -= taken;
            if (slot.Count <= 0)
                resources[i] = null;
        }

        return count <= 0;
    }
}
