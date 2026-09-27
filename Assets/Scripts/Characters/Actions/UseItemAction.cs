using System.Collections;
using UnityEngine;

public class UseItemAction : IAction
{
    public double Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly int bagIndex;

    public UseItemAction(PlayerCharacter player, int bagIndex)
    {
        this.player = player;
        this.bagIndex = bagIndex;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("UseItemAction");

        if (player == null || player.Energy <= 0)
            yield break;
        if (bagIndex < 0 || bagIndex >= player.Inventory.Bag.Count)
            yield break;
        if (player.Inventory.Bag[bagIndex].Kind != ItemKind.Consumable)
            yield break;
        if (!player.Inventory.TryTakeBag(bagIndex, out _))
            yield break;

        player.RemoveEnergy(Cost);
        yield return null;
    }
}
