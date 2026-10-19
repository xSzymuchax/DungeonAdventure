using System.Collections;

public enum ChestUse
{
    Open,
    Take,
    GoldKey
}

public class ChestAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly int bagIndex;
    readonly Position2D tile;
    readonly ChestUse use;

    public ChestAction(PlayerCharacter player, Position2D tile, ChestUse use, int bagIndex)
    {
        this.player = player;
        this.tile = tile;
        this.use = use;
        this.bagIndex = bagIndex;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || player.Energy <= 0)
            yield break;

        GameController controller = GameController.Instance;
        if (controller == null || controller.dungeon == null)
            yield break;
        if (player.Position.ChebyshevTo(tile) != 1)
            yield break;

        TileInfo info = controller.dungeon.GetTileInfos()[tile.x, tile.y];
        if (info == null || info.tileObject is not Chest chest)
            yield break;

        if (use == ChestUse.GoldKey)
        {
            if (player.Inventory == null || bagIndex < 0 || bagIndex >= player.Inventory.Bag.Count)
                yield break;
            if (!DoorItems.IsGoldKey(player.Inventory.Bag[bagIndex]) || !chest.TryGoldKey())
                yield break;
            if (!player.Inventory.TryTakeOne(bagIndex, out Item taken))
                yield break;
            player.RemoveEnergy(Cost);
            yield break;
        }

        if (use == ChestUse.Open)
        {
            if (chest.IsOpen || (chest.Lock != null && chest.Lock.BlocksPassage))
                yield break;
            chest.Open();
            player.RemoveEnergy(Cost);
            yield break;
        }

        if (!chest.Take(player.Inventory))
            yield break;
        player.RemoveEnergy(Cost);
    }
}
