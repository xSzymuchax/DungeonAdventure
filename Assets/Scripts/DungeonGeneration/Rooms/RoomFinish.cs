public static class RoomFinish
{
    public static void Apply(Room room, bool walled, IRoomModifier afterBorder = null)
    {
        if (walled)
            room.AddModifier(new BorderModifier());
        if (afterBorder != null)
            room.AddModifier(afterBorder);
        room.AddModifier(new PossibleDoorMarkerModifier());
        room.AddModifier(new RotateRoomModifier());
    }
}
