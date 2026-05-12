using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomSegment
{
    public int x;
    public int y;

    public List<Position2DWithField> GetTiles() { return GenerateMyTiles(); }

    protected abstract List<Position2DWithField> GenerateMyTiles();
}
