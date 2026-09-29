using System.Collections;
using UnityEngine;

public class EquipAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly int bagIndex;

    public EquipAction(PlayerCharacter player, int bagIndex)
    {
        this.player = player;
        this.bagIndex = bagIndex;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("EquipAction");

        if (player == null || player.Energy <= 0)
            yield break;
        if (!player.Inventory.TryEquipFromBag(bagIndex))
            yield break;

        player.RemoveEnergy(Cost);
        yield return null;
    }
}
