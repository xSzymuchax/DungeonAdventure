using System.Collections;
using System.Collections.Generic;
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
}
