public class ElipseRoomKind : IRoomKind
{
    public Room Create(int x, int y, int sizeX, int sizeY)
    {
        ElipseRoom room = new ElipseRoom();
        room.AddSegment(new ElipseRoomSegment
        {
            x = x,
            y = y,
            xRadius = sizeX,
            yRadius = sizeY
        });
        return room;
    }
}
