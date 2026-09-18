using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public struct Position2D : IEquatable<Position2D>
{
    public int x;
    public int y;

    public bool Equals(Position2D other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        return obj is Position2D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ (y.GetHashCode() << 2);
    }

    public static bool operator ==(Position2D a, Position2D b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Position2D a, Position2D b)
    {
        return !a.Equals(b);
    }

    public int ChebyshevTo(Position2D other)
    {
        return Math.Max(Math.Abs(x - other.x), Math.Abs(y - other.y));
    }

    public List<Position2D> LineTo(Position2D other)
    {
        List<Position2D> result = new();
        int x0 = x;
        int y0 = y;
        int x1 = other.x;
        int y1 = other.y;
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            result.Add(new Position2D { x = x0, y = y0 });
            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return result;
    }
}
