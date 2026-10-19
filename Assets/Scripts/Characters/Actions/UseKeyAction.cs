using System.Collections;

public class UseKeyAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly int bagIndex;
    readonly Position2D tile;

    public UseKeyAction(PlayerCharacter player, int bagIndex, Position2D tile)
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
        if (info == null)
            yield break;

        Item held = player.Inventory.Bag[bagIndex];
        if (info.tileObject is Door door)
        {
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
                controller.ShowLockPuzzle(door.Lock);
            }
            yield break;
        }

        if (info.tileObject is not Chest chest)
            yield break;

        if (DoorItems.IsGoldKey(held))
        {
            if (!chest.TryGoldKey())
                yield break;
            if (!player.Inventory.TryTakeOne(bagIndex, out Item taken))
                yield break;
            player.RemoveEnergy(Cost);
            yield break;
        }

        if (DoorItems.IsLockpick(held) && chest.AcceptsLockpick())
        {
            if (!player.Inventory.TryTakeOne(bagIndex, out Item taken))
                yield break;
            player.RemoveEnergy(Cost);
            controller.ShowLockPuzzle(chest.Lock);
        }
    }
}
