using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public enum FloorFieldType
{
    EMPTY = 0, BASE_FIELD = 1, SIDE_FIELD = 2, POSSIBLE_DOOR_FIELD = 3, CORRIDOR_FIELD=4, DOOR_FIELD=5, SPAWN_FIELD=6, EXIT_FIELD=7
}

public class DungeonFloorGenerator : MonoBehaviour
{
    FloorFieldType[,] FloorFieldTypes;
    GameObject[,] DungeonFloor;
    Position2D SpawnPoint;

    readonly float TILE_SIZE = 10;
    Vector3[] mapBorderPoints;
    List<Room> Rooms;

    int width;
    int height;
    int roomsAmount;
    int minRoomSize;
    int maxRoomSize;
    bool isGenerated = false;
    Dungeon result;
    private DungeonBiomePrefabSet prefabSet;

    public void SetDungeonParameters(int width, int heigth, int roomsAmount, int minRoomSize, int maxRoomSize, DungeonBiomePrefabSet prefabSet)
    {
        this.width = width;
        this.height = heigth;
        this.roomsAmount = roomsAmount;
        this.minRoomSize = minRoomSize;
        this.maxRoomSize = maxRoomSize;
        this.prefabSet = prefabSet;
        GenerateGizmosBorderPoints();
    }

    public Position2D FindSpawnPoint(Room spawnRoom)
    {
        FloorFieldType[,] roomFields = spawnRoom.GetRoomFields();
        Vector2Int roomCorner = spawnRoom.GetTopLeftCorner();

        for (int i = 0; i < roomFields.GetLength(0); i++)
        {
            for (int j = 0; j < roomFields.GetLength(1); j++)
            {
                if (roomFields[i,j] == FloorFieldType.SPAWN_FIELD)
                {
                    return new() { x = i + roomCorner.x, y = j + roomCorner.y };
                }
            }
        }
        
        return new() { x = -1, y = -1 };
    }

    public Dungeon GetGeneratedFloor()
    {
        if (isGenerated)
            return result;
        GenerateFloor();
        return new Dungeon(FloorFieldTypes, DungeonFloor, SpawnPoint);
    }

    private void GenerateGizmosBorderPoints()
    {
        mapBorderPoints = new Vector3[8];
        mapBorderPoints[0] = (new Vector3(transform.position.x + width*TILE_SIZE, transform.position.y, transform.position.z + height*TILE_SIZE));
        mapBorderPoints[1] = (new Vector3(transform.position.x, transform.position.y, transform.position.z + height*TILE_SIZE));
        mapBorderPoints[2] = (new Vector3(transform.position.x, transform.position.y, transform.position.z + height * TILE_SIZE));
        mapBorderPoints[3] = (new Vector3(transform.position.x, transform.position.y, transform.position.z));
        mapBorderPoints[4] = (new Vector3(transform.position.x, transform.position.y, transform.position.z));
        mapBorderPoints[5] = (new Vector3(transform.position.x + width*TILE_SIZE, transform.position.y, transform.position.z));
        mapBorderPoints[6] = (new Vector3(transform.position.x + width * TILE_SIZE, transform.position.y, transform.position.z));
        mapBorderPoints[7] = (new Vector3(transform.position.x + width * TILE_SIZE, transform.position.y, transform.position.z + height * TILE_SIZE));
    }

    [ContextMenu("Generate Dungeon")]
    public GameObject[,] GenerateFloor()
    {
        isGenerated = true;
        Rooms = new();
        RemoveAllChildren();
        FloorFieldTypes = GenerateEmptyFloor(width, height);


        ////floor = FillRandomBaseFields(floor, 1);
        GenerateRooms();

        SpawnPoint = FindSpawnPoint(Rooms[0]);
        ConnectRooms();

        GameObject[,] result = GenerateTiles();
        DungeonFloor = result;
        return result;
    }

    private void GenerateRooms()
    {
        int retries;
        Room room;
        int x, y, roomWidth, roomHeight;

        do
        {
            x = Random.Range(0, width - 1);
            y = Random.Range(0, height - 1);
            roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);

            room = RoomGenerator.GenerateRectRoom(x, y, roomWidth, roomHeight);
            room.AddModifier(new BorderModifier());
            room.AddModifier(new PossibleDoorMarkerModifier(2));
            room.AddModifier(new SpawnRoomModifier());
        } while (!CanRoomFit(room));
        
        Room r = PlaceRoom(room);
        Rooms.Add(r);

