using System.Collections;
using UnityEngine;

public class UnequipAction : IAction
{
    public double Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly EquipmentSlot slot;

    public UnequipAction(PlayerCharacter player, EquipmentSlot slot)
    {
        this.player = player;
        this.slot = slot;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("UnequipAction");

        if (player == null || player.Energy <= 0)
            yield break;
        if (!player.Inventory.TryUnequip(slot))
            yield break;

        player.RemoveEnergy(Cost);
        yield return null;
    }
}
