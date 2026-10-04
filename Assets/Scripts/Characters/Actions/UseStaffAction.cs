using System.Collections;

public class UseStaffAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly bool fromBag;
    readonly int bagIndex;
    readonly EquipmentSlot slot;
    readonly Position2D target;

    public UseStaffAction(PlayerCharacter player, int bagIndex, Position2D target)
    {
        this.player = player;
        fromBag = true;
        this.bagIndex = bagIndex;
        this.target = target;
    }

    public UseStaffAction(PlayerCharacter player, EquipmentSlot slot, Position2D target)
    {
        this.player = player;
        fromBag = false;
        this.slot = slot;
        this.target = target;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || player.Energy <= 0)
            yield break;

        StaffSpell spell = Find()?.Spell;
        if (spell == null || spell.Charge < 1f)
            yield break;

        spell.Charge -= 1f;
        player.RemoveEnergy(Cost);
        yield return spell.Use(player, target);
        player.Inventory.Touch();
    }

    StaffItem Find()
    {
        if (fromBag)
        {
            if (bagIndex < 0 || bagIndex >= player.Inventory.Bag.Count)
                return null;
            return player.Inventory.Bag[bagIndex] as StaffItem;
        }

        return player.Inventory.Equipped(slot) as StaffItem;
    }
}
