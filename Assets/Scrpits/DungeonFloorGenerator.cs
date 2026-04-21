using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

enum FloorFieldType
{
    EMPTY = 0, BASE_FIELD = 1
}

public class DungeonFloorGenerator : MonoBehaviour
{
    GameObject[,] DungeonFloor;
    float TileSize = 10;

    public GameObject BaseFloorTile;

    void Start()
    {
        DungeonFloor = GenerateFloor(10, 8);
    }

    public GameObject[,] GenerateFloor(int width, int height)
    {
        FloorFieldType[,]   floor = GenerateEmptyFloor(width, height);
                            floor = FillRandomBaseFields(floor, 1);

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

                Vector3 posVector = transform.position + new Vector3(x*TileSize, 0, y*TileSize);
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
            case FloorFieldType.BASE_FIELD:
                return Instantiate(BaseFloorTile, transform);
            default:
                return null;
        }
    }
}
