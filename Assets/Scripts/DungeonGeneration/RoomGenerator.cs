using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomGenerator
{
    public static Room GenerateRectRoom(int x, int y, int width, int heigth)
    {
        RectRoom room = new();

        RectRoomSegment rectRoomSegment = new()
        {
            x = x,
            y = y,
            width = width,
            height = heigth
        };

        room.AddSegment(rectRoomSegment);

        return room;
    }

    public static Room GenerateElipseRoom(int x, int y, int radiusX, int radiusY)
    {
        ElipseRoom room = new();

        ElipseRoomSegment elipseRoomSegment = new()
        {
            x = x,
            y = y,
            xRadius = radiusX,
            yRadius = radiusY
        };

        room.AddSegment(elipseRoomSegment);

        return room;
    }
}
