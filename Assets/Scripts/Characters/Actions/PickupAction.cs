using System.Collections;
using UnityEngine;

public class PickupAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly Position2D tile;
    readonly Dungeon dungeon;

    public PickupAction(PlayerCharacter player, Position2D tile, Dungeon dungeon)
    {
        this.player = player;
        this.tile = tile;
        this.dungeon = dungeon;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("PickupAction");

        if (player == null)
            yield break;
        if (!dungeon.TryGetGroundItem(tile, out Item item))
            yield break;
        if (!player.Inventory.TryAdd(item))
            yield break;

        player.RemoveEnergy(Cost);
        dungeon.RemoveGroundItem(tile, item);
        yield return null;
    }
}
