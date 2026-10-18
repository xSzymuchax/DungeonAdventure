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

        PlaceForks(fields);
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

    struct IslandSpot
    {
        public int x;
        public int y;
        public int along;
        public int across;
        public int shift;
        public bool horizontal;
    }

    static void PlaceForks(FloorFieldType[,] fields)
    {
        int singles = Random.Range(2, 6);
        for (int i = 0; i < singles; i++)
        {
            if (!TryPlace(fields, 1, 1))
                break;
        }

        int larges = Random.Range(0, 3);
        for (int i = 0; i < larges; i++)
        {
            if (!TryPlaceLarge(fields))
                break;
        }
    }

    static bool TryPlaceLarge(FloorFieldType[,] fields)
    {
        List<int> alongs = new List<int>();
        List<int> acrosses = new List<int>();
        for (int along = 1; along <= 3; along++)
        {
            for (int across = 1; across <= 3; across++)
            {
                if (along == 1 && across == 1)
                    continue;
                if (FindSpots(fields, along, across).Count == 0)
                    continue;
                alongs.Add(along);
                acrosses.Add(across);
            }
        }

        if (alongs.Count == 0)
            return false;

        int pick = Random.Range(0, alongs.Count);
        return TryPlace(fields, alongs[pick], acrosses[pick]);
    }

    static bool TryPlace(FloorFieldType[,] fields, int along, int across)
    {
        List<IslandSpot> spots = FindSpots(fields, along, across);
        if (spots.Count == 0)
            return false;

        ApplySpot(fields, spots[Random.Range(0, spots.Count)]);
        return true;
    }

    static List<IslandSpot> FindSpots(FloorFieldType[,] fields, int along, int across)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        List<IslandSpot> spots = new List<IslandSpot>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (fields[x, y] != FloorFieldType.CORRIDOR_FIELD)
                    continue;

                for (int shift = 0; shift < across; shift++)
                {
                    if (Fits(fields, x, y, along, across, shift, true))
                        spots.Add(new IslandSpot { x = x, y = y, along = along, across = across, shift = shift, horizontal = true });
                    if (Fits(fields, x, y, along, across, shift, false))
                        spots.Add(new IslandSpot { x = x, y = y, along = along, across = across, shift = shift, horizontal = false });
                }
            }
        }

        return spots;
    }

    static bool Fits(FloorFieldType[,] fields, int x, int y, int along, int across, int shift, bool horizontal)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        Origin(x, y, along, across, shift, horizontal, out int x0, out int y0, out int aw, out int ah);
        if (x0 - 1 < 0 || y0 - 1 < 0 || x0 + aw >= width || y0 + ah >= height)
            return false;

        for (int i = 0; i < along; i++)
        {
            int cx = horizontal ? x + i : x;
            int cy = horizontal ? y : y + i;
            if (fields[cx, cy] != FloorFieldType.CORRIDOR_FIELD)
                return false;

            int leftX = horizontal ? cx : cx - 1;
            int leftY = horizontal ? cy - 1 : cy;
            int rightX = horizontal ? cx : cx + 1;
            int rightY = horizontal ? cy + 1 : cy;
            if (fields[leftX, leftY] == FloorFieldType.CORRIDOR_FIELD || fields[rightX, rightY] == FloorFieldType.CORRIDOR_FIELD)
                return false;
        }

        int anchorX = horizontal ? x - 1 : x;
        int anchorY = horizontal ? y : y - 1;
        int endX = horizontal ? x + along : x;
        int endY = horizontal ? y : y + along;
        if (fields[anchorX, anchorY] != FloorFieldType.CORRIDOR_FIELD || fields[endX, endY] != FloorFieldType.CORRIDOR_FIELD)
            return false;

        for (int ix = x0 - 1; ix <= x0 + aw; ix++)
        {
            for (int iy = y0 - 1; iy <= y0 + ah; iy++)
            {
                bool inside = ix >= x0 && ix < x0 + aw && iy >= y0 && iy < y0 + ah;
                FloorFieldType cell = fields[ix, iy];
                if (!inside)
                {
                    if (cell != FloorFieldType.EMPTY && cell != FloorFieldType.CORRIDOR_FIELD)
                        return false;
                    continue;
                }

                bool onRun = horizontal ? iy == y : ix == x;
                if (onRun && cell != FloorFieldType.CORRIDOR_FIELD)
                    return false;
                if (!onRun && cell != FloorFieldType.EMPTY)
                    return false;
            }
        }

        return true;
    }

    static void ApplySpot(FloorFieldType[,] fields, IslandSpot spot)
    {
        Origin(spot.x, spot.y, spot.along, spot.across, spot.shift, spot.horizontal, out int x0, out int y0, out int aw, out int ah);
        for (int ix = x0 - 1; ix <= x0 + aw; ix++)
        {
            for (int iy = y0 - 1; iy <= y0 + ah; iy++)
            {
                bool inside = ix >= x0 && ix < x0 + aw && iy >= y0 && iy < y0 + ah;
                if (inside)
                    fields[ix, iy] = FloorFieldType.SIDE_FIELD;
                else if (fields[ix, iy] == FloorFieldType.EMPTY)
                    fields[ix, iy] = FloorFieldType.CORRIDOR_FIELD;
            }
        }
    }

    static void Origin(int x, int y, int along, int across, int shift, bool horizontal, out int x0, out int y0, out int aw, out int ah)
    {
        if (horizontal)
        {
            x0 = x;
            y0 = y - shift;
            aw = along;
            ah = across;
        }
        else
        {
            x0 = x - shift;
            y0 = y;
            aw = across;
            ah = along;
        }
    }
}
