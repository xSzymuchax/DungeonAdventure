using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Room
{
    private bool isGenerated = false;
    private List<FieldWithPosition2D> roomFields = new();
    protected List<RoomSegment> segmentsList = new();

    public abstract Vector2Int Center();

    private void Generate()
    {
        foreach (RoomSegment rs in segmentsList)
            roomFields.AddRange(rs.GetTiles());
        isGenerated = true;
    }
    public List<FieldWithPosition2D> GetCoveredFields() 
    {
        if (!isGenerated)
            Generate();
        return roomFields;
    }
    public void AddSegment(RoomSegment roomSegment) { segmentsList.Add(roomSegment); }
}
