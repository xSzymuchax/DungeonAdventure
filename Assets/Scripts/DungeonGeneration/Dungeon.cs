using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{
    private FloorFieldType[,] fieldTypes;
    private GameObject[,] fieldObjects;
    private TileInfo[,] tilesInfo;
    private Position2D spawnPointPosition;

    public Dungeon(FloorFieldType[,] fieldTypes, GameObject[,] fieldObjects, Position2D spawnPointPosition)
    {
        this.fieldTypes = fieldTypes;
        this.fieldObjects = fieldObjects;
        tilesInfo = GetTilesInfo(fieldObjects);
        this.spawnPointPosition = spawnPointPosition;
    }

    private TileInfo[,] GetTilesInfo(GameObject[,] fieldObjects)
    {
        int width = fieldObjects.GetLength(0);
        int height = fieldObjects.GetLength(1);

        TileInfo[,] ti = new TileInfo[width, height];

        for (int i=0;i<width;i++)
            for (int j = 0; j < height; j++)
            {
                if (fieldObjects[i,j] != null)
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

    public Position2D GetSpawnPoint()
    {
        return spawnPointPosition;
    }
}
