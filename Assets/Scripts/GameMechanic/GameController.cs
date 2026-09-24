//using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public Transform playerHolder;
    public GameObject playerPrefab;

    public Camera playerCamera;
    public GameObject dungeonHolder;

    public DungeonBiomePrefabSet mapPrefabSet;
    private DungeonFloorGenerator dungeonFloorGenerator;
    public Dungeon dungeon;
    private GameObject player;
    private PlayerCharacter playerCharacter;
    private TurnsController turnsController;

    public GameObject enemyPrefab;
    public EnemyCatalog enemyCatalog;
    public Transform enemyHolder;

    private HashSet<Position2D> lastSeenFields = new();
    private HashSet<IActor> lastSeenActors = new();
    
    public MovementSystem movementSystem;
    public VisionSystem visionSystem;
    public FightingSystem fightingSystem;
    public SaveSystem saveSystem;

    void Start()
    {
        Instance = this;
        turnsController = new(10);
        saveSystem = new SaveSystem();

        GenerateDungeon();
        saveSystem.ResetToSingleFloor(dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), null);

        movementSystem = gameObject.AddComponent<MovementSystem>();
        movementSystem.dungeonFloor = dungeon;

        visionSystem = gameObject.AddComponent<VisionSystem>();

        fightingSystem = gameObject.AddComponent<FightingSystem>();

        SpawnPlayer();
        SpawnEnemy();
        //SpawnEnemy();

        turnsController.SetPlayer(playerCharacter);

        // player
        dungeon.AddActor(playerCharacter, dungeon.GetSpawnPoint(), player);

        CheckPlayerPerception();
    }

    public void SpawnEnemy()
    {
        while (true)
        {
            FloorFieldType[,] floorFieldType = dungeon.GetFieldTypes();
            int x = Random.Range(0, floorFieldType.GetLength(0));
            int y = Random.Range(0, floorFieldType.GetLength(1));

            if (floorFieldType[x,y] == FloorFieldType.BASE_FIELD)
            {
                GameObject enemyGo = Instantiate(enemyPrefab, enemyHolder);
                dungeon.AddActor(enemyGo.GetComponent<EnemyCharacter>(), new() { x=x,y=y}, enemyGo);
                enemyGo.GetComponent<EnemyCharacter>().Position = new() { x = x, y = y };
                turnsController.AddEnemy(enemyGo.GetComponent<EnemyController>());
                enemyGo.transform.position = dungeon.GetFieldObjects()[x, y].GetComponent<Transform>().position;
                enemyGo.GetComponent<EnemyController>().SetChasedCharacter(player.GetComponent<IActor>());
                dungeon.GetTileInfos()[x, y].isOccupied = true;
                break;
            }
        }
    }

    [ContextMenu("Restart")]
    public void RestartGeneration()
    {
        Destroy(player);
        turnsController = new(10);
        GenerateDungeon();
        saveSystem.ResetToSingleFloor(dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), null);
        SpawnPlayer();
        SpawnEnemy();
        turnsController.SetPlayer(playerCharacter);
        dungeon.AddActor(playerCharacter, dungeon.GetSpawnPoint(), player);
        CheckPlayerPerception();
    }

    public void GenerateDungeon()
    {
        dungeonHolder.AddComponent<DungeonFloorGenerator>();
        dungeonFloorGenerator = dungeonHolder.GetComponent<DungeonFloorGenerator>();
        dungeonFloorGenerator.SetDungeonParameters(20, 20, 5, 4, 8, mapPrefabSet);
        dungeon = dungeonFloorGenerator.GetGeneratedFloor();
        DestroyImmediate(dungeonHolder.GetComponent<DungeonFloorGenerator>());
    }

    public bool TryChangeFloor(Position2D tile)
    {
        FloorFieldType stepped = dungeon.GetFieldTypes()[tile.x, tile.y];
        if (stepped == FloorFieldType.EXIT_FIELD)
        {
            playerCharacter.CurrentPath.Clear();
            GoToNextFloor();
            return true;
        }

        if (stepped == FloorFieldType.SPAWN_FIELD && GoToPreviousFloor())
        {
            playerCharacter.CurrentPath.Clear();
            return true;
        }

        return false;
    }

    public void GoToNextFloor()
    {
        saveSystem.RememberFloor(saveSystem.CurrentFloorIndex, dungeon, FloorEnemies(), true);
        ClearEnemies();

        int next = saveSystem.CurrentFloorIndex + 1;
        if (next < saveSystem.FloorCount)
            dungeon = RebuildSavedFloor(next);
        else
            GenerateDungeon();

        movementSystem.dungeonFloor = dungeon;
        PlacePlayer(dungeon.GetSpawnPoint());
        SpawnSavedEnemies(next);
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(next, dungeon, null, false);
        saveSystem.Save();
    }

    public bool GoToPreviousFloor()
    {
        if (saveSystem.CurrentFloorIndex <= 0)
        {
            Debug.Log("cant go back");
            return false;
        }

        saveSystem.RememberFloor(saveSystem.CurrentFloorIndex, dungeon, FloorEnemies(), true);
        ClearEnemies();
        int previous = saveSystem.CurrentFloorIndex - 1;
        dungeon = RebuildSavedFloor(previous);
        movementSystem.dungeonFloor = dungeon;

        Position2D arrival = dungeon.GetSpawnPoint();
        if (dungeon.TryFindField(FloorFieldType.EXIT_FIELD, out Position2D exit))
            arrival = exit;

        PlacePlayer(arrival);
        SpawnSavedEnemies(previous);
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(previous, dungeon, null, false);
        saveSystem.Save();
        return true;
    }

    [ContextMenu("Save")]
    public void SaveGame()
    {
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(saveSystem.CurrentFloorIndex, dungeon, FloorEnemies(), true);
        saveSystem.Save();
    }

    [ContextMenu("Load")]
    public void LoadGame()
    {
        if (saveSystem == null || !saveSystem.Load())
        {
            Debug.Log("cant load");
            return;
        }

        ClearEnemies();
        dungeon = RebuildSavedFloor(saveSystem.CurrentFloorIndex);
        movementSystem.dungeonFloor = dungeon;
        saveSystem.ApplyPlayer(playerCharacter);

        Position2D tile = dungeon.GetSpawnPoint();
        PlayerSave playerSave = saveSystem.Player;
        if (playerSave != null && TileExists(playerSave.x, playerSave.y))
            tile = new Position2D { x = playerSave.x, y = playerSave.y };

        PlacePlayer(tile);
        SpawnSavedEnemies(saveSystem.CurrentFloorIndex);
    }

    private Dungeon RebuildSavedFloor(int index)
    {
        dungeonHolder.AddComponent<DungeonFloorGenerator>();
        DungeonFloorGenerator generator = dungeonHolder.GetComponent<DungeonFloorGenerator>();
        FloorFieldType[,] fields = saveSystem.GetFields(index);
        generator.SetDungeonParameters(fields.GetLength(0), fields.GetLength(1), 5, 4, 8, mapPrefabSet);
        Dungeon built = generator.BuildFromFieldTypes(fields, saveSystem.GetSpawn(index));
        built.RestoreSeen(saveSystem.GetSeen(index));
        DestroyImmediate(generator);
        return built;
    }

    private void ClearEnemies()
    {
        EnemyCharacter[] characters = enemyHolder.GetComponentsInChildren<EnemyCharacter>(true);
        for (int i = 0; i < characters.Length; i++)
        {
            dungeon.RemoveActor(characters[i]);
            Destroy(characters[i].gameObject);
        }

        turnsController.ClearEnemies();
    }

    private List<EnemyCharacter> FloorEnemies()
    {
        return new List<EnemyCharacter>(enemyHolder.GetComponentsInChildren<EnemyCharacter>(true));
    }

    private void SpawnSavedEnemies(int floorIndex)
    {
        EnemySave[] enemies = saveSystem.GetEnemies(floorIndex);
        for (int i = 0; i < enemies.Length; i++)
            SpawnSavedEnemy(enemies[i]);
    }

    private void SpawnSavedEnemy(EnemySave save)
    {
        if (save == null)
            return;

        if (enemyCatalog == null || !enemyCatalog.TryGet(save.enemyId, out GameObject prefab))
        {
            Debug.Log("cant spawn enemy " + save.enemyId);
            return;
        }

        if (!TileExists(save.x, save.y))
            return;

        GameObject enemyGo = Instantiate(prefab, enemyHolder);
        EnemyCharacter character = enemyGo.GetComponent<EnemyCharacter>();
        saveSystem.ApplyEnemy(character, save);

        Position2D position = new() { x = save.x, y = save.y };
        character.Position = position;
        enemyGo.transform.position = dungeon.GetTileWorldPosition(position);
        if (character.IsDead)
        {
            character.HideDestroyed();
            return;
        }

        dungeon.AddActor(character, position, enemyGo);
        dungeon.GetTileInfos()[position.x, position.y].isOccupied = true;

        EnemyController controller = enemyGo.GetComponent<EnemyController>();
        controller.SetChasedCharacter(playerCharacter);
        turnsController.AddEnemy(controller);
    }

    private bool TileExists(int x, int y)
    {
        FloorFieldType[,] fields = dungeon.GetFieldTypes();
        return x >= 0 && y >= 0 && x < fields.GetLength(0) && y < fields.GetLength(1) && fields[x, y] != FloorFieldType.EMPTY;
    }

    private void PlacePlayer(Position2D tile)
    {
        lastSeenFields.Clear();
        lastSeenActors.Clear();
        playerCharacter.CurrentPath.Clear();
        playerCharacter.Position = tile;

        Vector3 world = dungeon.GetTileWorldPosition(tile);
        player.transform.position = world;
        if (playerCharacter.Representation != null)
            playerCharacter.Representation.transform.position = world;

        dungeon.AddActor(playerCharacter, tile, player);
        dungeon.GetTileInfos()[tile.x, tile.y].isOccupied = true;
        CheckPlayerPerception();
    }

    public void LoadPrefabSet(DungeonBiomePrefabSet dungeonBiomePrefabSet)
    {
        mapPrefabSet = dungeonBiomePrefabSet;
    }

    private void SpawnPlayer()
    {
        GameObject p = Instantiate(playerPrefab, playerHolder);
        playerCamera.GetComponent<CameraController>().SetTarget(p.transform);
        p.GetComponent<PlayerController>().SetRaySource(playerCamera.transform);
        player = p;
        Position2D spawn = dungeon.GetSpawnPoint();
        p.transform.position = dungeon.GetFieldObjects()[spawn.x, spawn.y].GetComponent<Transform>().position;
        dungeon.GetTileInfos()[spawn.x, spawn.y].isOccupied = true;
        playerCharacter = player.GetComponent<PlayerCharacter>();
        playerCharacter.Position = spawn;
        PlayerReady?.Invoke(playerCharacter);
    }

    public PlayerCharacter Player => playerCharacter;
    public event System.Action<PlayerCharacter> PlayerReady;

    public bool CheckPlayerPerception()
    {
        bool interruptionEvent = false;
        CheckPlayerMapPerception();
        interruptionEvent = interruptionEvent || CheckPlayerEnemiesPerception();
        return interruptionEvent;
    }

    private void CheckPlayerMapPerception()
    {
        HashSet<Position2D> visible = new();

        visible = visionSystem.AllVisibleFields(
            dungeon.FindActorPosition(playerCharacter),
            playerCharacter.ViewRange,
            dungeon);

        var showed = dungeon.ShowFields(visible);
        foreach (var s in showed)
            StartCoroutine(dungeon.ShowFieldCoroutine(s.go, s.start, s.end));

        foreach (var v in visible)
            if (lastSeenFields.Contains(v))
                lastSeenFields.Remove(v);
        dungeon.SetHiddenVisited(lastSeenFields);

        lastSeenFields = visible;
    }

    private bool CheckPlayerEnemiesPerception()
    {
        var currentSeen = dungeon.CheckPlayerSeeActors(playerCharacter, lastSeenFields);

        foreach (var s in currentSeen)
            if (!lastSeenActors.Contains(s))
            {
                lastSeenActors = currentSeen;
                return true;
            }

        lastSeenActors = currentSeen;
        return false;
    }

    public void RequestCalculatePath(IWalkable actor, Position2D tile)
    {
        movementSystem.RecalculatePath(actor, tile);
    }

    public IEnumerator EvaluateTurn()
    {
        yield return turnsController.EvaluateTurn();
        yield return movementSystem.PlayPendingAnimations();
    }

    public Position2D GetPositionOfActor(IActor actor)
    {
        return dungeon.FindActorPosition(actor);
    }

    public IEnumerator RequestWaitTurn(IActor actor)
    {
        if (!actor.HasEnergy)
            yield break;

        actor.RemoveEnergy(Consts.GAME_SPEED);
    }

    public void NotifyDestroyed(IDamagable destroyed)
    {
        if (destroyed is not IActor actor)
            return;

        if (!ReferenceEquals(actor, playerCharacter))
            dungeon.RemoveActor(actor);

        if (destroyed is Component component)
        {
            EnemyController enemy = component.GetComponent<EnemyController>();
            if (enemy != null)
                turnsController.RemoveEnemy(enemy);
        }
    }
}
