using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

enum FloorFieldType
{
    EMPTY = 0, BASE_FIELD = 1, SIDE_FIELD = 2, CORNER_FIELD = 3, CORRIDOR_FIELD=4
}

struct Room
{
    public int x;
    public int y;
    public int height;
    public int width;

    public Vector2Int Center()
    {
        return new Vector2Int(x + width / 2, y + height / 2);
    }

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
        PlaceEmptyRectangularRooms(roomsAmount, maxRoomSize);
        ConnectRooms();

        GameObject[,] result = GenerateTiles();
        return result;
    }

    private void RemoveAllChildren()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }

    //private FloorFieldType[,] ConnectRooms()
    //{
    //    for (int i=0; i < Rooms.Count - 1; i++)
    //    {
    //        Vector2Int roomA = Rooms[i].Center();
    //        Vector2Int roomB = Rooms[i+1].Center();

    //        CreateCorridorLShape(roomA, roomB);
    //    }
    //    return FloorFieldTypes;
    //}    
    
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
            FloorFieldTypes[x, y] = FloorFieldType.CORRIDOR_FIELD;
            x += (b.x > x) ? 1 : -1;
        }

        while (y != b.y)
        {
            FloorFieldTypes[x, y] = FloorFieldType.CORRIDOR_FIELD;
            y += (b.y > y) ? 1 : -1;
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

    private FloorFieldType[,] PlaceEmptyRectangularRooms(int roomsCount, int maxRoomSize)
    {
        int width = FloorFieldTypes.GetLength(0);
        int height = FloorFieldTypes.GetLength(1);
        int retries = 0;
        while (roomsCount > 0 && retries < MAX_RETRIES_AMOUNT_FOR_ROOM_FIT)
        {
            int x = Random.Range(0, width-1);
            int y = Random.Range(0, height - 1);
            int room_width = Random.Range(minRoomSize, maxRoomSize+1);
            int room_height = Random.Range(minRoomSize, maxRoomSize+1);

            if (CanRectangularRoomFit(x, y, room_width, room_height))
            {
                Room r = PlaceRectangularRoom(x, y, room_width, room_height);
                Rooms.Add(r);
                roomsCount--;
                retries = 0;
            }
            else
                retries++;
        }

        return FloorFieldTypes;
    }

    private Room PlaceRectangularRoom(int x, int y, int room_width, int room_height)
    {
        for (int i = x; i < x + room_width; i++)
            for (int j = y; j < y + room_height; j++)
            {
                if (i == x || j == y || i==x+room_width-1 || j==y+room_height-1)
                    FloorFieldTypes[i, j] = FloorFieldType.SIDE_FIELD;
                else
                    FloorFieldTypes[i, j] = FloorFieldType.BASE_FIELD;
            }

        FloorFieldTypes[x, y] = FloorFieldType.CORNER_FIELD;
        FloorFieldTypes[x+room_width-1, y] = FloorFieldType.CORNER_FIELD;
        FloorFieldTypes[x, y+room_height-1] = FloorFieldType.CORNER_FIELD;
        FloorFieldTypes[x+room_width-1, y + room_height - 1] = FloorFieldType.CORNER_FIELD;

        Room room = new()
        {
            x = x,
            y = y,
            width = room_width,
            height = room_height
        };

        return room;
    }

    private bool CanRectangularRoomFit(int x, int y, int room_width, int room_height)
    {
        for (int i = x; i < x + room_width; i++)
            for (int j = y; j < y + room_height; j++)
                if (i>=FloorFieldTypes.GetLength(0) || j>=FloorFieldTypes.GetLength(1) || FloorFieldTypes[i, j] != FloorFieldType.EMPTY)
                    return false;
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
