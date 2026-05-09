using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomSegment
{
    public int x;
    public int y;

    public List<FieldWithPosition2D> GetTiles() { return GenerateMyTiles(); }

    protected abstract List<FieldWithPosition2D> GenerateMyTiles();
}
