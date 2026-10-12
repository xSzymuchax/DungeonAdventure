public abstract class WorkbenchPenalty
{
    public abstract void Apply(Workbench bench, PlayerCharacter player);
}

public class DestroyPlacedItemPenalty : WorkbenchPenalty
{
    public override void Apply(Workbench bench, PlayerCharacter player)
    {
        if (bench != null)
            bench.DiscardSubject();
    }
}

public class LoseTurnPenalty : WorkbenchPenalty
{
    public override void Apply(Workbench bench, PlayerCharacter player)
    {
        if (player != null)
            player.RemoveEnergy(Consts.GAME_SPEED);
    }
}

public class ExplosionPenalty : WorkbenchPenalty
{
    public override void Apply(Workbench bench, PlayerCharacter player)
    {
        if (bench != null)
            bench.DiscardPlaced();
    }
}
