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
            int beforeWidth = fields.GetLength(0);
            int beforeHeight = fields.GetLength(1);
            FloorFieldType[,] next = modifier.Apply(fields);
            if (modifier is BorderModifier
                && next.GetLength(0) == beforeWidth + 2
                && next.GetLength(1) == beforeHeight + 2)
                topLeftCorner = new Vector2Int(topLeftCorner.x - 1, topLeftCorner.y - 1);
            fields = next;
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
        if (segmentsList.Count == 1)
        {
            topLeftCorner = new Vector2Int(roomSegment.x, roomSegment.y);
            return;
        }

        topLeftCorner = new Vector2Int(
            Mathf.Min(topLeftCorner.x, roomSegment.x),
            Mathf.Min(topLeftCorner.y, roomSegment.y));
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

    public bool TryKeepOneDoor(out Position2D kept)
    {
        List<Position2D> doors = GetDoorMarkerGlobalPositions();
        kept = default;
        if (doors.Count == 0 || fields == null)
            return false;

        kept = doors[Random.Range(0, doors.Count)];
        FloorFieldType restore = HasWall() ? FloorFieldType.SIDE_FIELD : FloorFieldType.BASE_FIELD;
        Vector2Int corner = GetTopLeftCorner();
        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i].x == kept.x && doors[i].y == kept.y)
                continue;

            int localX = doors[i].x - corner.x;
            int localY = doors[i].y - corner.y;
            if (localX < 0 || localY < 0 || localX >= fields.GetLength(0) || localY >= fields.GetLength(1))
                continue;

            fields[localX, localY] = restore;
        }

        return true;
    }

    bool HasWall()
    {
        if (fields == null)
            return false;

        for (int x = 0; x < fields.GetLength(0); x++)
            for (int y = 0; y < fields.GetLength(1); y++)
                if (fields[x, y] == FloorFieldType.SIDE_FIELD)
                    return true;

        return false;
    }
    
    public bool DoesFieldBelong(Position2D position2D)
    {
        if (position2D.x >= topLeftCorner.x && position2D.x < topLeftCorner.x + fields.GetLength(0) &&
            position2D.y >= topLeftCorner.y && position2D.y < topLeftCorner.y + fields.GetLength(0))
            return true;
        return false;
    }
}
