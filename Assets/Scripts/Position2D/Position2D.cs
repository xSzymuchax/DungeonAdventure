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
}
