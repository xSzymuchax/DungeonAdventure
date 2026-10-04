using System.Collections;

public class UseScrollAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly int bagIndex;
    readonly Position2D target;

    public UseScrollAction(PlayerCharacter player, int bagIndex, Position2D target)
    {
        this.player = player;
        this.bagIndex = bagIndex;
        this.target = target;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || player.Energy <= 0)
            yield break;
        if (bagIndex < 0 || bagIndex >= player.Inventory.Bag.Count)
            yield break;

        ItemGenerator.EnsureEffects(player.Inventory.Bag[bagIndex]);
        if (player.Inventory.Bag[bagIndex] is not Scroll ready || ready.Effects.Count == 0)
            yield break;
        if (!SpellMana.CanPay(player, ready))
            yield break;
        if (!player.Inventory.TryTakeOne(bagIndex, out Item taken) || taken is not Scroll scroll)
        {
            if (taken != null)
                player.Inventory.TryAdd(taken);
            yield break;
        }

        player.RemoveEnergy(Cost);
        player.RemoveMana(SpellMana.Cost(scroll));
        yield return scroll.UseScroll(player, target);
    }
}
