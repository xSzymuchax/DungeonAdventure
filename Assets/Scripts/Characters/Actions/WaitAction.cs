using System.Collections;
using UnityEngine;

public class WaitAction : IAction
{
    public double Cost => Consts.GAME_SPEED;

    readonly IActor actor;

    public WaitAction(IActor actor)
    {
        this.actor = actor;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("WaitAction");

        if (actor == null || !actor.HasEnergy)
            yield break;

        actor.RemoveEnergy(Cost);
        yield return null;
    }
}
