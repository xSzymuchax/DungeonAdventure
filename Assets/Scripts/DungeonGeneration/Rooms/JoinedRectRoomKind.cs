using UnityEngine;

public class JoinedRectRoomKind : IRoomKind
{
    readonly bool walled;

    public JoinedRectRoomKind(bool walled)
    {
        this.walled = walled;
    }

    public Room Create(int x, int y, int sizeX, int sizeY)
    {
        int width = sizeX < 1 ? 1 : sizeX;
        int height = sizeY < 1 ? 1 : sizeY;
        int otherWidth = Random.Range(Mathf.Min(2, width), width + 1);
        int otherHeight = Random.Range(Mathf.Min(2, height), height + 1);
        Vector2Int offset = Offset(width, height, otherWidth, otherHeight);

        RectRoom room = new RectRoom();
        room.AddSegment(new RectRoomSegment
        {
            x = x,
            y = y,
            width = width,
            height = height
        });
        room.AddSegment(new RectRoomSegment
        {
            x = x + offset.x,
            y = y + offset.y,
            width = otherWidth,
            height = otherHeight
        });
        RoomFinish.Apply(room, walled);
        return room;
    }

    static Vector2Int Offset(int width, int height, int otherWidth, int otherHeight)
    {
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0:
                return new Vector2Int(Random.Range(width - otherWidth + 1, width + 1), Random.Range(1 - otherHeight, height));
            case 1:
                return new Vector2Int(Random.Range(-otherWidth, 0), Random.Range(1 - otherHeight, height));
            case 2:
                return new Vector2Int(Random.Range(1 - otherWidth, width), Random.Range(height - otherHeight + 1, height + 1));
            default:
                return new Vector2Int(Random.Range(1 - otherWidth, width), Random.Range(-otherHeight, 0));
        }
    }
}
