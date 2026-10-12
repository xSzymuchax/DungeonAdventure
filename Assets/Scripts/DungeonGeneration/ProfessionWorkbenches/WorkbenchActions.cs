using System.Collections;
using UnityEngine;

public class OpenWorkbenchAction : IAction
{
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly CraftBench bench;

    public OpenWorkbenchAction(PlayerCharacter player, CraftBench bench)
    {
        this.player = player;
        this.bench = bench;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || bench == null)
            yield break;
        if (player.Position.ChebyshevTo(bench.Position) != 1)
            yield break;

        player.RemoveEnergy(Cost);
        bench.Open(player);
        yield return null;
    }
}

public class ConfirmUpgradeAction : IAction
{
    public const float AnimationTime = 1f;
    public float Cost => Consts.GAME_SPEED;

    readonly PlayerCharacter player;
    readonly CraftBench bench;

    public ConfirmUpgradeAction(PlayerCharacter player, CraftBench bench)
    {
        this.player = player;
        this.bench = bench;
    }

    public IEnumerator PerformAction()
    {
        if (player == null || bench == null || !bench.CanCraft())
            yield break;

        bench.ClosePanel();
        player.RemoveEnergy(Cost);
        yield return new WaitForSeconds(AnimationTime);
        if (player.IsDead || !bench.Craft())
            yield break;

        yield return bench.AfterCraft();
        yield return null;
    }
}
