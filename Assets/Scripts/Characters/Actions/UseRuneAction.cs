using System.Collections;

public class UseRuneAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly int bagIndex;
    readonly Position2D target;

    public UseRuneAction(PlayerCharacter player, int bagIndex, Position2D target)
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
        if (player.Inventory.Bag[bagIndex] is not RuneStone rune || rune.Charge < 1f)
            yield break;
        if (!SpellMana.CanPay(player, rune))
            yield break;

        rune.Charge -= 1f;
        player.RemoveEnergy(Cost);
        player.RemoveMana(SpellMana.Cost(rune));
        yield return rune.UseRune(player, target);
        player.Inventory.Touch();
    }
}
