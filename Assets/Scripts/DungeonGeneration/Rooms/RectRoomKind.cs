public class RectRoomKind : IRoomKind
{
    public Room Create(int x, int y, int sizeX, int sizeY)
    {
        RectRoom room = new RectRoom();
        room.AddSegment(new RectRoomSegment
        {
            x = x,
            y = y,
            width = sizeX,
            height = sizeY
        });
        return room;
    }
}
