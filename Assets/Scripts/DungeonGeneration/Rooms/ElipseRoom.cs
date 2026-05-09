using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElipseRoom : Room
{
    public override Vector2Int Center()
    {
        ElipseRoomSegment roomSegment = segmentsList[0] as ElipseRoomSegment;
        return new(roomSegment.x + roomSegment.xRadius, roomSegment.y + roomSegment.yRadius);
    }
}
