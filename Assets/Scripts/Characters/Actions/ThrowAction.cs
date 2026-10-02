using System.Collections;
using UnityEngine;

public class ThrowAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly bool fromBag;
    readonly int bagIndex;
    readonly EquipmentSlot slot;
    readonly Position2D? target;

    public ThrowAction(PlayerCharacter player, int bagIndex, Position2D target)
    {
        this.player = player;
        fromBag = true;
        this.bagIndex = bagIndex;
        this.target = target;
    }

    public ThrowAction(PlayerCharacter player, EquipmentSlot slot, Position2D target)
    {
        this.player = player;
        fromBag = false;
        this.slot = slot;
        this.target = target;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("ThrowAction");

        if (player == null || player.Energy <= 0 || target == null || GameController.Instance == null)
            yield break;

        if (!Take(out Item item))
            yield break;

        ItemProjectile projectile = new(item, player);
        yield return projectile.Launch(player.Position, target.Value);
        if (!projectile.Landed)
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
            ? player.Inventory.TryTakeOne(bagIndex, out item)
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
