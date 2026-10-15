public class RoomsWithoutWallsFloorModifier : IFloorModifier
{
    public void Apply(DungeonRequest request)
    {
        request.OpenRoomChance = 0.2f;
    }
}
