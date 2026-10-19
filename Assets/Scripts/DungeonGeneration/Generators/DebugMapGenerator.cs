using System.Collections.Generic;
using UnityEngine;

public class DebugMapGenerator : IDungeonGenerator
{
    public IReadOnlyList<IRoomKind> RoomKinds => System.Array.Empty<IRoomKind>();
    public ICorridorRule Corridor { get; } = new EmptyCorridorRule();

    public FloorPlan Generate(DungeonRequest request)
    {
        int width = request.Width > 0 ? request.Width : 1;
        int height = request.Height > 0 ? request.Height : 1;
        FloorFieldType[,] fields = new FloorFieldType[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                fields[x, y] = FloorFieldType.BASE_FIELD;

        FloorPlan plan = new FloorPlan
        {
            Fields = fields,
            Spawn = new Position2D { x = 0, y = 0 },
            RoomCount = 0
        };

        DoorOrder[] row =
        {
            Door(DoorMaterial.Wood, PadlockKind.None),
            Door(DoorMaterial.Glass, PadlockKind.None),
            Door(DoorMaterial.Steel, PadlockKind.None),
            Door(DoorMaterial.Steel, PadlockKind.Steel),
            Door(DoorMaterial.Steel, PadlockKind.Weak),
            Door(DoorMaterial.Steel, PadlockKind.Nasty),
            Door(DoorMaterial.Steel, PadlockKind.Temporary)
        };

        int doorRow = 2;
        for (int i = 0; i < row.Length; i++)
        {
            int x = 2 + i * 2;
            if (x >= width || doorRow >= height)
                break;

            fields[x, doorRow] = FloorFieldType.POSSIBLE_DOOR_FIELD;
            DoorOrder order = row[i];
            order.x = x;
            order.y = doorRow;
            plan.Doors.Add(order);
        }

        if (width > 2 && height > 3)
            plan.SteelKeys.Add(new Position2D { x = 2, y = 3 });

        ChestOrder[] chests =
        {
            Chest(ChestMaterial.Wood, PadlockKind.None, 1),
            Chest(ChestMaterial.Glass, PadlockKind.None, 1),
            Chest(ChestMaterial.Steel, PadlockKind.None, 1),
            Chest(ChestMaterial.Wood, PadlockKind.Weak, 1),
            Chest(ChestMaterial.Steel, PadlockKind.Gold, Random.Range(2, 4))
        };
        int chestRow = 7;
        for (int i = 0; i < chests.Length; i++)
        {
            int x = 2 + i * 2;
            if (x >= width || chestRow >= height)
                break;

            ChestOrder order = chests[i];
            order.x = x;
            order.y = chestRow;
            plan.Chests.Add(order);
        }

        if (width > 10 && height > 8)
            plan.GoldKeys.Add(new Position2D { x = 10, y = 8 });
        return plan;
    }

    static DoorOrder Door(DoorMaterial material, PadlockKind padlock)
    {
        return new DoorOrder
        {
            rollMaterial = false,
            material = material,
            padlock = padlock
        };
    }

    static ChestOrder Chest(ChestMaterial material, PadlockKind padlock, int itemCount)
    {
        return new ChestOrder
        {
            rollMaterial = false,
            material = material,
            padlock = padlock,
            itemCount = itemCount
        };
    }
}
