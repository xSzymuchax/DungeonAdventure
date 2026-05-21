using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : IAction
{
    public double Cost { get => _cost; }
    protected double _cost;

    private IActor character;
    private Position2D to;
    private Dungeon dungeon;
    public MoveAction(double cost, IActor character, Position2D to, Dungeon dungeon)
    {
        _cost = cost;
        this.to = to;
        this.character = character;
        this.dungeon = dungeon;
    }

    public void PerformAction()
    {
        Debug.Log("MoveAction");

        Position2D start = dungeon.FindActorPosition(character);
        if (start.x == -1)
        {
            Debug.Log("BAD POSITION");
            return;
        }

        MoveCostManager moveCostManager = (character as Character).GetMoveCosts();
        List<Position2D> path = AStar.FindPath(dungeon.GetFieldTypes(), start, to, moveCostManager, MovementDirections.EIGHT);

        foreach (Position2D p in path)
        {
            if (!character.HasEnergy)
                return;

            character.RemoveEnergy(Cost);
            dungeon.MoveActor(character, p);
        }
    }
}
