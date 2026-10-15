using System.Collections.Generic;
using UnityEngine;

public class PossibleDoorMarkerModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int width = roomFields.GetLength(0);
        int height = roomFields.GetLength(1);
        int maxMarkerCount = DoorLimit(roomFields);
        bool[,] outside = Outside(roomFields);
        bool[,] main = LargestFloor(roomFields);
        List<Position2D> found = new List<Position2D>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CanHoldDoor(roomFields, outside, main, x, y))
                    found.Add(new Position2D { x = x, y = y });
            }
        }

        int amount = Random.Range(1, maxMarkerCount + 1);
        List<Position2D> placed = new List<Position2D>();
        while (placed.Count < amount && found.Count > 0)
        {
            int index = Random.Range(0, found.Count);
            Position2D candidate = found[index];
            found.RemoveAt(index);
            if (BesidePlacedDoor(candidate, placed))
                continue;

            roomFields[candidate.x, candidate.y] = FloorFieldType.POSSIBLE_DOOR_FIELD;
            placed.Add(candidate);
        }

        return roomFields;
    }

    static bool CanHoldDoor(FloorFieldType[,] fields, bool[,] outside, bool[,] main, int x, int y)
    {
        FloorFieldType cell = fields[x, y];
        if (cell != FloorFieldType.SIDE_FIELD && cell != FloorFieldType.BASE_FIELD)
            return false;
        if (!OnRoomEdge(fields, outside, x, y))
            return false;
        return JoinsMainFloor(fields, main, x, y);
    }

    static bool OnRoomEdge(FloorFieldType[,] fields, bool[,] outside, int x, int y)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
            return true;

        bool edge = false;
        VisitNeighbors(fields, x, y, (nx, ny) =>
        {
            if (outside[nx, ny])
                edge = true;
        });
        return edge;
    }

    static bool JoinsMainFloor(FloorFieldType[,] fields, bool[,] main, int x, int y)
    {
        if (fields[x, y] == FloorFieldType.BASE_FIELD)
            return main[x, y];

        bool joins = false;
        VisitNeighbors(fields, x, y, (nx, ny) =>
        {
            if (main[nx, ny])
                joins = true;
        });
        return joins;
    }

    static int DoorLimit(FloorFieldType[,] fields)
    {
        if (!FloorSpan(fields, out int spanX, out int spanY))
            return Consts.ROOM_DOOR_MAX;
        if (spanX >= Consts.LARGE_ROOM_SPAN && spanY >= Consts.LARGE_ROOM_SPAN)
            return Consts.LARGE_ROOM_DOOR_MAX;
        return Consts.ROOM_DOOR_MAX;
    }

    static bool FloorSpan(FloorFieldType[,] fields, out int spanX, out int spanY)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fields[x, y] != FloorFieldType.BASE_FIELD)
                    continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        spanX = maxX - minX + 1;
        spanY = maxY - minY + 1;
        return maxX >= 0;
    }

    static bool[,] Outside(FloorFieldType[,] fields)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        bool[,] outside = new bool[width, height];
        Queue<Position2D> queue = new Queue<Position2D>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fields[x, y] != FloorFieldType.EMPTY)
                    continue;
                if (x != 0 && y != 0 && x != width - 1 && y != height - 1)
                    continue;

                outside[x, y] = true;
                queue.Enqueue(new Position2D { x = x, y = y });
            }
        }

        while (queue.Count > 0)
        {
            Position2D current = queue.Dequeue();
            VisitNeighbors(fields, current.x, current.y, (x, y) =>
            {
                if (outside[x, y] || fields[x, y] != FloorFieldType.EMPTY)
                    return;
                outside[x, y] = true;
                queue.Enqueue(new Position2D { x = x, y = y });
            });
        }

        return outside;
    }

    static bool[,] LargestFloor(FloorFieldType[,] fields)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        bool[,] seen = new bool[width, height];
        bool[,] largest = new bool[width, height];
        int best = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (seen[x, y] || fields[x, y] != FloorFieldType.BASE_FIELD)
                    continue;

                bool[,] component = new bool[width, height];
                int count = 0;
                Queue<Position2D> queue = new Queue<Position2D>();
                queue.Enqueue(new Position2D { x = x, y = y });
                seen[x, y] = true;
                component[x, y] = true;

                while (queue.Count > 0)
                {
                    Position2D current = queue.Dequeue();
                    count++;
                    VisitNeighbors(fields, current.x, current.y, (nx, ny) =>
                    {
                        if (seen[nx, ny] || fields[nx, ny] != FloorFieldType.BASE_FIELD)
                            return;
                        seen[nx, ny] = true;
                        component[nx, ny] = true;
                        queue.Enqueue(new Position2D { x = nx, y = ny });
                    });
                }

                if (count > best)
                {
                    best = count;
                    largest = component;
                }
            }
        }

        return largest;
    }

    static void VisitNeighbors(FloorFieldType[,] fields, int x, int y, System.Action<int, int> visit)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        if (x > 0)
            visit(x - 1, y);
        if (x + 1 < width)
            visit(x + 1, y);
        if (y > 0)
            visit(x, y - 1);
        if (y + 1 < height)
            visit(x, y + 1);
    }

    static bool BesidePlacedDoor(Position2D candidate, List<Position2D> placedDoors)
    {
        foreach (Position2D door in placedDoors)
        {
            int dx = Mathf.Abs(candidate.x - door.x);
            int dy = Mathf.Abs(candidate.y - door.y);
            if (dx + dy == 1)
                return true;
        }

        return false;
    }
}
