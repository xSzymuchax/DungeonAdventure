using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public enum FloorFieldType
{
    EMPTY = 0, BASE_FIELD = 1, SIDE_FIELD = 2, POSSIBLE_DOOR_FIELD = 3, CORRIDOR_FIELD=4, DOOR_FIELD=5
}

public class DungeonFloorGenerator : MonoBehaviour
{
    FloorFieldType[,] FloorFieldTypes;
    GameObject[,] DungeonFloor;
    readonly float TILE_SIZE = 10;
    readonly int MAX_RETRIES_AMOUNT_FOR_ROOM_FIT = 100;
    Vector3[] mapBorderPoints;
    List<Room> Rooms;

    public int width = 20;
    public int height = 20;
    public int roomsAmount = 3;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;
    public GameObject BaseFloorTile;
    public GameObject PossibleDoorTile;
    public GameObject SideTile;
    public GameObject CorridorTile;
    public GameObject DoorTile;

    void Start()
    {
        DungeonFloor = GenerateFloor();
        GenerateGizmosBorderPoints();
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
        Rooms = new();
        RemoveAllChildren();
        FloorFieldTypes = GenerateEmptyFloor(width, height);


        //floor = FillRandomBaseFields(floor, 1);
        for (int i = 0; i < roomsAmount; i++)
        {
            if (Random.value <= 0.5)
                PlaceEmptyRectangularRoom(minRoomSize, maxRoomSize);
            else
                PlaceEmptyElipseRoom(minRoomSize / 2, maxRoomSize / 2);
        }
            
        
        ConnectRooms();

        GameObject[,] result = GenerateTiles();
        return result;
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

    private FloorFieldType[,] PlaceEmptyRectangularRoom(int minRoomSize, int maxRoomSize)
    {
        int width = FloorFieldTypes.GetLength(0);
        int height = FloorFieldTypes.GetLength(1);
        int retries = 0;
        while (retries < MAX_RETRIES_AMOUNT_FOR_ROOM_FIT)
        {
            int x = Random.Range(0, width-1);
            int y = Random.Range(0, height - 1);
            int roomWidth = Random.Range(minRoomSize, maxRoomSize+1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize+1);

            RectRoomSegment rectRoomSegment = new()
            {
                x=x,
                y=y,
                width=roomWidth,
                height=roomHeight
            };

            RectRoom rectRoom = new();
            rectRoom.AddSegment(rectRoomSegment);
            rectRoom.AddModifier(new BorderModifier());
            rectRoom.AddModifier(new PossibleDoorMarkerModifier(2));

            if (CanRoomFit(rectRoom))
            {
                Room r = PlaceRoom(rectRoom);
                Rooms.Add(r);
                break;
            }
            else
                retries++;
        }

        return FloorFieldTypes;
    }

    private FloorFieldType[,] PlaceEmptyElipseRoom(int minRadius, int maxRadius)
    {
        int width = FloorFieldTypes.GetLength(0);
        int height = FloorFieldTypes.GetLength(1);
        int retries = 0;
        while (retries < MAX_RETRIES_AMOUNT_FOR_ROOM_FIT)
        {
            int x = Random.Range(0, width - 1);
            int y = Random.Range(0, height - 1);
            int xRadius = Random.Range(minRadius, maxRadius + 1);
            int yRadius = Random.Range(minRadius, maxRadius + 1);

            ElipseRoomSegment elipseRoomSegment = new()
            {
                x = x,
                y = y,
                xRadius = xRadius,
                yRadius = yRadius
            };

            ElipseRoom rectRoom = new();
            rectRoom.AddSegment(elipseRoomSegment);
            rectRoom.AddModifier(new BorderModifier());
            rectRoom.AddModifier(new PossibleDoorMarkerModifier(2)); 

            if (CanRoomFit(rectRoom))
            {
                Room r = PlaceRoom(rectRoom);
                Rooms.Add(r);
                break;
            }
            else
                retries++;
        }

        return FloorFieldTypes;
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

    private FloorFieldType[,] FillRandomBaseFields(float chanceOfFill)
    {
        int width = FloorFieldTypes.GetLength(0);
        int height = FloorFieldTypes.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Random.value <= chanceOfFill)
                    FloorFieldTypes[x, y] = FloorFieldType.BASE_FIELD;
            }
        }
        return FloorFieldTypes;
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

                go.GetComponent<TilePosition>().x = x;
                go.GetComponent<TilePosition>().y = y;
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
                return Instantiate(BaseFloorTile, transform);

            case FloorFieldType.SIDE_FIELD:
                return Instantiate(SideTile, transform);

            case FloorFieldType.POSSIBLE_DOOR_FIELD:
                return Instantiate(PossibleDoorTile, transform);

            case FloorFieldType.CORRIDOR_FIELD:
                return Instantiate(CorridorTile, transform);

            case FloorFieldType.DOOR_FIELD:
                return Instantiate(DoorTile, transform);

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


//private FloorFieldType[,] ConnectRooms()
//{
//    HashSet<int> connected = new();
//    connected.Add(0);

//    while (connected.Count < Rooms.Count)
//    {
//        float closest = float.MaxValue;
//        int bestA = -1;
//        int bestB = -1;

//        foreach (int a in connected)
//        {
//            for (int b = 0; b < Rooms.Count; b++)
//            {
//                if (connected.Contains(b))
//                    continue;

//                float dist = Vector2Int.Distance(Rooms[a].Center(), Rooms[b].Center());
//                if (dist < closest)
//                {
//                    closest = dist;
//                    bestA = a;
//                    bestB = b;
//                }
//            }
//        }

//        //CreateCorridorLShape(Rooms[bestA].Center(), Rooms[bestB].Center());
//        CreateCorridorAStar(Rooms[bestA].Center(), Rooms[bestB].Center());
//        connected.Add(bestB);
//    }

//    return FloorFieldTypes;
//}    