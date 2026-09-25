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

    public EnemyCatalog enemyCatalog;
    public Transform enemyHolder;

    private HashSet<Position2D> lastSeenFields = new();
    private HashSet<IActor> lastSeenActors = new();
    
    public MovementSystem movementSystem;
    public VisionSystem visionSystem;
    public FightingSystem fightingSystem;
    public SaveSystem saveSystem;
    private EnemySpawnSystem enemySpawnSystem;

    void Start()
    {
        Instance = this;
        turnsController = new(10);
        saveSystem = new SaveSystem();
        enemySpawnSystem = new EnemySpawnSystem(enemyCatalog, enemyHolder, turnsController, saveSystem);

        GenerateDungeon();
        saveSystem.ResetToSingleFloor(dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), null, dungeon.RoomCount);

        movementSystem = gameObject.AddComponent<MovementSystem>();
        movementSystem.dungeonFloor = dungeon;
        enemySpawnSystem.Bind(dungeon);

        visionSystem = gameObject.AddComponent<VisionSystem>();

        fightingSystem = gameObject.AddComponent<FightingSystem>();

        SpawnPlayer();
        enemySpawnSystem.SetPlayer(playerCharacter);
        enemySpawnSystem.SpawnInitial();

        turnsController.SetPlayer(playerCharacter);

        // player
        dungeon.AddActor(playerCharacter, dungeon.GetSpawnPoint(), player);

        CheckPlayerPerception();
    }

    [ContextMenu("Restart")]
    public void RestartGeneration()
    {
        Destroy(player);
        ClearEnemies();
        turnsController = new(10);
        enemySpawnSystem.BindTurns(turnsController);
        GenerateDungeon();
        saveSystem.ResetToSingleFloor(dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), null, dungeon.RoomCount);
        movementSystem.dungeonFloor = dungeon;
        enemySpawnSystem.Bind(dungeon);
        SpawnPlayer();
        enemySpawnSystem.SetPlayer(playerCharacter);
        enemySpawnSystem.SpawnInitial();
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
        bool restored = next < saveSystem.FloorCount;
        if (restored)
            dungeon = RebuildSavedFloor(next);
        else
            GenerateDungeon();

        movementSystem.dungeonFloor = dungeon;
        enemySpawnSystem.Bind(dungeon);
        PlacePlayer(dungeon.GetSpawnPoint());
        if (restored)
            enemySpawnSystem.SpawnSaved(saveSystem.GetEnemies(next));
        else
            enemySpawnSystem.SpawnInitial();
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(next, dungeon, FloorEnemies(), true);
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
        enemySpawnSystem.Bind(dungeon);

        Position2D arrival = dungeon.GetSpawnPoint();
        if (dungeon.TryFindField(FloorFieldType.EXIT_FIELD, out Position2D exit))
            arrival = exit;

        PlacePlayer(arrival);
        enemySpawnSystem.SpawnSaved(saveSystem.GetEnemies(previous));
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(previous, dungeon, FloorEnemies(), true);
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
        enemySpawnSystem.Bind(dungeon);
        saveSystem.ApplyPlayer(playerCharacter);

        Position2D tile = dungeon.GetSpawnPoint();
        PlayerSave playerSave = saveSystem.Player;
        if (playerSave != null && TileExists(playerSave.x, playerSave.y))
            tile = new Position2D { x = playerSave.x, y = playerSave.y };

        PlacePlayer(tile);
        enemySpawnSystem.SpawnSaved(saveSystem.GetEnemies(saveSystem.CurrentFloorIndex));
    }

    private Dungeon RebuildSavedFloor(int index)
    {
        dungeonHolder.AddComponent<DungeonFloorGenerator>();
        DungeonFloorGenerator generator = dungeonHolder.GetComponent<DungeonFloorGenerator>();
        FloorFieldType[,] fields = saveSystem.GetFields(index);
        generator.SetDungeonParameters(fields.GetLength(0), fields.GetLength(1), 5, 4, 8, mapPrefabSet);
        Dungeon built = generator.BuildFromFieldTypes(fields, saveSystem.GetSpawn(index), saveSystem.GetRoomCount(index));
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
        if (turnsController.TurnElapsed && playerCharacter != null && !playerCharacter.IsDead && enemySpawnSystem.TrySpawnAmbient())
            CheckPlayerPerception();
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
