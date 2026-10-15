public class ElipseRoomKind : IRoomKind
{
    readonly bool walled;

    public ElipseRoomKind(bool walled)
    {
        this.walled = walled;
    }

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
        RoomFinish.Apply(room, walled);
        return room;
    }
}
