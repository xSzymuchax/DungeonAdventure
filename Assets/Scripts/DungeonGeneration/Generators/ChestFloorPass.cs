using System.Collections.Generic;
using UnityEngine;

public struct ChestOrder
{
    public int x;
    public int y;
    public bool rollMaterial;
    public ChestMaterial material;
    public PadlockKind padlock;
    public int itemCount;
}

public static class ChestFloorPass
{
    public static void Place(FloorDraft draft, Position2D spawn, List<Room> lockedRooms, List<Position2D> reserved, List<ChestOrder> chests, List<Position2D> goldKeys)
    {
        MoveCostManager costs = MoveCostManager.CreateDefault();
        bool[,] reachable = draft.ReachableFrom(spawn, costs);
        List<Position2D> spots = new List<Position2D>();
        for (int x = 0; x < draft.Width; x++)
        {
            for (int y = 0; y < draft.Height; y++)
            {
                if (draft.Fields[x, y] != FloorFieldType.BASE_FIELD || !reachable[x, y])
                    continue;
                Position2D tile = new Position2D { x = x, y = y };
                if (Same(tile, spawn) || Contains(reserved, tile))
                    continue;
                spots.Add(tile);
            }
        }

        int count = Mathf.Min(Random.Range(0, 4), spots.Count);
        List<Position2D> used = new List<Position2D>();
        if (reserved != null)
            used.AddRange(reserved);
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, spots.Count);
            Position2D tile = spots[index];
            spots.RemoveAt(index);
            PadlockKind padlock = Padlock.RollChest();
            chests.Add(new ChestOrder
            {
                x = tile.x,
                y = tile.y,
                rollMaterial = false,
                material = Chest.RollMaterial(),
                padlock = padlock,
                itemCount = padlock == PadlockKind.Gold ? Random.Range(2, 4) : 1
            });
            used.Add(tile);
        }

        for (int i = 0; i < chests.Count; i++)
        {
            if (chests[i].padlock != PadlockKind.Gold)
                continue;
            Position2D door = new Position2D { x = chests[i].x, y = chests[i].y };
            if (TryKeyTile(draft, reachable, lockedRooms, door, used, out Position2D key))
            {
                goldKeys.Add(key);
                used.Add(key);
            }
        }
    }

    static bool TryKeyTile(FloorDraft draft, bool[,] reachable, List<Room> lockedRooms, Position2D chest, List<Position2D> used, out Position2D tile)
    {
        Position2D[] sides =
        {
            new Position2D { x = chest.x - 1, y = chest.y },
            new Position2D { x = chest.x + 1, y = chest.y },
            new Position2D { x = chest.x, y = chest.y - 1 },
            new Position2D { x = chest.x, y = chest.y + 1 }
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
        for (int x = 0; x < draft.Width; x++)
        {
            for (int y = 0; y < draft.Height; y++)
            {
                Position2D candidate = new Position2D { x = x, y = y };
                if (!KeyTile(draft, reachable, lockedRooms, used, candidate))
                    continue;

                int distance = Mathf.Abs(x - chest.x) + Mathf.Abs(y - chest.y);
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
        if (draft.Fields[tile.x, tile.y] == FloorFieldType.EXIT_FIELD)
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

    static bool Contains(List<Position2D> spots, Position2D tile)
    {
        if (spots == null)
            return false;
        for (int i = 0; i < spots.Count; i++)
        {
            if (spots[i].x == tile.x && spots[i].y == tile.y)
                return true;
        }

        return false;
    }

    static bool Same(Position2D a, Position2D b)
    {
        return a.x == b.x && a.y == b.y;
    }
}
