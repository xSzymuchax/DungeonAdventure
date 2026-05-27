using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum MovementDirections
{
    FOUR, EIGHT
}

public static class AStar
{
    private static List<Position2D> GetPath(Position2D?[,] parentsArray, Position2D from)
    {
        List<Position2D> result = new();
        Position2D? current = from;
        
        while (current != null)
        {
            result.Add(current.Value);
            current = parentsArray[current.Value.x, current.Value.y];
        }

        result.Reverse();
        return result;
    }
    private static double CalculateChebyshevDistance(Position2D current, Position2D end)
    {
        int dx = Math.Abs(current.x - end.x);
        int dy = Math.Abs(current.y - end.y);

        return Math.Max(dx, dy);
    }
    private static double CalculateManhattanDistance(Position2D current, Position2D end)
    {
        int dx = Math.Abs(current.x - end.x);
        int dy = Math.Abs(current.y - end.y);

        return (dx + dy);
    }
    private static List<Position2D> FourDirectionalMovement(Position2D current, int width, int heigth)
    {
        List<Position2D> result = new();
        List<Vector2Int> possibleMoves = new() { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };

        foreach (Vector2Int m in possibleMoves)
        {
            int x = current.x + m.x;
            int y = current.y + m.y;

            if (x < 0 || y < 0 || x >= width || y >= heigth || (current.x == x && current.y == y))
                continue;
            result.Add(new() { x = x, y = y });
        }
        return result;
    }
    private static List<Position2D> EightDirectionalMovement(Position2D current, int width, int heigth)
    {
        List<Position2D> result = new();
        for (int x = current.x - 1; x <= current.x + 1; x++)
            for (int y = current.y - 1; y <= current.y + 1; y++)
            {
                if (x < 0 || y < 0 || x >= width || y >= heigth || (current.x == x && current.y == y))
                    continue;
                result.Add(new() { x = x, y = y });
            }
        return result;
    }

    public static List<Position2D> FindPath(FloorFieldType[,] fields, Position2D start, Position2D end, MoveCostManager moveCostManager, MovementDirections movementDirectionsAmount)
    {
        int width = fields.GetLength(0);
        int heigth = fields.GetLength(1);
        
        Position2D?[,] parentsArray = new Position2D?[width, heigth];
        bool[,] visitedArray = new bool[width, heigth];
        double[,] costArray = new double[width, heigth];
        
        for (int i = 0; i < width; i++)
            for (int j = 0; j < heigth; j++)
                costArray[i, j] = double.MaxValue;
        costArray[start.x, start.y] = 0;

        List<Position2DWithPriority> openSet = new();

        double heuristic;
        if (movementDirectionsAmount == MovementDirections.EIGHT)
            heuristic = CalculateChebyshevDistance(start, end);
        else
            heuristic = CalculateManhattanDistance(start, end);

        Position2DWithPriority pwp = new()
        {
            x = start.x,
            y = start.y,
            priority = heuristic
        };
        openSet.Add(pwp);


        Position2DWithPriority current;
        while (openSet.Count > 0)
        {
            openSet.Sort((a, b) => a.priority.CompareTo(b.priority));
            current = openSet[0];
            openSet.RemoveAt(0);

            if (visitedArray[current.x, current.y])
                continue;
            visitedArray[current.x, current.y] = true;

            if (current.x == end.x && current.y == end.y)
            {
                if (costArray[end.x, end.y] == int.MaxValue)
                    return new();

                return GetPath(parentsArray, end);
            }
                

            List<Position2D> moves;
            if (movementDirectionsAmount == MovementDirections.EIGHT)
                moves = EightDirectionalMovement(new() { x=current.x, y=current.y}, width, heigth);
            else
                moves = FourDirectionalMovement(new() { x = current.x, y = current.y }, width, heigth);

            foreach (Position2D p in moves)
            {
                int x = p.x;
                int y = p.y;

                if (visitedArray[x, y])
                    continue;

                

                double moveCost = moveCostManager.GetCost(fields[x, y]);
                bool diagonal = current.x != x && current.y != y;

                if (diagonal) moveCost *= Math.Sqrt(2);

                double newCost = costArray[current.x, current.y] + moveCost;

                if (newCost < costArray[x, y])
                {
                    costArray[x, y] = newCost;
                    parentsArray[x, y] = new() { x = current.x, y = current.y };
                    if (movementDirectionsAmount == MovementDirections.EIGHT)
                        heuristic = CalculateChebyshevDistance(p, end);
                    else
                        heuristic = CalculateManhattanDistance(p, end);
                    double priority = newCost + heuristic;

                    openSet.Add(new Position2DWithPriority() { x = x, y = y, priority = priority });
                }
            }
        }

        return new(); // empty -> path not found
    }


