using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

enum FloorFieldType
{
    EMPTY = 0, BASE_FIELD = 1, SIDE_FIELD = 2, CORNER_FIELD = 3
}

public class DungeonFloorGenerator : MonoBehaviour
{
    GameObject[,] DungeonFloor;
    readonly float TILE_SIZE = 10;
    readonly int MAX_RETRIES_AMOUNT_FOR_ROOM_FIT = 100;
    Vector3[] mapBorderPoints;

    public int width = 20;
    public int height = 20;
    public int roomsAmount = 3;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;
    public GameObject BaseFloorTile;
    public GameObject CornerTile;
    public GameObject SideTile;

    void Start()
    {
        DungeonFloor = GenerateFloor(width, height);
        GenerateGizmosBorderPoints(width, height);
    }

    private void GenerateGizmosBorderPoints(int width, int height)
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

    public GameObject[,] GenerateFloor(int width, int height)
    {
        FloorFieldType[,] floor = GenerateEmptyFloor(width, height);
        //floor = FillRandomBaseFields(floor, 1);
        floor = PlaceEmptyRectangularRooms(floor, roomsAmount, maxRoomSize);

        GameObject[,] result = GenerateTiles(floor);
        return result;
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

    private FloorFieldType[,] PlaceEmptyRectangularRooms(FloorFieldType[,] floor, int roomsCount, int maxRoomSize)
    {
        int width = floor.GetLength(0);
        int height = floor.GetLength(1);
        int retries = 0;
        while (roomsCount > 0 && retries < MAX_RETRIES_AMOUNT_FOR_ROOM_FIT)
        {
            int x = Random.Range(0, width-1);
            int y = Random.Range(0, height - 1);
            int room_width = Random.Range(minRoomSize, maxRoomSize+1);
            int room_height = Random.Range(minRoomSize, maxRoomSize+1);

            if (CanRectangularRoomFit(floor, x, y, room_width, room_height))
            {
                floor = PlaceRectangularRoom(floor, x, y, room_width, room_height);
                roomsCount--;
                retries = 0;
            }
            else
                retries++;
        }

        return floor;
    }

    private FloorFieldType[,] PlaceRectangularRoom(FloorFieldType[,] floor, int x, int y, int room_width, int room_height)
    {
        for (int i = x; i < x + room_width; i++)
            for (int j = y; j < y + room_height; j++)
            {
                if (i == x || j == y || i==x+room_width-1 || j==y+room_height-1)
                    floor[i, j] = FloorFieldType.SIDE_FIELD;
                else
                    floor[i, j] = FloorFieldType.BASE_FIELD;
            }

        floor[x, y] = FloorFieldType.CORNER_FIELD;
        floor[x+room_width-1, y] = FloorFieldType.CORNER_FIELD;
        floor[x, y+room_height-1] = FloorFieldType.CORNER_FIELD;
        floor[x+room_width-1, y + room_height - 1] = FloorFieldType.CORNER_FIELD;
                
        return floor;
    }

    private bool CanRectangularRoomFit(FloorFieldType[,] floor, int x, int y, int room_width, int room_height)
    {
        for (int i = x; i < x + room_width; i++)
            for (int j = y; j < y + room_height; j++)
                if (i>=floor.GetLength(0) || j>=floor.GetLength(1) || floor[i, j] != FloorFieldType.EMPTY)
                    return false;
        return true;
    }

    private FloorFieldType[,] FillRandomBaseFields(FloorFieldType[,] floor, float chanceOfFill)
    {
        int width = floor.GetLength(0);
        int height = floor.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Random.value <= chanceOfFill)
                    floor[x, y] = FloorFieldType.BASE_FIELD;
            }
        }
        return floor;
    }

    private GameObject[,] GenerateTiles(FloorFieldType[,] floor)
    {
        int width = floor.GetLength(0);
        int height = floor.GetLength(1);

        GameObject[,] result = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject go = FindObjectRelatedToFloorType(floor[x, y]);

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

            default:
                return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLineList(mapBorderPoints);
    }
}
