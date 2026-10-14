using System.Collections.Generic;
using UnityEngine;

public class ForestDungeonGenerator : IDungeonGenerator
{
    static readonly IRoomKind[] kinds =
    {
        new RectRoomKind(),
        new ElipseRoomKind()
    };

    public IReadOnlyList<IRoomKind> RoomKinds => kinds;
    public ICorridorRule Corridor { get; } = new ForestCorridorRule();

    FloorDraft draft;
    Position2D spawnPoint;
    int roomCount;
    int minRoomSize;
    int maxRoomSize;

    public FloorPlan Generate(DungeonRequest request)
    {
        int width = request.Width > 0 ? request.Width : 1;
        int height = request.Height > 0 ? request.Height : 1;
        roomCount = request.RoomCount;
        minRoomSize = request.MinRoomSize;
        maxRoomSize = request.MaxRoomSize;

        const int maxExitAttempts = 100;
        for (int attempt = 0; attempt < maxExitAttempts; attempt++)
        {
            draft = new FloorDraft(width, height);
            GenerateRooms();
            spawnPoint = draft.FindSpawnPoint(draft.Rooms[0]);
            Corridor.Connect(draft.Fields, draft.Rooms);
            if (PlaceExitField())
                break;

            if (attempt == maxExitAttempts - 1)
                Debug.LogWarning("Nie udało się wygenerować piętra z osiągalnym zejściem.");
        }

        return new FloorPlan
        {
            Fields = draft.Fields,
            Spawn = spawnPoint,
            RoomCount = draft.Rooms.Count
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

        for (int i = 0; i < roomCount; i++)
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
        return RoomKinds[Random.Range(0, RoomKinds.Count)];
    }

    Room MakeRoom(IRoomKind kind, bool spawn)
    {
        int x = Random.Range(0, draft.Width - 1);
        int y = Random.Range(0, draft.Height - 1);
        int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
        int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
        Room room = kind.Create(x, y, roomWidth, roomHeight);
        room.AddModifier(new BorderModifier());
        room.AddModifier(new PossibleDoorMarkerModifier(2));
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
