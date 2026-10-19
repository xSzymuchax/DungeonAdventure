using System.Collections.Generic;
using UnityEngine;

public struct DoorOrder
{
    public int x;
    public int y;
    public bool rollMaterial;
    public DoorMaterial material;
    public PadlockKind padlock;
}

public static class DoorFloorPass
{
    public static void SealRooms(FloorDraft draft, List<DoorOrder> orders, List<Room> lockedRooms, List<Position2D> lockedDoors)
    {
        List<Room> candidates = new List<Room>();
        for (int i = 1; i < draft.Rooms.Count; i++)
        {
            if (draft.Rooms[i].GetDoorMarkerGlobalPositions().Count > 0)
                candidates.Add(draft.Rooms[i]);
        }

        int want = Mathf.Min(Random.Range(1, 3), candidates.Count);
        for (int i = 0; i < want; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Room room = candidates[index];
            candidates.RemoveAt(index);
            if (!room.TryKeepOneDoor(out Position2D kept))
                continue;

            WriteRoomDoors(draft, room);
            lockedRooms.Add(room);
            lockedDoors.Add(kept);
            orders.Add(new DoorOrder
            {
                x = kept.x,
                y = kept.y,
                rollMaterial = false,
                material = DoorMaterial.Steel,
                padlock = Padlock.RollLocked()
            });
        }
    }

    public static void PlaceNasty(FloorDraft draft, List<Position2D> lockedDoors, List<DoorOrder> orders)
    {
        List<Position2D> candidates = new List<Position2D>();
        FloorFieldType[,] fields = draft.Fields;
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fields[x, y] != FloorFieldType.POSSIBLE_DOOR_FIELD)
                    continue;

                Position2D door = new Position2D { x = x, y = y };
                if (Contains(lockedDoors, door))
                    continue;
                if (CanBypass(fields, door))
                    candidates.Add(door);
            }
        }

        if (candidates.Count == 0)
            return;

        Position2D picked = candidates[Random.Range(0, candidates.Count)];
        orders.Add(new DoorOrder
        {
            x = picked.x,
            y = picked.y,
            rollMaterial = true,
            padlock = PadlockKind.Nasty
        });
    }

    public static void PlaceKeys(FloorDraft draft, Position2D spawn, List<Room> lockedRooms, List<Position2D> lockedDoors, List<DoorOrder> orders, List<Position2D> keys)
    {
        MoveCostManager costs = MoveCostManager.CreateDefault();
        bool[,] reachable = draft.ReachableFrom(spawn, costs);
        for (int i = 0; i < lockedRooms.Count && i < lockedDoors.Count; i++)
        {
            if (!HasSteelLock(orders, lockedDoors[i]))
                continue;
            if (TryKeyTile(draft, reachable, lockedRooms, lockedDoors[i], keys, out Position2D tile))
                keys.Add(tile);
        }
    }

    static bool HasSteelLock(List<DoorOrder> orders, Position2D door)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].x == door.x && orders[i].y == door.y)
                return orders[i].padlock == PadlockKind.Steel;
        }

        return false;
    }

    static bool TryKeyTile(FloorDraft draft, bool[,] reachable, List<Room> lockedRooms, Position2D door, List<Position2D> used, out Position2D tile)
    {
        Position2D[] sides =
        {
            new Position2D { x = door.x - 1, y = door.y },
            new Position2D { x = door.x + 1, y = door.y },
            new Position2D { x = door.x, y = door.y - 1 },
            new Position2D { x = door.x, y = door.y + 1 }
        };

        for (int i = 0; i < sides.Length; i++)
        {
            if (KeyTile(draft, reachable, lockedRooms, used, sides[i]))
            {
                tile = sides[i];
                return true;
            }
        }

        int best = int.MaxValue;
        tile = default;
        bool found = false;
        int width = draft.Width;
        int height = draft.Height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Position2D candidate = new Position2D { x = x, y = y };
                if (!KeyTile(draft, reachable, lockedRooms, used, candidate))
                    continue;

                int distance = Mathf.Abs(x - door.x) + Mathf.Abs(y - door.y);
                if (distance < best)
                {
                    best = distance;
                    tile = candidate;
                    found = true;
                }
            }
        }

        return found;
    }

    static bool KeyTile(FloorDraft draft, bool[,] reachable, List<Room> lockedRooms, List<Position2D> used, Position2D tile)
    {
        if (tile.x < 0 || tile.y < 0 || tile.x >= draft.Width || tile.y >= draft.Height)
            return false;
        if (!reachable[tile.x, tile.y])
            return false;
        if (draft.Fields[tile.x, tile.y] == FloorFieldType.POSSIBLE_DOOR_FIELD)
            return false;
        if (Contains(used, tile))
            return false;

        for (int i = 0; i < lockedRooms.Count; i++)
        {
            if (draft.IsInsideRoom(lockedRooms[i], tile.x, tile.y))
                return false;
        }

        return true;
    }

    static bool CanBypass(FloorFieldType[,] fields, Position2D door)
    {
        bool across = PairBypassed(fields, door, new Position2D { x = door.x - 1, y = door.y }, new Position2D { x = door.x + 1, y = door.y });
        bool along = PairBypassed(fields, door, new Position2D { x = door.x, y = door.y - 1 }, new Position2D { x = door.x, y = door.y + 1 });
        return across || along;
    }

    static bool PairBypassed(FloorFieldType[,] fields, Position2D door, Position2D a, Position2D b)
    {
        if (!Walkable(fields, a) || !Walkable(fields, b))
            return false;

        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        bool[,] seen = new bool[width, height];
        Queue<Position2D> queue = new Queue<Position2D>();
        queue.Enqueue(a);
        seen[a.x, a.y] = true;
        while (queue.Count > 0)
        {
            Position2D current = queue.Dequeue();
            if (current.x == b.x && current.y == b.y)
                return true;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Position2D next = new Position2D { x = current.x + dx, y = current.y + dy };
                    if (next.x == door.x && next.y == door.y)
                        continue;
                    if (!Walkable(fields, next) || seen[next.x, next.y])
                        continue;

                    seen[next.x, next.y] = true;
                    queue.Enqueue(next);
                }
            }
        }

        return false;
    }

    static bool Walkable(FloorFieldType[,] fields, Position2D tile)
    {
        if (tile.x < 0 || tile.y < 0 || tile.x >= fields.GetLength(0) || tile.y >= fields.GetLength(1))
            return false;

        return MoveCostManager.CreateDefault().CanStep(fields[tile.x, tile.y]);
    }

    static void WriteRoomDoors(FloorDraft draft, Room room)
    {
        Vector2Int corner = room.GetTopLeftCorner();
        FloorFieldType[,] local = room.GetRoomFields();
        for (int x = 0; x < local.GetLength(0); x++)
        {
            for (int y = 0; y < local.GetLength(1); y++)
            {
                int gx = corner.x + x;
                int gy = corner.y + y;
                if (gx < 0 || gy < 0 || gx >= draft.Width || gy >= draft.Height)
                    continue;
                if (draft.Fields[gx, gy] == FloorFieldType.POSSIBLE_DOOR_FIELD || local[x, y] == FloorFieldType.POSSIBLE_DOOR_FIELD)
                    draft.Fields[gx, gy] = local[x, y];
            }
        }
    }

    static bool Contains(List<Position2D> spots, Position2D tile)
    {
        for (int i = 0; i < spots.Count; i++)
        {
            if (spots[i].x == tile.x && spots[i].y == tile.y)
                return true;
        }

        return false;
    }
}
