public class LShapeRoomKind : IRoomKind
{
    readonly bool walled;

    public LShapeRoomKind(bool walled)
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
            width = AtLeast(sizeX),
            height = AtLeast(sizeY)
        });
        room.AddModifier(new LShapeModifier());
        RoomFinish.Apply(room, walled);
        return room;
    }

    static int AtLeast(int size)
    {
        return size < Consts.ROOM_PASSAGE_WIDTH * 2 ? Consts.ROOM_PASSAGE_WIDTH * 2 : size;
    }
}
