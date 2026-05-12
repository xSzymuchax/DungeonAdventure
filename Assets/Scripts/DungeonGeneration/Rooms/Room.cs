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
    protected List<Position2D> doorMarkerPositions = new();
    protected List<IRoomModifier> modifiersList = new();

    public abstract Vector2Int Center();

    protected void Generate()
    {
        var localTiles = ConvertSegmentsTileCordsToRoomCords();
        var boundingBox = FindRoomBoundingBox(localTiles);

        int width = boundingBox.maxX - boundingBox.minX + 1;
        int heigth = boundingBox.maxY - boundingBox.minY + 1;

        fields = new FloorFieldType[width, heigth];
        foreach (var tile in localTiles)
            fields[tile.x - boundingBox.minX, tile.y-boundingBox.minY] = tile.fieldType;

        foreach (IRoomModifier modifier in modifiersList)
        {
            fields = modifier.Apply(fields);
        }

        isGenerated = true;
    }

    protected List<Position2DWithField> ConvertSegmentsTileCordsToRoomCords()
    {
        List<Position2DWithField> result = new();

        foreach (RoomSegment rs in segmentsList)
        {
            foreach (Position2DWithField field in rs.GetTiles())
            {
                result.Add(new()
                {
                    x = rs.x + field.x,
                    y = rs.y + field.y,
                    fieldType = field.fieldType
                });
            }
        }

        return result;
    }
    protected (int minX, int maxX, int minY, int maxY) FindRoomBoundingBox(List<Position2DWithField> fields)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = -int.MaxValue;
        int maxY = -int.MaxValue;

        foreach (Position2DWithField field in fields)
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
    public void AddModifier(IRoomModifier modifier)
    {
        modifiersList.Add(modifier);
    }
    public List<Position2D> GetDoorMarkerGlobalPositions()
    {
        doorMarkerPositions = new();
        for (int i=0;i<fields.GetLength(0);i++)
            for (int j = 0; j < fields.GetLength(1); j++)
            {
                if (fields[i,j] == FloorFieldType.POSSIBLE_DOOR_FIELD)
                    doorMarkerPositions.Add(new() { x = i+topLeftCorner.x, y = j+topLeftCorner.y });
            }
        return doorMarkerPositions;
    }
    
    public bool DoesFieldBelong(Position2D position2D)
    {
        if (position2D.x >= topLeftCorner.x && position2D.x < topLeftCorner.x + fields.GetLength(0) &&
            position2D.y >= topLeftCorner.y && position2D.y < topLeftCorner.y + fields.GetLength(0))
            return true;
        return false;
    }
}
