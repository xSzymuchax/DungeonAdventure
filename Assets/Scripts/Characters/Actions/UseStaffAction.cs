using System.Collections;

public class UseStaffAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly EquipmentSlot slot;
    readonly Position2D target;

    public UseStaffAction(PlayerCharacter player, EquipmentSlot slot, Position2D target)
    {
        this.player = player;
        this.slot = slot;
        this.target = target;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || player.Energy <= 0)
            yield break;

        StaffItem staff = player.Inventory.Equipped(slot) as StaffItem;
        StaffSpell spell = staff != null ? staff.Spell : null;
        if (spell == null || spell.Charge < 1f || !SpellMana.CanPay(player, staff))
            yield break;

        spell.Charge -= 1f;
        player.RemoveEnergy(Cost);
        player.RemoveMana(SpellMana.Cost(staff));
        yield return spell.Use(player, target);
        player.Inventory.Touch();
    }
}
