using System.Collections.Generic;

public class DungeonRequest
{
    public int Width;
    public int Height;
    public int RoomCount;
    public int MinRoomSize;
    public int MaxRoomSize;
    public float OpenRoomChance = 0.02f;
}

public class FloorPlan
{
    public FloorFieldType[,] Fields;
    public Position2D Spawn;
    public int RoomCount;
    public List<DoorOrder> Doors = new List<DoorOrder>();
    public List<Position2D> SteelKeys = new List<Position2D>();
}

public interface IDungeonGenerator
{
    IReadOnlyList<IRoomKind> RoomKinds { get; }
    ICorridorRule Corridor { get; }
    FloorPlan Generate(DungeonRequest request);
}
