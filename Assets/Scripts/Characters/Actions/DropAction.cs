using System.Collections;
using UnityEngine;

public class DropAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly bool fromBag;
    readonly int bagIndex;
    readonly EquipmentSlot slot;

    public DropAction(PlayerCharacter player, int bagIndex)
    {
        this.player = player;
        fromBag = true;
        this.bagIndex = bagIndex;
    }

    public DropAction(PlayerCharacter player, EquipmentSlot slot)
    {
        this.player = player;
        fromBag = false;
        this.slot = slot;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("DropAction");

        if (player == null || player.Energy <= 0)
            yield break;
        if (!Take(out Item item))
            yield break;
        if (GameController.Instance == null || !GameController.Instance.TryDropItem(item))
        {
            Return(item);
            yield break;
        }

        player.RemoveEnergy(Cost);
        yield return null;
    }

    bool Take(out Item item)
    {
        return fromBag
            ? player.Inventory.TryTakeBag(bagIndex, out item)
            : player.Inventory.TryTakeEquipped(slot, out item);
    }

    void Return(Item item)
    {
        if (!fromBag && item is EquipmentItem equipment)
        {
            player.Inventory.PutEquipped(equipment);
            return;
        }

        player.Inventory.TryAdd(item);
    }
}
