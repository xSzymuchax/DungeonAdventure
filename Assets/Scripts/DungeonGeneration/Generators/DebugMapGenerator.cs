using System.Collections.Generic;

public class DebugMapGenerator : IDungeonGenerator
{
    public IReadOnlyList<IRoomKind> RoomKinds => System.Array.Empty<IRoomKind>();
    public ICorridorRule Corridor { get; } = new EmptyCorridorRule();

    public FloorPlan Generate(DungeonRequest request)
    {
        int width = request.Width;
        int height = request.Height;
        FloorFieldType[,] fields = new FloorFieldType[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                fields[x, y] = FloorFieldType.BASE_FIELD;

        return new FloorPlan
        {
            Fields = fields,
            Spawn = new Position2D { x = 0, y = 0 },
            RoomCount = 0
        };
    }
}
