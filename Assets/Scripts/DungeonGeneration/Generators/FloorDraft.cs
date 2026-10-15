using System.Collections.Generic;
using UnityEngine;

public class FloorDraft
{
    public FloorFieldType[,] Fields { get; }
    public List<Room> Rooms { get; } = new List<Room>();
    public int Width => Fields.GetLength(0);
    public int Height => Fields.GetLength(1);

    public FloorDraft(int width, int height)
    {
        Fields = new FloorFieldType[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                Fields[x, y] = FloorFieldType.EMPTY;
    }

    public bool CanRoomFit(Room room)
    {
        int startX = room.GetTopLeftCorner().x;
        int startY = room.GetTopLeftCorner().y;
        FloorFieldType[,] roomFields = room.GetRoomFields();
        int roomWidth = roomFields.GetLength(0);
        int roomHeight = roomFields.GetLength(1);

        if (startX + roomWidth >= Width || startY + roomHeight >= Height || startX < 0 || startY < 0)
            return false;

        for (int i = 0; i < roomWidth; i++)
        {
            for (int j = 0; j < roomHeight; j++)
            {
                if (roomFields[i, j] == FloorFieldType.EMPTY)
                    continue;

                int globalX = i + startX;
                int globalY = j + startY;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int boundX = globalX + dx;
                        int boundY = globalY + dy;
                        if (boundX < 0 || boundY < 0 || boundX >= Width || boundY >= Height)
                            return false;
                        if (Fields[boundX, boundY] != FloorFieldType.EMPTY)
                            return false;
                    }
                }
            }
        }

        return true;
    }

    public void PlaceRoom(Room room)
    {
        int startX = room.GetTopLeftCorner().x;
        int startY = room.GetTopLeftCorner().y;
        FloorFieldType[,] roomFields = room.GetRoomFields();
        int roomWidth = roomFields.GetLength(0);
        int roomHeight = roomFields.GetLength(1);

        for (int i = 0; i < roomWidth; i++)
        {
            for (int j = 0; j < roomHeight; j++)
            {
                if (roomFields[i, j] == FloorFieldType.EMPTY)
                    continue;
                Fields[i + startX, j + startY] = roomFields[i, j];
            }
        }

        Rooms.Add(room);
    }

    public Position2D FindSpawnPoint(Room spawnRoom)
    {
        FloorFieldType[,] roomFields = spawnRoom.GetRoomFields();
        Vector2Int roomCorner = spawnRoom.GetTopLeftCorner();

        for (int i = 0; i < roomFields.GetLength(0); i++)
        {
            for (int j = 0; j < roomFields.GetLength(1); j++)
            {
                if (roomFields[i, j] == FloorFieldType.SPAWN_FIELD)
                    return new Position2D { x = i + roomCorner.x, y = j + roomCorner.y };
            }
        }

        return new Position2D { x = -1, y = -1 };
    }

    public bool IsInsideRoom(Room room, int x, int y)
    {
        Vector2Int corner = room.GetTopLeftCorner();
        FloorFieldType[,] roomFields = room.GetRoomFields();
        int localX = x - corner.x;
        int localY = y - corner.y;
        if (localX < 0 || localY < 0 || localX >= roomFields.GetLength(0) || localY >= roomFields.GetLength(1))
            return false;

        return roomFields[localX, localY] != FloorFieldType.EMPTY;
    }

    public bool[,] ReachableFrom(Position2D start, MoveCostManager moveCosts)
    {
        bool[,] reachable = new bool[Width, Height];
        if (start.x < 0 || start.y < 0 || start.x >= Width || start.y >= Height)
            return reachable;

        Queue<Position2D> queue = new Queue<Position2D>();
        queue.Enqueue(start);
        reachable[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            Position2D current = queue.Dequeue();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int x = current.x + dx;
                    int y = current.y + dy;
                    if (x < 0 || y < 0 || x >= Width || y >= Height || reachable[x, y])
                        continue;
                    if (moveCosts == null || !moveCosts.CanStep(Fields[x, y]))
                        continue;

                    reachable[x, y] = true;
                    queue.Enqueue(new Position2D { x = x, y = y });
                }
            }
        }

        return reachable;
    }
}
