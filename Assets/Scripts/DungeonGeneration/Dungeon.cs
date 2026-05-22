using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{
    private FloorFieldType[,] fieldTypes;
    private GameObject[,] fieldObjects;
    private TileInfo[,] tilesInfo;
    private Position2D spawnPointPosition;
    private Dictionary<IActor, Position2D> actorsPositions;

    public Dungeon(FloorFieldType[,] fieldTypes, GameObject[,] fieldObjects, Position2D spawnPointPosition)
    {
        this.fieldTypes = fieldTypes;
        this.fieldObjects = fieldObjects;
        tilesInfo = GetTilesInfo(fieldObjects);
        this.spawnPointPosition = spawnPointPosition;
        actorsPositions = new();
    }

    public Vector3 MoveActor(IActor actor, Position2D position)
    {
        SetTileFieldUnoccupied(actorsPositions[actor]);
        actorsPositions[actor] = position;
        SetTileFieldOccupied(position);

        if (actor is MonoBehaviour mb)
        {
            //mb.transform.position = fieldObjects[position.x, position.y].transform.position;
            return fieldObjects[position.x, position.y].transform.position;
        }
        else
            Debug.Log("nie jest");

        return new(); // TODO - pamiêtaæ, tutaj moze kiedys byc blad
    }

    private void SetTileFieldUnoccupied(Position2D position)
    {
        tilesInfo[position.x, position.y].isOccupied = false;
    }

    private void SetTileFieldOccupied(Position2D position)
    {
        tilesInfo[position.x, position.y].isOccupied = true;
    }

    public Position2D FindActorPosition(IActor actor)
    {
        Position2D result;
        if (actorsPositions.TryGetValue(actor, out result))
            return result;
        return new() { x=-1, y=-1};
    }

    public void AddActor(IActor actor, Position2D position)
    {
        actorsPositions.Add(actor, position);
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
