using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Room
{
    private bool isGenerated = false;
    private FloorFieldType[,] fields;
    private Vector2Int topLeftCorner;

    protected List<RoomSegment> segmentsList = new();


    public abstract Vector2Int Center();

    protected void Generate()
    {
        var localTiles = ConvertSegmentsTileCordsToRoomCords();
        var boundingBox = FindRoomBoundingBox(localTiles);

        foreach (var t in localTiles)
            Debug.Log($"{t.x} {t.y}");
        Debug.Log(boundingBox);

        int width = boundingBox.maxX - boundingBox.minX + 1;
        int heigth = boundingBox.maxY - boundingBox.minY + 1;

        Debug.Log(width);
        Debug.Log(heigth);

        fields = new FloorFieldType[width, heigth];
        foreach (var tile in localTiles)
            fields[tile.x - boundingBox.minX, tile.y-boundingBox.minY] = tile.fieldType;

        Debug.Log("przeszlo");
        isGenerated = true;
    }

    protected List<FieldWithPosition2D> ConvertSegmentsTileCordsToRoomCords()
    {
        List<FieldWithPosition2D> result = new();

        foreach (RoomSegment rs in segmentsList)
        {
            foreach (FieldWithPosition2D field in rs.GetTiles())
            {
                result.Add(new(
                    rs.x + field.x,
                    rs.y + field.y,
                    field.fieldType));
            }
        }

        return result;
    }
    protected (int minX, int maxX, int minY, int maxY) FindRoomBoundingBox(List<FieldWithPosition2D> fields)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = -int.MaxValue;
        int maxY = -int.MaxValue;

        foreach (FieldWithPosition2D field in fields)
        {
            if (field.x < minX) minX = field.x;
            if (field.x > maxX) maxX = field.x;
            if (field.y > maxY) maxY = field.y;
            if (field.y < minY) minY = field.y;
        }

        return (minX, maxX, minY, maxY);
    }
    public FloorFieldType[,] GetRoomFields() 
    {
        if (!isGenerated)
            Generate();
        return fields;
    }
    public Vector2Int GetTopLeftCorner() { return topLeftCorner; }
    public void AddSegment(RoomSegment roomSegment) 
    { 
        segmentsList.Add(roomSegment);
        if (topLeftCorner.x < roomSegment.x || topLeftCorner.y < roomSegment.y)
            topLeftCorner = new(roomSegment.x, roomSegment.y);
    }
}
