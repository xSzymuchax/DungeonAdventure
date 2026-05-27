using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : IAction
{
    public double Cost { get => _cost; }


    protected double _cost;

    private readonly IWalkable walker;
    private Position2D to;
    private readonly Dungeon dungeon;
    public MoveAction(IWalkable walker, Position2D to, Dungeon dungeon)
    {
        _cost = walker.WalkCost;
        this.to = to;
        this.walker = walker;
        this.dungeon = dungeon;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("MoveAction");

        Position2D start = dungeon.FindActorPosition(walker);
        if (start.x == -1)
        {
            Debug.Log("BAD POSITION");
            yield break;
        }

        if (dungeon.GetTileInfos()[to.x,to.y].isOccupied)
        {
            yield break;
        }    

        walker.RemoveEnergy(Cost);
        walker.Position = this.to;
        Vector3 target = dungeon.MoveActor(walker, to);
        yield return null;
    }
}
