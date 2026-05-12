using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectRoomSegment : RoomSegment
{
    public int width;
    public int height;

    protected override List<Position2DWithField> GenerateMyTiles()
    {
        List<Position2DWithField> result = new();

        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                result.Add(new()
                {
                    x = i,
                    y = j,
                    fieldType = FloorFieldType.BASE_FIELD
                });

        return result;
    }
}
