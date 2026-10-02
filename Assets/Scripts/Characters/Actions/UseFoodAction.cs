using System.Collections;

public class UseFoodAction : IAction
{
    public float Cost { get; }

    readonly PlayerCharacter player;
    readonly int bagIndex;

    public UseFoodAction(PlayerCharacter player, int bagIndex)
    {
        this.player = player;
        this.bagIndex = bagIndex;
        Cost = Consts.GAME_SPEED;
        if (player == null || bagIndex < 0 || bagIndex >= player.Inventory.Bag.Count)
            return;
        if (player.Inventory.Bag[bagIndex] is Food food && food.TimeToEat > 0)
            Cost = food.TimeToEat * Consts.GAME_SPEED;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || player.Energy <= 0)
            yield break;
        if (!player.Inventory.TryTakeOne(bagIndex, out Item taken) || taken is not Food food)
        {
            if (taken != null)
                player.Inventory.TryAdd(taken);
            yield break;
        }

        food.Eat(player);
        player.RemoveEnergy(Cost);
        yield return null;
    }
}
