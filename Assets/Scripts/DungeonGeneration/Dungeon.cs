using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Dungeon
{
    private FloorFieldType[,] fieldTypes;
    private GameObject[,] fieldObjects;
    private TileInfo[,] tilesInfo;
    private Position2D spawnPointPosition;
    private Dictionary<IActor, Position2D> actorsPositions;
    private bool tilesInfoReady = false;

    private float SHOW_TILE_ANIMATION_TIME = 0.1f;

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

    public List<(GameObject go, Vector3 start, Vector3 end)> ShowFields(HashSet<Position2D> positions)
    {
        List<(GameObject go, Vector3, Vector3)> result = new();
        foreach (Position2D p in positions)
        {
            var f = ShowField(p);
            if (f.go == null)
                continue;
            result.Add(f);
        }
            
        return result;
    }

    public IEnumerator ShowFieldCoroutine(GameObject go, Vector3 startPosition, Vector3 targetPosition)
    {
        float elapsed = 0f;
        while (elapsed < SHOW_TILE_ANIMATION_TIME)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / SHOW_TILE_ANIMATION_TIME;

            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            go.transform.position = currentPosition;
            go.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            yield return null;
        }

        go.transform.position = targetPosition;
        go.transform.localScale = Vector3.one;
    }

    public void SetHiddenVisited(HashSet<Position2D> positions)
    {
        foreach (var p in positions)
        {
            Color c = tilesInfo[p.x, p.y].gameObject.GetComponent<MeshRenderer>().material.color;
            c.a = 0.2f;
            tilesInfo[p.x, p.y].gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", c);
        }
    }

    public (GameObject go, Vector3 start, Vector3 target) ShowField(Position2D p)
    {
        if (tilesInfo[p.x, p.y].wasSeen == true)
        {
            Color c = tilesInfo[p.x, p.y].gameObject.GetComponent<MeshRenderer>().material.color;
            c.a = 1f;
            tilesInfo[p.x, p.y].gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", c);
            return new(null, new(-1, -1, -1), new(-1, -1, -1));
        }
            

        tilesInfo[p.x, p.y].wasSeen = true;

        GameObject go = tilesInfo[p.x, p.y].gameObject;
        go.transform.localScale = new(0.0f, 0.0f, 0.0f);
        go.SetActive(true);

        Vector3 target = go.transform.position;
        Vector3 start = go.transform.position - new Vector3(0f, 2f* Consts.TILE_SIZE, 0f);

        return (go, start, target);
    }
    public void HideFields(HashSet<Position2D> positions)
    {
        foreach (Position2D p in positions)
            HideField(p);
    }

    public void HideField(Position2D p)
    {
        tilesInfo[p.x, p.y].wasSeen = false;
        tilesInfo[p.x, p.y].gameObject.SetActive(false);
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
        if (tilesInfoReady)
            return tilesInfo;

        int width = fieldObjects.GetLength(0);
        int height = fieldObjects.GetLength(1);

        TileInfo[,] ti = new TileInfo[width, height];

        for (int i=0;i<width;i++)
            for (int j = 0; j < height; j++)
            {
                if (fieldObjects[i,j] != null)
                    ti[i, j] = fieldObjects[i, j].GetComponent<TileInfo>();
            }

        tilesInfoReady = true;
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