        for (int i = 0; i < roomsAmount; i++)
        {
            retries = 0;
            while (retries < Consts.MAX_RETRIES_AMOUNT_FOR_ROOM_FIT)
            {
                x = Random.Range(0, width - 1);
                y = Random.Range(0, height - 1);
                roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
                roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);

                if (Random.value <= 0.5)
                {
                    room = RoomGenerator.GenerateRectRoom(x, y, roomWidth, roomHeight);
                }
                else
                {
                    room = RoomGenerator.GenerateElipseRoom(x, y, roomWidth, roomHeight);
                }

                room.AddModifier(new BorderModifier());
                room.AddModifier(new PossibleDoorMarkerModifier(2));

                if (CanRoomFit(room))
                {
                    r = PlaceRoom(room);
                    Rooms.Add(r);
                    retries = 0;
                    break;
                }
                else
                    retries++;
            }
        }
    }

    private void RemoveAllChildren()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }
    
    private List<Position2D> CollectAllDoorMarkers()
    {
        List<Position2D> markers = new();

        foreach (Room room in Rooms)
            markers.AddRange(room.GetDoorMarkerGlobalPositions());

        return markers;
    }

    private (Position2D from, Position2D to) FindClosestMarkers(Room roomA, Room roomB)
    {
        List<Position2D> markersA = roomA.GetDoorMarkerGlobalPositions();
        List<Position2D> markersB = roomB.GetDoorMarkerGlobalPositions();

        double bestDistance = double.MaxValue;
        Position2D bestA = default;
        Position2D bestB = default;

        foreach (Position2D ma in markersA)
            foreach (Position2D mb in markersB)
            {
                float distance = Vector2Int.Distance(new(ma.x, ma.y), new(mb.x, mb.y));
                if (distance < bestDistance)
                {
                    bestA = ma;
                    bestB = mb;
                }
            }

        return (bestA, bestB);
    }
    
    private FloorFieldType[,] ConnectRooms()
    {
        HashSet<int> connected = new();
        connected.Add(0);

        while (connected.Count < Rooms.Count)
        {
            float closest = float.MaxValue;
            int bestA = -1;
            int bestB = -1;
            Position2D bestMarkerA = default;
            Position2D bestMarkerB = default;

            foreach (int a in connected)
            {
                for (int b = 0; b < Rooms.Count; b++)
                {
                    if (connected.Contains(b))
                        continue;

                    var (markA, markB) = FindClosestMarkers(Rooms[a], Rooms[b]);
                    float dist = Vector2Int.Distance(new(markA.x, markA.y), new(markB.x, markB.y));
                    if (dist < closest)
                    {
                        closest = dist;
                        bestA = a;
                        bestB = b;
                        bestMarkerA = markA;
                        bestMarkerB = markB;
                    }
                }
            }

            //CreateCorridorLShape(Rooms[bestA].Center(), Rooms[bestB].Center());
            CreateCorridorAStar(new() { x = bestMarkerA.x, y= bestMarkerA.y},new() { x=bestMarkerB.x, y=bestMarkerB.y});
            connected.Add(bestB);
        }

        List<Position2D> allDoorMarkers = CollectAllDoorMarkers();
        List<Position2D> notConnectedDoorMarkers = new();
        List<Vector2Int> possibleMoves = new() { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) };

        foreach (Position2D position2D in allDoorMarkers)
        {
            bool isCorridorNearby = false;
            foreach (Vector2Int m in possibleMoves)
            {
                int x = position2D.x + m.x;
                int y = position2D.y + m.y;

                if (x < 0 || y < 0 || x >= width || y >= height || (position2D.x == x && position2D.y == y))
                    continue;

                if (FloorFieldTypes[x, y] == FloorFieldType.CORRIDOR_FIELD)
                    isCorridorNearby = true;
            }

            if (!isCorridorNearby)
                notConnectedDoorMarkers.Add(position2D);
        }

        while (notConnectedDoorMarkers.Count >= 2)
        {
            int a = Random.Range(0, notConnectedDoorMarkers.Count);
            int b = Random.Range(0, notConnectedDoorMarkers.Count);
            while (a==b)
                b = Random.Range(0, notConnectedDoorMarkers.Count);

            CreateCorridorAStar(
                new(notConnectedDoorMarkers[a].x, notConnectedDoorMarkers[a].y),
                new(notConnectedDoorMarkers[b].x, notConnectedDoorMarkers[b].y)
                );

            int c;
            if (a < b)
            {
                c = a;
                a = b;
                b = c;
            }

            notConnectedDoorMarkers.RemoveAt(a);
            notConnectedDoorMarkers.RemoveAt(b);
        }

        return FloorFieldTypes;
    }

    private FloorFieldType[,] CreateCorridorAStar(Vector2Int a, Vector2Int b)
    {
        MoveCostManager moveCostManager = new();

        moveCostManager.AddCost(FloorFieldType.EMPTY, 5);
        moveCostManager.AddCost(FloorFieldType.POSSIBLE_DOOR_FIELD, 0);
        moveCostManager.AddCost(FloorFieldType.CORRIDOR_FIELD, 4);
        moveCostManager.AddCost(FloorFieldType.BASE_FIELD, 50);
        moveCostManager.AddCost(FloorFieldType.SIDE_FIELD, 1000);

        var result = AStar.FindPath(
            FloorFieldTypes,
            new() { x = a.x, y = a.y },
            new() { x = b.x, y = b.y },
            moveCostManager, MovementDirections.FOUR);

        foreach (var p in result)
            PlaceCorridor(p.x, p.y);

        return FloorFieldTypes;
    }

    private FloorFieldType[,] CreateCorridorLShape(Vector2Int a, Vector2Int b)
    {
        int x = a.x;
        int y = a.y;

        while (x != b.x)
        {
            PlaceCorridor(x, y);
            x += (b.x > x) ? 1 : -1;
        }

        while (y != b.y)
        {
            PlaceCorridor(x, y);
            y += (b.y > y) ? 1 : -1;
        }

        return FloorFieldTypes;
    }

    private void PlaceCorridor(int x, int y)
    {
        if (FloorFieldTypes[x, y] == FloorFieldType.EMPTY)
            FloorFieldTypes[x, y] = FloorFieldType.CORRIDOR_FIELD;
    }

    private FloorFieldType[,] GenerateDoors()
    {
        for (int x=0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // TODO - generate door tiles
            }
        }
        return FloorFieldTypes;
    }

    private FloorFieldType[,] GenerateEmptyFloor(int width, int height)
    {
        FloorFieldType[,] newFloor = new FloorFieldType[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                newFloor[x, y] = FloorFieldType.EMPTY;
            }
        }
        return newFloor;
    }

    private Room PlaceRoom(Room room)
    {
        int startX = room.GetTopLeftCorner().x;
        int startY = room.GetTopLeftCorner().y;
        FloorFieldType[,] fields = room.GetRoomFields();
        int width = fields.GetLength(0);
        int heigth = fields.GetLength(1);

        for (int i = 0; i < width; i++)
            for (int j = 0; j < heigth; j++)
                FloorFieldTypes[i+startX, j+startY] = fields[i, j];

        return room;
    }

    private bool CanRoomFit(Room room)
    {
        int startX = room.GetTopLeftCorner().x;
        int startY = room.GetTopLeftCorner().y;
        FloorFieldType[,] fields = room.GetRoomFields();
        int roomWidth = fields.GetLength(0);
        int roomHeight = fields.GetLength(1);

        if (startX + roomWidth >= width ||
            startY + roomHeight >= height ||
            startX < 0 || startY < 0)
            return false;

        for (int i=0;i< roomWidth; i++)
            for (int j = 0; j < roomHeight; j++)
            {
                if (fields[i, j] == FloorFieldType.EMPTY)
                    continue;

                int globalX = i + startX;
                int globalY = j + startY;

                for (int dx=-1;dx<=1;dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int boundX = globalX + dx;
                        int boundY = globalY + dy;

                        if (boundX < 0 || boundY < 0 || boundX >= width || boundY >= height)
                            return false;

                        if (FloorFieldTypes[boundX, boundY] != FloorFieldType.EMPTY)
                            return false;
                    }
            }
        return true;
    }

    private GameObject[,] GenerateTiles()
    {
        int width = FloorFieldTypes.GetLength(0);
        int height = FloorFieldTypes.GetLength(1);

        GameObject[,] result = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject go = FindObjectRelatedToFloorType(FloorFieldTypes[x, y]);

                if (go == null)
                    continue;

                Vector3 posVector = transform.position + new Vector3(x * TILE_SIZE + (0.5f * TILE_SIZE), 0, y * TILE_SIZE + (0.5f * TILE_SIZE));
                go.transform.position = posVector;
                result[x, y] = go;

                TileInfo ti = go.GetComponent<TileInfo>();
                ti.position.x = x;
                ti.position.y = y;
                ti.type = FloorFieldTypes[x, y];
            }
        }

        return result;
    }

    private GameObject FindObjectRelatedToFloorType(FloorFieldType type)
    {
        switch (type)
        {
            case FloorFieldType.EMPTY:
                return null;

            case FloorFieldType.BASE_FIELD:
                return Instantiate(prefabSet.BaseFloorTile, transform);

            case FloorFieldType.SIDE_FIELD:
                return Instantiate(prefabSet.SideTile, transform);

            case FloorFieldType.POSSIBLE_DOOR_FIELD:
                return Instantiate(prefabSet.PossibleDoorTile, transform);

            case FloorFieldType.CORRIDOR_FIELD:
                return Instantiate(prefabSet.CorridorTile, transform);

            case FloorFieldType.DOOR_FIELD:
                return Instantiate(prefabSet.DoorTile, transform);

            case FloorFieldType.SPAWN_FIELD:
                return Instantiate(prefabSet.SpawnTile, transform);

            case FloorFieldType.EXIT_FIELD:
                return Instantiate(prefabSet.ExitTile, transform);

            default:
                return null;
        }
    }

    private void OnValidate()
    {
        if (width <= 0)
            width = 1;

        if (height <= 0)
            height = 1;

        GenerateGizmosBorderPoints();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLineList(mapBorderPoints);
    }
}
