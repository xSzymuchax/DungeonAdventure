using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public static class VisionCalculator
{
    public static bool CanSee(Position2D source, Position2D target, int viewRange, Dungeon dungeon)
    {
        int x0 = source.x;
        int y0 = source.y;
        int x1 = target.x;
        int y1 = target.y;
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        if (Mathf.Max(dx, dy) > viewRange)
            return false;

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        FloorFieldType[,] floor = dungeon.GetFieldTypes();
        while (true)
        {
            if (floor[x0, y0] == FloorFieldType.SIDE_FIELD)
                return false;

            if (x0 == x1 && y0 == y1)
                break;

            int err2 = 2 * err;

            if (err2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (err2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return true;
    }


    private static readonly int[,] TRANSFORMATIONS =
    {
        {1,0,0,-1,-1,0,0,1 },
        {0,1,-1,0,0,-1,1,0 },
        {0,1,1,0,0,-1,-1,0 },
        {1,0,0,1,-1,0,0,-1 },
    };
    private static void LookInDirection(
        int x, int y, int row, double start, double end, 
        int radius, int xx, int xy, int yx, int yy, 
        HashSet<Position2D> visible, TileInfo[,] tiles)
    {
        if (start < end)
            return;

        double radiusSquared = radius * radius;
        int maxX = tiles.GetLength(0);
        int maxY = tiles.GetLength(1);

        for (int i = row; i <= radius; i++)
        {
            int dx = -i - 1;
            int dy = -i;

            bool blocked = false;

            while (dx <= 0)
            {
                dx++;

                int X = x + dx * xx + dy * xy;
                int Y = y + dx * yx + dy * yy;

                if (X<0 || Y<0 || X >= maxX || Y >= maxY) continue;
                if (tiles[X, Y] == null) continue;

                double lSlope = (dx - 0.5) / (dy + 0.5);
                double rSlope = (dx + 0.5) / (dy - 0.5);

                if (start < rSlope)
                    continue;

                if (end > lSlope)
                    break;

                if (dx * dx + dy * dy <= radiusSquared)
                    visible.Add(new Position2D() { x = X, y = Y });

                bool blocksVision = tiles[X, Y].type == FloorFieldType.SIDE_FIELD
                    ? true : false;

                if (blocked)
                {
                    if (blocksVision)
                    {
                        start = rSlope;
                        continue;
                    }
                    else
                    {
                        blocked = false;
                        start = rSlope;
                    }
                }
                else
                {
                    if (blocksVision && i < radius)
                    {
                        LookInDirection(x, y, i + 1, start, lSlope, radius, xx, xy, yx, yy, visible, tiles);
                        start = rSlope;
                    }
                }
            }

            if (blocked)
                break;
        }
    }
    public static HashSet<Position2D> AllVisibleFields(Position2D from, int viewRange, Dungeon dungeon)
    {
        HashSet<Position2D> visible = new();
        visible.Add(from);

        for (int i = 0; i < 8; i++)
        {
            LookInDirection(
                from.x, from.y, 1, 1.0, 0, viewRange,
                TRANSFORMATIONS[0, i],
                TRANSFORMATIONS[1, i],
                TRANSFORMATIONS[2, i],
                TRANSFORMATIONS[3, i],
                visible,
                dungeon.GetTileInfos());
        }

        return visible;
    }
}
