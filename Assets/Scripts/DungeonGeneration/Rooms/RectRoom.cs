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
}
