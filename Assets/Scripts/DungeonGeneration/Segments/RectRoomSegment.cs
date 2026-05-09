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

        for (int i = x; i < x + width; i++)
            for (int j = y; j < y + height; j++)
            {
                if (i == x || i == x + width - 1 ||
                    j == y || j == y + height - 1)
                    result.Add(new(i, j, FloorFieldType.SIDE_FIELD));
                else
                    result.Add(new(i, j, FloorFieldType.BASE_FIELD));
            }

        return result;
    }
}
