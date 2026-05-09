using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Room
{
    protected List<RoomSegment> segmentsList = new();
    public abstract List<FieldWithPosition2D> GetCoveredFields();
    public abstract Vector2Int Center();
    public void AddSegment(RoomSegment roomSegment) { segmentsList.Add(roomSegment); }
    public List<RoomSegment> GetRoomSegments() { return segmentsList; }
}
