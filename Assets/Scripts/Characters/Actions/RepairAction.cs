using System.Collections;

public class RepairAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly bool fromBag;
    readonly int bagIndex;
    readonly EquipmentSlot slot;

    public RepairAction(PlayerCharacter player, int bagIndex)
    {
        this.player = player;
        fromBag = true;
        this.bagIndex = bagIndex;
    }

    public RepairAction(PlayerCharacter player, EquipmentSlot slot)
    {
        this.player = player;
        fromBag = false;
        this.slot = slot;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || player.Energy <= 0)
            yield break;

        bool repaired = fromBag
            ? Durability.RepairPiece(player, bagIndex)
            : Durability.RepairEquipped(player, slot);
        if (!repaired)
            yield break;

        player.RemoveEnergy(Cost);
        yield return null;
    }
}