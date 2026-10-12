using System;
using System.Collections;
using UnityEngine;

public class RunecraftWorkbench : CraftBench
{
    public const int SlotCount = 6;

    readonly Item[] slots = new Item[SlotCount];
    RuneRecipe[] recipes = Array.Empty<RuneRecipe>();
    RunecraftWorkbenchPanel panel;
    Item result;
    Item forecast;
    Scroll forecastScroll;
    int forecastSpell;
    IEnumerator backlash;
    SpellBlast blastPrefab;

    public Item Result => result;

    public void Setup(TileInfo tile, RunecraftWorkbenchPanel view, RuneRecipe[] known, SpellBlast blast)
    {
        Place(tile);
        panel = view;
        blastPrefab = blast;
        recipes = known ?? Array.Empty<RuneRecipe>();
        result = null;
        forecast = null;
        for (int i = 0; i < slots.Length; i++)
            slots[i] = null;
    }

    public Item SlotAt(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;
        return slots[index];
    }

    public Item ShownResult()
    {
        if (result != null)
            return result;

        RuneRecipe recipe = FindSatisfied();
        if (recipe == null)
        {
            forecast = null;
            forecastScroll = null;
            return null;
        }

        if (recipe.Inscribes)
        {
            Scroll scroll = recipe.PlacedScroll(slots);
            int spell = scroll != null ? scroll.SpellLevel : 0;
            if (forecast == null || forecastScroll != scroll || forecastSpell != spell)
            {
                forecastScroll = scroll;
                forecastSpell = spell;
                forecast = recipe.CreateResult(slots);
            }

            return forecast;
        }

        if (forecast == null)
            forecast = recipe.CreateResult(slots);
        return forecast;
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
        return FindSatisfied() != null && result == null;
    }

    public override void ClosePanel()
    {
        if (panel != null)
            panel.Close();
    }

    public bool TryPlaceAt(PlayerInventory inventory, int index, Item item)
    {
        if (inventory == null || item == null)
            return false;
        if (index < 0 || index >= slots.Length || slots[index] != null)
            return false;
        if (!inventory.TrySplitOne(item, out Item one) || one == null)
            return false;

        one.Count = 1;
        slots[index] = one;
        if (panel != null)
            panel.Refresh();
        return true;
    }

    public bool ReturnSlot(PlayerInventory inventory, int index)
    {
        if (index < 0 || index >= slots.Length)
            return false;
        return Return(inventory, slots[index], value => slots[index] = value);
    }

    public bool TakeResult(PlayerInventory inventory)
    {
        return Return(inventory, result, value => result = value);
    }

    public override void ReturnAll(PlayerInventory inventory)
    {
        for (int i = 0; i < slots.Length; i++)
            ReturnSlot(inventory, i);
        TakeResult(inventory);
    }

    public RuneRecipe FindSatisfied()
    {
        for (int i = 0; i < recipes.Length; i++)
        {
            RuneRecipe recipe = recipes[i];
            if (recipe != null && recipe.HasIngredients(slots))
                return recipe;
        }

        return null;
    }

    public override bool Craft()
    {
        RuneRecipe recipe = FindSatisfied();
        if (recipe == null || result != null)
            return false;

        if (recipe.Inscribes)
            return Inscribe(recipe);

        Item produced = recipe.CreateResult(slots);
        if (produced == null || !Consume(recipe))
            return false;

        result = produced;
        forecast = null;
        forecastScroll = null;
        if (panel != null)
            panel.Refresh();
        return true;
    }

    bool Inscribe(RuneRecipe recipe)
    {
        Scroll scroll = recipe.PlacedScroll(slots);
        int spell = scroll != null && scroll.SpellLevel > 0 ? scroll.SpellLevel : 1;
        int knowledge = 0;
        PlayerCharacter player = GameController.Instance != null ? GameController.Instance.Player : null;
        if (player != null)
            knowledge = player.Knowledge;

        bool success = RuneRecipe.InscribeSucceeds(spell, knowledge);
        if (!success)
        {
            DestroyScroll();
            backlash = Burst(scroll);
            forecast = null;
            forecastScroll = null;
            if (panel != null)
                panel.Refresh();
            return true;
        }

        Item produced = recipe.CreateResult(slots);
        if (produced == null || !recipe.Spend(slots))
            return false;

        result = produced;
        forecast = null;
        forecastScroll = null;
        if (panel != null)
            panel.Refresh();
        return true;
    }

    public override IEnumerator AfterCraft()
    {
        if (backlash == null)
            yield break;

        IEnumerator effect = backlash;
        backlash = null;
        yield return effect;
    }

    void DestroyScroll()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] is Scroll)
                slots[i] = null;
        }
    }

    IEnumerator Burst(Scroll scroll)
    {
        if (scroll == null)
            yield break;

        ItemGenerator.EnsureEffects(scroll);
        int spell = scroll.SpellLevel < 1 ? 1 : scroll.SpellLevel;
        SpellBlast blast = SpawnBlast();
        if (blast != null)
            yield return blast.UntilGrown();
        Dungeon dungeon = GameController.Instance != null ? GameController.Instance.dungeon : null;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                Position2D tile = new Position2D { x = Position.x + dx, y = Position.y + dy };
                if (dungeon != null && !dungeon.TileExists(tile.x, tile.y))
                    continue;

                for (int i = 0; i < scroll.Effects.Count; i++)
                {
                    if (scroll.Effects[i] is not SkillEffect effect || effect.skill is not AttackSkill attack)
                        continue;
                    yield return attack.HitWithPower(tile, attack.Power(spell, 0));
                }
            }
        }
    }

    SpellBlast SpawnBlast()
    {
        Dungeon dungeon = GameController.Instance != null ? GameController.Instance.dungeon : null;
        if (blastPrefab == null || dungeon == null)
            return null;

        return Instantiate(blastPrefab, dungeon.GetTileWorldPosition(Position), Quaternion.identity);
    }

    bool Return(PlayerInventory inventory, Item item, Action<Item> clear)
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

    bool Consume(RuneRecipe recipe)
    {
        if (recipe.Inscribes)
            return recipe.Spend(slots);
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
        for (int i = 0; i < slots.Length && count > 0; i++)
        {
            Item slot = slots[i];
            if (slot == null || !UpgradeRecipe.SameId(slot.Id, id))
                continue;

            slots[i] = null;
            count--;
        }

        return count <= 0;
    }
}
