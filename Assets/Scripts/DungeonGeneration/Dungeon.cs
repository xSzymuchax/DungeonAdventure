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
    private int roomCount;
    private Dictionary<IActor, Position2D> actorsPositions;
    private Dictionary<IActor, GameObject> actorsObjects;
    private bool tilesInfoReady = false;

    private float SHOW_TILE_ANIMATION_TIME = 0.1f;

    public Dungeon(FloorFieldType[,] fieldTypes, GameObject[,] fieldObjects, Position2D spawnPointPosition, int roomCount)
    {
        this.fieldTypes = fieldTypes;
        this.fieldObjects = fieldObjects;
        tilesInfo = GetTilesInfo(fieldObjects);
        this.spawnPointPosition = spawnPointPosition;
        this.roomCount = roomCount;
        actorsPositions = new();
        actorsObjects = new();
    }

    public int RoomCount => roomCount;

    public int CountLivingEnemies()
    {
        int count = 0;
        foreach (IActor actor in actorsPositions.Keys)
        {
            if (actor is EnemyCharacter enemy && !enemy.IsDead)
                count++;
        }

        return count;
    }

    public bool TryGetRandomFreeBaseField(out Position2D position)
    {
        List<Position2D> free = new();
        int width = fieldTypes.GetLength(0);
        int height = fieldTypes.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fieldTypes[x, y] != FloorFieldType.BASE_FIELD)
                    continue;
                if (tilesInfo[x, y] != null && tilesInfo[x, y].isOccupied)
                    continue;

                Position2D tile = new() { x = x, y = y };
                if (TryGetActorAt(tile, out _))
                    continue;

                free.Add(tile);
            }
        }

        if (free.Count == 0)
        {
            position = default;
            return false;
        }

        position = free[UnityEngine.Random.Range(0, free.Count)];
        return true;
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

    public HashSet<IActor> CheckPlayerSeeActors(IActor playerActor, HashSet<Position2D> visibleFields)
    {
        Position2D playerPosition = actorsPositions[playerActor];
        HashSet<IActor> seenActors = new();

        foreach (var a in actorsPositions)
        {
            if (a.Key == playerActor) continue;

            if (visibleFields.Contains(a.Value))
            {
                actorsObjects[a.Key].transform.Find(Consts.GRAPHIC_REPRESENTATION_IN_ACTOR_NAME).gameObject.SetActive(true);
                seenActors.Add(a.Key);
            }
                
            else
                actorsObjects[a.Key].transform.Find(Consts.GRAPHIC_REPRESENTATION_IN_ACTOR_NAME).gameObject.SetActive(false);

        }

        return seenActors;
    }

    public Position2D GetRandomBaseField()
    {
        int x=-1, y=-1;
        int width = fieldObjects.GetLength(0);
        int height = fieldObjects.GetLength(1);
        while (x < 0 || y < 0 || x >= width || y >= height || tilesInfo[x,y] == null || tilesInfo[x,y].type != FloorFieldType.BASE_FIELD)
        {
            x = Random.Range(0, width);
            y = Random.Range(0, height);
        }

        return tilesInfo[x, y].position;
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
            tilesInfo[p.x, p.y].Dim();
        }
    }

    public (GameObject go, Vector3 start, Vector3 target) ShowField(Position2D p)
    {
        if (tilesInfo[p.x, p.y].wasSeen == true)
        {
            tilesInfo[p.x, p.y].Brighten();
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

    public bool TryGetActorAt(Position2D position, out IActor actor)
    {
        foreach (var entry in actorsPositions)
        {
            if (entry.Value == position)
            {
                actor = entry.Key;
                return true;
            }
        }

        actor = null;
        return false;
    }

    public bool TryGetDamagableAt(Position2D position, out IDamagable damagable)
    {
        if (TryGetActorAt(position, out IActor actor) && actor is IDamagable found && !found.IsDead)
        {
            damagable = found;
            return true;
        }

        damagable = null;
        return false;
    }

    public void RemoveActor(IActor actor)
    {
        if (!actorsPositions.TryGetValue(actor, out Position2D position))
            return;

        SetTileFieldUnoccupied(position);
        actorsPositions.Remove(actor);
        actorsObjects.Remove(actor);
    }

    public bool IsWall(Position2D tile)
    {
        if (tile.x < 0 || tile.y < 0 || tile.x >= tilesInfo.GetLength(0) || tile.y >= tilesInfo.GetLength(1))
            return true;

        TileInfo info = tilesInfo[tile.x, tile.y];
        if (info == null)
            return true;

        return info.type == FloorFieldType.SIDE_FIELD || info.type == FloorFieldType.EMPTY;
    }

    public Vector3 GetTileWorldPosition(Position2D tile)
    {
        if (tile.x >= 0 && tile.y >= 0 && tile.x < fieldObjects.GetLength(0) && tile.y < fieldObjects.GetLength(1)
            && fieldObjects[tile.x, tile.y] != null)
            return fieldObjects[tile.x, tile.y].transform.position;

        return new Vector3(
            tile.x * Consts.TILE_SIZE + 0.5f * Consts.TILE_SIZE,
            0f,
            tile.y * Consts.TILE_SIZE + 0.5f * Consts.TILE_SIZE);
    }

    public void AddActor(IActor actor, Position2D position, GameObject actorObject)
    {
        actorsPositions.Add(actor, position);
        actorsObjects.Add(actor, actorObject);
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

    public void RestoreSeen(bool[,] seen)
    {
        if (seen == null)
            return;

        int width = tilesInfo.GetLength(0);
        int height = tilesInfo.GetLength(1);
        int seenWidth = seen.GetLength(0);
        int seenHeight = seen.GetLength(1);
        for (int x = 0; x < width && x < seenWidth; x++)
        {
            for (int y = 0; y < height && y < seenHeight; y++)
            {
                TileInfo tile = tilesInfo[x, y];
                if (tile == null || !seen[x, y])
                    continue;

                tile.wasSeen = true;
                tile.gameObject.SetActive(true);
                tile.Dim();
            }
        }
    }

    public bool TryFindField(FloorFieldType type, out Position2D position)
    {
        int width = fieldTypes.GetLength(0);
        int height = fieldTypes.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fieldTypes[x, y] != type)
                    continue;

                position = new Position2D { x = x, y = y };
                return true;
            }
        }

        position = default;
        return false;
    }
}
