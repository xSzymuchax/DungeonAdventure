using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum FloorFieldType
{
    EMPTY = 0, BASE_FIELD = 1, SIDE_FIELD = 2, CORNER_FIELD = 3, CORRIDOR_FIELD=4, DOOR_FIELD=5
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
    public GameObject CornerTile;
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
        for (int i=0;i<roomsAmount;i++)
            PlaceEmptyRectangularRoom(minRoomSize, maxRoomSize);
        
        ConnectRooms();

        GameObject[,] result = GenerateTiles();
        return result;
    }

    private void RemoveAllChildren()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
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

            foreach (int a in connected)
            {
                for (int b = 0; b < Rooms.Count; b++)
                {
                    if (connected.Contains(b))
                        continue;

                    float dist = Vector2Int.Distance(Rooms[a].Center(), Rooms[b].Center());
                    if (dist < closest)
                    {
                        closest = dist;
                        bestA = a;
                        bestB = b;
                    }
                }
            }

            CreateCorridorLShape(Rooms[bestA].Center(), Rooms[bestB].Center());
            connected.Add(bestB);
        }

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
        foreach (FieldWithPosition2D fieldWithPosition2D in room.GetCoveredFields())
            FloorFieldTypes[fieldWithPosition2D.x, fieldWithPosition2D.y] = fieldWithPosition2D.fieldType;

        return room;
    }

    private bool CanRoomFit(Room room)
    {
        foreach (FieldWithPosition2D field in room.GetCoveredFields())
        {
            for (int dx=-1;dx<=1;dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = field.x + dx;
                    int y = field.y + dy;


                    if (x < 0 || x >= width)
                        return false;

                    if (y < 0 || y >= height)
                        return false;

                    if (FloorFieldTypes[x,y] != FloorFieldType.EMPTY)
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

            case FloorFieldType.CORNER_FIELD:
                return Instantiate(CornerTile, transform);

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
