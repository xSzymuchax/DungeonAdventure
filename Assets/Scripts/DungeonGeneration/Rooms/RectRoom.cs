using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectRoom : Room
{
    public override Vector2Int Center()
    {
        RectRoomSegment roomSegment = segmentsList[0] as RectRoomSegment;
        return new(roomSegment.x + roomSegment.width/2, roomSegment.y + roomSegment.height / 2);
    }

    public override List<FieldWithPosition2D> GetCoveredFields()
    {
        List<FieldWithPosition2D> result = new();
        RectRoomSegment roomSegment = segmentsList[0] as RectRoomSegment;

        for (int i=roomSegment.x;i<roomSegment.x + roomSegment.width; i++)
            for (int j=roomSegment.y;j<roomSegment.y + roomSegment.height; j++)
            {
                if (i == roomSegment.x || i == roomSegment.x + roomSegment.width-1 || 
                    j == roomSegment.y || j == roomSegment.y + roomSegment.height-1)
                    result.Add(new(i, j, FloorFieldType.SIDE_FIELD));
                else
                    result.Add(new(i, j, FloorFieldType.BASE_FIELD));
            }

        return result;
    }
}
