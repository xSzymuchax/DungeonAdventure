using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class VisionSystem : MonoBehaviour
{
    private int FindOctant(int dx, int dy)
    {
        bool steep = Mathf.Abs(dy) > Mathf.Abs(dx);

        if (!steep)
        {
            if (dx >= 0)
                return dy >= 0 ? 0 : 7;
            else
                return dy >= 0 ? 3 : 4;
        }
        else
        {
            if (dy >= 0)
                return dx >= 0 ? 1 : 2;
            else
                return dx >= 0 ? 6 : 5;
        }
    }
    public bool CanSee(Position2D source, Position2D target, int viewRange, Dungeon dungeon)
    {
        int octant = FindOctant(target.x - source.x, target.y - source.y);
        HashSet<Position2D> positions = new();

            LookInDirection(
                source.x, source.y, 1, 1.0, 0, viewRange,
                TRANSFORMATIONS[0, octant],
                TRANSFORMATIONS[1, octant],
                TRANSFORMATIONS[2, octant],
                TRANSFORMATIONS[3, octant],
                positions,
                dungeon.GetTileInfos());

        Debug.Log("seen tiles: " + positions.Count);            

        if (positions.Contains(target))
            return true;

        return false;
    }

    private readonly int[,] TRANSFORMATIONS =
    {
        {1,0,0,-1,-1,0,0,1 },
        {0,1,-1,0,0,-1,1,0 },
        {0,1,1,0,0,-1,-1,0 },
        {1,0,0,1,-1,0,0,-1 },
    };
    private void LookInDirection(
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
                        blocked = true;// zmiana
                        LookInDirection(x, y, i + 1, start, lSlope, radius, xx, xy, yx, yy, visible, tiles);
                        start = rSlope;
                    }
                }
            }

            if (blocked)
                break;
        }
    }

    
    public HashSet<Position2D> AllVisibleFields(Position2D from, int viewRange, Dungeon dungeon)
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
