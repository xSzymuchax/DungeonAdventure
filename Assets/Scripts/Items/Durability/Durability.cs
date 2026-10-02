using UnityEngine;

public static class Durability
{
    public static void OnWeaponAttack(PlayerCharacter player)
    {
        if (player == null || Random.value >= Consts.WEAPON_WEAR_CHANCE)
            return;

        WearEquipped(player, EquipmentSlot.Weapon);
    }

    public static void OnPlayerHit(PlayerCharacter player)
    {
        if (player == null)
            return;

        WearSlot(player, EquipmentSlot.Helmet);
        WearSlot(player, EquipmentSlot.Armor);
        WearSlot(player, EquipmentSlot.Shield);
    }

    public static void RollBag(PlayerCharacter player)
    {
        if (player == null || player.Inventory == null || Random.value >= Consts.BAG_WEAR_CHANCE)
            return;

        Item picked = player.Inventory.RandomBagPiece();
        if (picked == null)
            return;

        WearBagItem(player, picked);
    }

    public static void WearBagItem(PlayerCharacter player, Item stack)
    {
        WearPiece(player, stack);
    }

    public static int RepairAmount(Item item)
    {
        if (item == null)
            return 0;

        float portion = Random.Range(0.1f, 0.3f);
        return Mathf.Max(1, Mathf.RoundToInt(item.MaxDurability * portion));
    }

    public static bool RepairPiece(PlayerCharacter player, int bagIndex)
    {
        if (player == null || !player.Inventory.TryTakeOne(bagIndex, out Item piece))
            return false;
        if (piece.Durability >= piece.MaxDurability)
        {
            player.Inventory.TryAdd(piece);
            return false;
        }

        piece.Durability = Mathf.Min(piece.MaxDurability, piece.Durability + RepairAmount(piece));
        if (!player.Inventory.TryAdd(piece) && GameController.Instance != null)
            GameController.Instance.TryDropItem(piece);
        return true;
    }

    public static bool RepairEquipped(PlayerCharacter player, EquipmentSlot slot)
    {
        Item item = player == null ? null : player.Inventory.Equipped(slot);
        if (item == null || item.Durability >= item.MaxDurability)
            return false;

        item.Durability = Mathf.Min(item.MaxDurability, item.Durability + RepairAmount(item));
        player.Inventory.Touch();
        return true;
    }

    static void WearSlot(PlayerCharacter player, EquipmentSlot slot)
    {
        if (player.Inventory.Equipped(slot) == null || Random.value >= Consts.ARMOR_WEAR_CHANCE)
            return;

        WearEquipped(player, slot);
    }

    static void WearEquipped(PlayerCharacter player, EquipmentSlot slot)
    {
        Item item = player.Inventory.Equipped(slot);
        if (item == null)
            return;

        item.Durability--;
        if (item.Durability <= 0)
            player.Inventory.DestroyEquipped(slot);
        else
            player.Inventory.Touch();
    }

    static void WearPiece(PlayerCharacter player, Item stack)
    {
        if (stack.Count <= 1)
        {
            stack.Durability--;
            if (stack.Durability <= 0)
                player.Inventory.RemoveBagItem(stack);
            else
                player.Inventory.Touch();
            return;
        }

        stack.Count--;
        Item piece = stack.Copy();
        piece.Count = 1;
        piece.Durability = stack.Durability - 1;
        if (piece is RuneStone worn)
            worn.Charge = 0f;
        player.Inventory.Touch();
        if (piece.Durability <= 0)
            return;
        if (!player.Inventory.TryAdd(piece) && GameController.Instance != null)
            GameController.Instance.TryDropItem(piece);
    }
}
