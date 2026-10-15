public class HoleRoomKind : IRoomKind
{
    readonly bool walled;

    public HoleRoomKind(bool walled)
    {
        this.walled = walled;
    }

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
        RoomFinish.Apply(room, walled, new InteriorHoleModifier());
        return room;
    }
}
