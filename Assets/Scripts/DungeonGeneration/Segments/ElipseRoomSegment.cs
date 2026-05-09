using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElipseRoomSegment : RoomSegment
{
    public int xRadius;
    public int yRadius;

    // (x-h)^2 / a^2 + (y-k)^2 / b^2 <= 1
    protected override List<FieldWithPosition2D> GenerateMyTiles()
    {
        List<FieldWithPosition2D> result = new();
        int centerX = x + xRadius;
        int centerY = y + yRadius;

        for (int i=x; i<= x+2*xRadius;i++)
            for (int j = y; j <= y + 2 * yRadius; j++)
            {
                float dx = (i - centerX) / (float)xRadius;
                float dy = (j - centerY) / (float)yRadius;

                if (dx * dx + dy * dy <= 1f)
                    result.Add(new(i, j, FloorFieldType.BASE_FIELD));
            }

        return result;
    }
}
