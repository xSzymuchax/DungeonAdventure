using System.Collections.Generic;
using UnityEngine;

public class ForestDungeonGenerator : IDungeonGenerator
{
    static readonly System.Func<bool, IRoomKind>[] shapes =
    {
        walled => new RectRoomKind(walled),
        walled => new ElipseRoomKind(walled),
        walled => new LShapeRoomKind(walled),
        walled => new SShapeRoomKind(walled),
        walled => new HoleRoomKind(walled),
        walled => new RandomHolesRoomKind(walled),
        walled => new JoinedRectRoomKind(walled)
    };

    static readonly IFloorModifier[] floorModifiers =
    {
        new LargerMapFloorModifier(),
        new RoomsWithoutWallsFloorModifier()
    };

    static readonly IRoomKind[] kinds;

    static ForestDungeonGenerator()
    {
        kinds = new IRoomKind[shapes.Length];
        for (int i = 0; i < shapes.Length; i++)
            kinds[i] = shapes[i](true);
    }

    public IReadOnlyList<IRoomKind> RoomKinds => kinds;
    public ICorridorRule Corridor { get; } = new ForestCorridorRule();

    FloorDraft draft;
    Position2D spawnPoint;
    int roomCount;
    int minRoomSize;
    int maxRoomSize;
    float openRoomChance;

    public FloorPlan Generate(DungeonRequest request)
    {
        floorModifiers[Random.Range(0, floorModifiers.Length)].Apply(request);

        int width = request.Width > 0 ? request.Width : 1;
        int height = request.Height > 0 ? request.Height : 1;
        roomCount = request.RoomCount;
        minRoomSize = request.MinRoomSize;
        maxRoomSize = request.MaxRoomSize;
        openRoomChance = request.OpenRoomChance;

        const int maxExitAttempts = 100;
        List<DoorOrder> lastDoors = new List<DoorOrder>();
        List<Position2D> lastKeys = new List<Position2D>();
        List<ChestOrder> lastChests = new List<ChestOrder>();
        List<Position2D> lastGoldKeys = new List<Position2D>();
        for (int attempt = 0; attempt < maxExitAttempts; attempt++)
        {
            draft = new FloorDraft(width, height);
            GenerateRooms();
            spawnPoint = draft.FindSpawnPoint(draft.Rooms[0]);
            List<DoorOrder> doors = new List<DoorOrder>();
            List<Room> lockedRooms = new List<Room>();
            List<Position2D> lockedDoors = new List<Position2D>();
            DoorFloorPass.SealRooms(draft, doors, lockedRooms, lockedDoors);
            Corridor.Connect(draft.Fields, draft.Rooms);
            DoorFloorPass.PlaceNasty(draft, lockedDoors, doors);
            List<Position2D> keys = new List<Position2D>();
            DoorFloorPass.PlaceKeys(draft, spawnPoint, lockedRooms, lockedDoors, doors, keys);
            bool exit = PlaceExitField();
            List<ChestOrder> chests = new List<ChestOrder>();
            List<Position2D> goldKeys = new List<Position2D>();
            ChestFloorPass.Place(draft, spawnPoint, lockedRooms, keys, chests, goldKeys);
            lastDoors = doors;
            lastKeys = keys;
            lastChests = chests;
            lastGoldKeys = goldKeys;
            if (exit)
            {
                return new FloorPlan
                {
                    Fields = draft.Fields,
                    Spawn = spawnPoint,
                    RoomCount = draft.Rooms.Count,
                    Doors = doors,
                    SteelKeys = keys,
                    Chests = chests,
                    GoldKeys = goldKeys
                };
            }

            if (attempt == maxExitAttempts - 1)
                Debug.LogWarning("Nie udało się wygenerować piętra z osiągalnym zejściem.");
        }

        return new FloorPlan
        {
            Fields = draft.Fields,
            Spawn = spawnPoint,
            RoomCount = draft.Rooms.Count,
            Doors = lastDoors,
            SteelKeys = lastKeys,
            Chests = lastChests,
            GoldKeys = lastGoldKeys
        };
    }

    void GenerateRooms()
    {
        Room room;
        do
        {
            room = MakeRoom(PickKind(), true);
        } while (!draft.CanRoomFit(room));

        draft.PlaceRoom(room);

        for (int i = 1; i < roomCount; i++)
        {
            int retries = 0;
            while (retries < Consts.MAX_RETRIES_AMOUNT_FOR_ROOM_FIT)
            {
                room = MakeRoom(PickKind(), false);
                if (draft.CanRoomFit(room))
                {
                    draft.PlaceRoom(room);
                    break;
                }

                retries++;
            }
        }
    }

    IRoomKind PickKind()
    {
        bool walled = Random.value >= openRoomChance;
        return shapes[Random.Range(0, shapes.Length)](walled);
    }

    Room MakeRoom(IRoomKind kind, bool spawn)
    {
        int x = Random.Range(0, draft.Width - 1);
        int y = Random.Range(0, draft.Height - 1);
        bool large = Random.value < 0.1f;
        int low = large ? minRoomSize * 2 : minRoomSize;
        int high = large ? maxRoomSize * 2 : maxRoomSize;
        int roomWidth = Random.Range(low, high + 1);
        int roomHeight = Random.Range(low, high + 1);
        Room room = kind.Create(x, y, roomWidth, roomHeight);
        if (spawn)
            room.AddModifier(new SpawnRoomModifier());
        return room;
    }

    bool PlaceExitField()
    {
        if (draft.Rooms.Count < 2 || spawnPoint.x < 0 || spawnPoint.y < 0)
            return false;

        bool[,] reachable = draft.ReachableFrom(spawnPoint, MoveCostManager.CreateDefault());
        List<Position2D> candidates = new List<Position2D>();

        for (int roomIndex = 1; roomIndex < draft.Rooms.Count; roomIndex++)
        {
            Room room = draft.Rooms[roomIndex];
            Vector2Int corner = room.GetTopLeftCorner();
            FloorFieldType[,] roomFields = room.GetRoomFields();
            int roomWidth = roomFields.GetLength(0);
            int roomHeight = roomFields.GetLength(1);

            for (int i = 0; i < roomWidth; i++)
            {
                for (int j = 0; j < roomHeight; j++)
                {
                    if (roomFields[i, j] != FloorFieldType.BASE_FIELD)
                        continue;

                    int x = corner.x + i;
                    int y = corner.y + j;
                    if (x < 0 || y < 0 || x >= draft.Width || y >= draft.Height)
                        continue;
                    if (draft.Fields[x, y] != FloorFieldType.BASE_FIELD)
                        continue;
                    if (!reachable[x, y])
                        continue;
                    if (draft.IsInsideRoom(draft.Rooms[0], x, y))
                        continue;

                    candidates.Add(new Position2D { x = x, y = y });
                }
            }
        }

        if (candidates.Count == 0)
            return false;

        Position2D exit = candidates[Random.Range(0, candidates.Count)];
        draft.Fields[exit.x, exit.y] = FloorFieldType.EXIT_FIELD;
        return true;
    }
}
