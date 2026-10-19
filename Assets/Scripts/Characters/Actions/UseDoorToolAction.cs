using System.Collections;

public class UseDoorToolAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly int bagIndex;
    readonly Position2D tile;

    public UseDoorToolAction(PlayerCharacter player, int bagIndex, Position2D tile)
    {
        this.player = player;
        this.bagIndex = bagIndex;
        this.tile = tile;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || player.Energy <= 0 || player.Inventory == null)
            yield break;
        if (bagIndex < 0 || bagIndex >= player.Inventory.Bag.Count)
            yield break;

        GameController controller = GameController.Instance;
        if (controller == null || controller.dungeon == null)
            yield break;
        if (player.Position.ChebyshevTo(tile) != 1)
            yield break;

        TileInfo info = controller.dungeon.GetTileInfos()[tile.x, tile.y];
        if (info == null || info.tileObject is not Door door)
            yield break;

        Item held = player.Inventory.Bag[bagIndex];
        if (DoorItems.IsSteelKey(held))
        {
            if (!door.TrySteelKey())
                yield break;
            if (!player.Inventory.TryTakeOne(bagIndex, out Item taken))
                yield break;
            player.RemoveEnergy(Cost);
            yield break;
        }

        if (DoorItems.IsLockpick(held) && door.AcceptsLockpick())
        {
            if (!player.Inventory.TryTakeOne(bagIndex, out Item taken))
                yield break;
            player.RemoveEnergy(Cost);
            controller.ShowLockPuzzle(door);
        }
    }
}
