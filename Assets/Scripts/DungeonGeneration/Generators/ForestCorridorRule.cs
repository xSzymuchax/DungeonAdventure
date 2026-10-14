using System.Collections.Generic;
using UnityEngine;

public class ForestCorridorRule : ICorridorRule
{
    public void Connect(FloorFieldType[,] fields, IReadOnlyList<Room> rooms)
    {
        if (fields == null || rooms == null || rooms.Count == 0)
            return;

        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        HashSet<int> connected = new HashSet<int>();
        connected.Add(0);

        while (connected.Count < rooms.Count)
        {
            float closest = float.MaxValue;
            int bestB = -1;
            Position2D bestMarkerA = default;
            Position2D bestMarkerB = default;

            foreach (int a in connected)
            {
                for (int b = 0; b < rooms.Count; b++)
                {
                    if (connected.Contains(b))
                        continue;

                    var marks = FindClosestMarkers(rooms[a], rooms[b]);
                    float dist = Vector2Int.Distance(new Vector2Int(marks.from.x, marks.from.y), new Vector2Int(marks.to.x, marks.to.y));
                    if (dist < closest)
                    {
                        closest = dist;
                        bestB = b;
                        bestMarkerA = marks.from;
                        bestMarkerB = marks.to;
                    }
                }
            }

            Carve(fields, new Vector2Int(bestMarkerA.x, bestMarkerA.y), new Vector2Int(bestMarkerB.x, bestMarkerB.y));
            connected.Add(bestB);
        }

        List<Position2D> notConnectedDoorMarkers = new List<Position2D>();
        List<Vector2Int> possibleMoves = new List<Vector2Int> { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };

        foreach (Position2D position in CollectDoorMarkers(rooms))
        {
            bool isCorridorNearby = false;
            foreach (Vector2Int move in possibleMoves)
            {
                int x = position.x + move.x;
                int y = position.y + move.y;
                if (x < 0 || y < 0 || x >= width || y >= height || (position.x == x && position.y == y))
                    continue;

                if (fields[x, y] == FloorFieldType.CORRIDOR_FIELD)
                    isCorridorNearby = true;
            }

            if (!isCorridorNearby)
                notConnectedDoorMarkers.Add(position);
        }

        while (notConnectedDoorMarkers.Count >= 2)
        {
            int a = Random.Range(0, notConnectedDoorMarkers.Count);
            int b = Random.Range(0, notConnectedDoorMarkers.Count);
            while (a == b)
                b = Random.Range(0, notConnectedDoorMarkers.Count);

            Carve(
                fields,
                new Vector2Int(notConnectedDoorMarkers[a].x, notConnectedDoorMarkers[a].y),
                new Vector2Int(notConnectedDoorMarkers[b].x, notConnectedDoorMarkers[b].y));

            if (a < b)
            {
                int swap = a;
                a = b;
                b = swap;
            }

            notConnectedDoorMarkers.RemoveAt(a);
            notConnectedDoorMarkers.RemoveAt(b);
        }
    }

    static List<Position2D> CollectDoorMarkers(IReadOnlyList<Room> rooms)
    {
        List<Position2D> markers = new List<Position2D>();
        for (int i = 0; i < rooms.Count; i++)
            markers.AddRange(rooms[i].GetDoorMarkerGlobalPositions());
        return markers;
    }

    static (Position2D from, Position2D to) FindClosestMarkers(Room roomA, Room roomB)
    {
        List<Position2D> markersA = roomA.GetDoorMarkerGlobalPositions();
        List<Position2D> markersB = roomB.GetDoorMarkerGlobalPositions();

        float bestDistance = float.MaxValue;
        Position2D bestA = default;
        Position2D bestB = default;

        foreach (Position2D markerA in markersA)
        {
            foreach (Position2D markerB in markersB)
            {
                float distance = Vector2Int.Distance(new Vector2Int(markerA.x, markerA.y), new Vector2Int(markerB.x, markerB.y));
                if (distance < bestDistance)
                {
                    bestA = markerA;
                    bestB = markerB;
                }
            }
        }

        return (bestA, bestB);
    }

    static void Carve(FloorFieldType[,] fields, Vector2Int a, Vector2Int b)
    {
        MoveCostManager moveCostManager = new MoveCostManager();
        moveCostManager.AddCost(FloorFieldType.EMPTY, 5);
        moveCostManager.AddCost(FloorFieldType.POSSIBLE_DOOR_FIELD, 0);
        moveCostManager.AddCost(FloorFieldType.CORRIDOR_FIELD, 4);
        moveCostManager.AddCost(FloorFieldType.BASE_FIELD, 50);
        moveCostManager.AddCost(FloorFieldType.SIDE_FIELD, 1000);

        var result = AStar.FindPath(
            fields,
            new Position2D { x = a.x, y = a.y },
            new Position2D { x = b.x, y = b.y },
            moveCostManager,
            MovementDirections.FOUR);

        foreach (var point in result)
        {
            if (fields[point.x, point.y] == FloorFieldType.EMPTY)
                fields[point.x, point.y] = FloorFieldType.CORRIDOR_FIELD;
        }
    }
}
