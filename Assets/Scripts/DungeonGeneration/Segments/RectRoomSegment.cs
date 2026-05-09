using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectRoomSegment : RoomSegment
{
    public int width;
    public int height;

    protected override List<FieldWithPosition2D> GenerateMyTiles()
    {
        List<FieldWithPosition2D> result = new();

        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                 result.Add(new(i, j, FloorFieldType.BASE_FIELD));

        return result;
    }
}
