using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{
    private FloorFieldType[,] fieldTypes;
    private GameObject[,] fieldObjects;
    private TileInfo[,] tilesInfo;

    public Dungeon(FloorFieldType[,] fieldTypes, GameObject[,] fieldObjects)
    {
        this.fieldTypes = fieldTypes;
        this.fieldObjects = fieldObjects;
        tilesInfo = GetTilesInfo(fieldObjects);
    }

    private TileInfo[,] GetTilesInfo(GameObject[,] fieldObjects)
    {
        int width = fieldObjects.GetLength(0);
        int height = fieldObjects.GetLength(1);

        TileInfo[,] ti = new TileInfo[width, height];

        for (int i=0;i<width;i++)
            for (int j = 0; j < height; j++)
            {
                ti[i, j] = fieldObjects[i, j].GetComponent<TileInfo>();
            }

        return ti;
    }

    public FloorFieldType[,] GetFieldTypes()
    {
        return fieldTypes;
    }

    public GameObject[,] GetFieldObjects()
    {
        return fieldObjects;
    }

    public TileInfo[,] GetTileInfos()
    {
        return tilesInfo;
    }
}
