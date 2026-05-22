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

        walker.RemoveEnergy(Cost);
        Vector3 target = dungeon.MoveActor(walker, to);
            
        yield return walker.WalkingAnimation(target);
    }

    //public IEnumerator PerformAction()
    //{
    //    Debug.Log("MoveAction");

    //    Position2D start = dungeon.FindActorPosition(character);
    //    if (start.x == -1)
    //    {
    //        Debug.Log("BAD POSITION");
    //        yield break;
    //    }

    //    MoveCostManager moveCostManager = (character as Character).GetMoveCosts();
    //    List<Position2D> path = AStar.FindPath(dungeon.GetFieldTypes(), start, to, moveCostManager, MovementDirections.EIGHT);
    //    Character c = character as Character;

    //    foreach (Position2D p in path)
    //    {
    //        //if (!character.HasEnergy)
    //        //    yield break;

    //        character.RemoveEnergy(Cost);
    //        Vector3 target = dungeon.MoveActor(character, p);

    //        yield return c.StartCoroutine(c.WalkingAnimation(target));
    //    }
    //}
}