    public static List<Position2D> FindClosestPath(TileInfo[,] fields, Position2D start, Position2D end, MoveCostManager moveCostManager, MovementDirections movementDirectionsAmount)
    {
        int width = fields.GetLength(0);
        int heigth = fields.GetLength(1);

        Position2D?[,] parentsArray = new Position2D?[width, heigth];
        bool[,] visitedArray = new bool[width, heigth];
        double[,] costArray = new double[width, heigth];

        for (int i = 0; i < width; i++)
            for (int j = 0; j < heigth; j++)
                costArray[i, j] = double.MaxValue;
        costArray[start.x, start.y] = 0;

        List<Position2DWithPriority> openSet = new();

        double heuristic;
        if (movementDirectionsAmount == MovementDirections.EIGHT)
            heuristic = CalculateChebyshevDistance(start, end);
        else
            heuristic = CalculateManhattanDistance(start, end);

        Position2DWithPriority pwp = new()
        {
            x = start.x,
            y = start.y,
            priority = heuristic
        };
        openSet.Add(pwp);

        Position2D bestField = start;
        double bestHeuristic = double.MaxValue;

        Position2DWithPriority current;
        while (openSet.Count > 0)
        {
            openSet.Sort((a, b) => a.priority.CompareTo(b.priority));
            current = openSet[0];
            openSet.RemoveAt(0);

            double currentHeuristic =
                movementDirectionsAmount == MovementDirections.EIGHT
                    ? CalculateChebyshevDistance(new() { x = current.x, y = current.y }, end)
                    : CalculateManhattanDistance(new() { x = current.x, y = current.y }, end);

            if (currentHeuristic < bestHeuristic)
            {
                bestHeuristic = currentHeuristic;
                bestField = new() { x = current.x, y = current.y };
            }

            if (visitedArray[current.x, current.y])
                continue;
            visitedArray[current.x, current.y] = true;

            if (current.x == end.x && current.y == end.y)
            {
                if (costArray[end.x, end.y] == int.MaxValue)
                    return new();

                return GetPath(parentsArray, end);
            }


            List<Position2D> moves;
            if (movementDirectionsAmount == MovementDirections.EIGHT)
                moves = EightDirectionalMovement(new() { x = current.x, y = current.y }, width, heigth);
            else
                moves = FourDirectionalMovement(new() { x = current.x, y = current.y }, width, heigth);

            foreach (Position2D p in moves)
            {
                int x = p.x;
                int y = p.y;

                if (visitedArray[x, y])
                    continue;

                if (fields[x, y] == null)
                    continue;

                double moveCost = moveCostManager.GetCost(fields[x, y].type);
                if (fields[x, y].isOccupied && x!=end.x && y!=end.y)
                    moveCost = double.MaxValue;

                bool diagonal = current.x != x && current.y != y;

                if (diagonal) moveCost *= Math.Sqrt(2);

                double newCost = costArray[current.x, current.y] + moveCost;

                if (newCost < costArray[x, y])
                {
                    costArray[x, y] = newCost;
                    parentsArray[x, y] = new() { x = current.x, y = current.y };
                    if (movementDirectionsAmount == MovementDirections.EIGHT)
                        heuristic = CalculateChebyshevDistance(p, end);
                    else
                        heuristic = CalculateManhattanDistance(p, end);
                    double priority = newCost + heuristic;

                    openSet.Add(new Position2DWithPriority() { x = x, y = y, priority = priority });
                }
            }
        }

        return GetPath(parentsArray, bestField); // empty -> path not found
    }

}
