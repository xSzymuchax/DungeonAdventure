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
    public bool debugMap = true;
    private DungeonFloorGenerator dungeonFloorGenerator;
    public Dungeon dungeon;
    private GameObject player;
    private PlayerCharacter playerCharacter;
    private TurnsController turnsController;

    public EnemyCatalog enemyCatalog;
    public Transform enemyHolder;
    public GameObject itemModel;
    public ItemCatalog baseItems;
    public InventoryDisplay inventoryDisplay;

    private HashSet<Position2D> lastSeenFields = new();
    private HashSet<IActor> lastSeenActors = new();
    
    public MovementSystem movementSystem;
    public VisionSystem visionSystem;
    public FightingSystem fightingSystem;
    public SaveSystem saveSystem;
    private EnemySpawnSystem enemySpawnSystem;
    private ItemSpawnSystem itemSpawnSystem;
    private ProjectileSystem projectileSystem;
    private Transform itemHolder;
    bool throwArmed;
    bool throwFromBag;
    int throwBagIndex;
    EquipmentSlot throwSlot;
    int throwArmFrame;
    bool useArmed;
    int useBagIndex;
    int useArmFrame;

    void Start()
    {
        Instance = this;
        turnsController = new(10);
        saveSystem = new SaveSystem();
        enemySpawnSystem = new EnemySpawnSystem(enemyCatalog, enemyHolder, turnsController, saveSystem);
        itemHolder = new GameObject("ItemHolder").transform;
        itemSpawnSystem = new ItemSpawnSystem(itemHolder, itemModel, baseItems);
        projectileSystem = new ProjectileSystem();

        BuildFloor();
        saveSystem.ResetToSingleFloor(dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), null, dungeon.RoomCount);

        movementSystem = gameObject.AddComponent<MovementSystem>();

        visionSystem = gameObject.AddComponent<VisionSystem>();

        fightingSystem = gameObject.AddComponent<FightingSystem>();

        SpawnPlayer();
        enemySpawnSystem.SetPlayer(playerCharacter);
        turnsController.SetPlayer(playerCharacter);
        BindInventoryUi();
        EnterFloor(dungeon, dungeon.GetSpawnPoint(), true, null, null);
    }

    [ContextMenu("Restart")]
    public void RestartGeneration()
    {
        Destroy(player);
        enemySpawnSystem.Clear();
        itemSpawnSystem.Clear();
        BuildFloor();
        saveSystem.ResetToSingleFloor(dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), null, dungeon.RoomCount);
        SpawnPlayer();
        enemySpawnSystem.SetPlayer(playerCharacter);
        turnsController.SetPlayer(playerCharacter);
        BindInventoryUi();
        EnterFloor(dungeon, dungeon.GetSpawnPoint(), true, null, null);
    }

    public void GenerateDungeon()
    {
        dungeonHolder.AddComponent<DungeonFloorGenerator>();
        dungeonFloorGenerator = dungeonHolder.GetComponent<DungeonFloorGenerator>();
        dungeonFloorGenerator.SetDungeonParameters(20, 20, 5, 4, 8, mapPrefabSet);
        dungeon = dungeonFloorGenerator.GetGeneratedFloor();
        DestroyImmediate(dungeonHolder.GetComponent<DungeonFloorGenerator>());
    }

    public void GenerateDebugMap()
    {
        dungeonHolder.AddComponent<DungeonFloorGenerator>();
        dungeonFloorGenerator = dungeonHolder.GetComponent<DungeonFloorGenerator>();
        dungeon = dungeonFloorGenerator.GenerateDebug(mapPrefabSet);
        DestroyImmediate(dungeonFloorGenerator);
    }

    void BuildFloor()
    {
        if (debugMap)
            GenerateDebugMap();
        else
            GenerateDungeon();
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
        LeaveFloor();

        int next = saveSystem.CurrentFloorIndex + 1;
        bool restored = next < saveSystem.FloorCount;
        if (restored)
            dungeon = RebuildSavedFloor(next);
        else
            GenerateDungeon();

        EnterFloor(dungeon, dungeon.GetSpawnPoint(), !restored, restored ? saveSystem.GetEnemies(next) : null, restored ? saveSystem.GetItems(next) : null);
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(next, dungeon, enemySpawnSystem.FloorEnemies(), true);
        saveSystem.Save();
    }

    public bool GoToPreviousFloor()
    {
        if (saveSystem.CurrentFloorIndex <= 0)
        {
            Debug.Log("cant go back");
            return false;
        }

        LeaveFloor();
        int previous = saveSystem.CurrentFloorIndex - 1;
        dungeon = RebuildSavedFloor(previous);

        Position2D arrival = dungeon.GetSpawnPoint();
        if (dungeon.TryFindField(FloorFieldType.EXIT_FIELD, out Position2D exit))
            arrival = exit;

        EnterFloor(dungeon, arrival, false, saveSystem.GetEnemies(previous), saveSystem.GetItems(previous));
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(previous, dungeon, enemySpawnSystem.FloorEnemies(), true);
        saveSystem.Save();
        return true;
    }

    [ContextMenu("Save")]
    public void SaveGame()
    {
        saveSystem.RememberPlayer(playerCharacter);
        saveSystem.RememberFloor(saveSystem.CurrentFloorIndex, dungeon, enemySpawnSystem.FloorEnemies(), true);
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

        enemySpawnSystem.Clear();
        dungeon = RebuildSavedFloor(saveSystem.CurrentFloorIndex);
        saveSystem.ApplyPlayer(playerCharacter);

        Position2D tile = dungeon.GetSpawnPoint();
        PlayerSave playerSave = saveSystem.Player;
        if (playerSave != null && dungeon.TileExists(playerSave.x, playerSave.y))
            tile = new Position2D { x = playerSave.x, y = playerSave.y };

        EnterFloor(dungeon, tile, false, saveSystem.GetEnemies(saveSystem.CurrentFloorIndex), saveSystem.GetItems(saveSystem.CurrentFloorIndex));
    }

    private void LeaveFloor()
    {
        saveSystem.RememberFloor(saveSystem.CurrentFloorIndex, dungeon, enemySpawnSystem.FloorEnemies(), true);
        enemySpawnSystem.Clear();
        itemSpawnSystem.Clear();
    }

    private void EnterFloor(Dungeon floor, Position2D arrival, bool freshContent, EnemySave[] savedEnemies, ItemSave[] savedItems)
    {
        dungeon = floor;
        movementSystem.dungeonFloor = dungeon;
        enemySpawnSystem.Bind(dungeon);
        itemSpawnSystem.Bind(dungeon);
        projectileSystem.Bind(dungeon);
        PlacePlayer(arrival);
        if (freshContent && debugMap)
        {
            Position2D dummy = new Position2D { x = 5, y = 5 };
            itemSpawnSystem.SpawnAll(dummy);
            enemySpawnSystem.SpawnDummy(EnemySpawnSystem.AmbientEnemyId, dummy);
        }
        else if (freshContent)
        {
            enemySpawnSystem.SpawnInitial();
            itemSpawnSystem.SpawnInitial();
        }
        else
        {
            enemySpawnSystem.SpawnSaved(savedEnemies);
            itemSpawnSystem.SpawnSaved(savedItems);
        }
        CheckPlayerPerception();
    }

    public IEnumerator TryPickupItem(Position2D tile)
    {
        if (playerCharacter == null || !dungeon.HasGroundItem(tile))
            yield break;

        IAction action = new PickupAction(playerCharacter, tile, dungeon);
        yield return action.PerformAction();
    }

    public void BeginPlayerAction(IAction action)
    {
        StartCoroutine(RunPlayerAction(action));
    }

    IEnumerator RunPlayerAction(IAction action)
    {
        if (playerCharacter == null || playerCharacter.Energy <= 0 || action == null)
            yield break;

        if (playerCharacter.CurrentPath != null)
            playerCharacter.CurrentPath.Clear();

        yield return fightingSystem.TickTokens(playerCharacter);
        if (playerCharacter.IsDead || playerCharacter.Energy <= 0)
            yield break;

        float energy = playerCharacter.Energy;
        yield return action.PerformAction();
        if (playerCharacter.IsDead || playerCharacter.Energy >= energy)
            yield break;

        yield return EvaluateTurn();
    }

    void BindInventoryUi()
    {
        if (inventoryDisplay == null || playerCharacter == null)
            return;

        inventoryDisplay.Bind(playerCharacter.Inventory, itemModel);
    }

    public bool CanDropItem()
    {
        return playerCharacter != null && dungeon != null;
    }

    public bool CanThrowAt(Position2D tile)
    {
        return dungeon != null && !dungeon.IsWall(tile);
    }

    public ProjectileSystem Projectiles => projectileSystem;

    public GameObject CreateItemView(Item item, Position2D position)
    {
        if (itemSpawnSystem == null)
            return null;

        return itemSpawnSystem.CreateView(item, position);
    }

    public bool PlaceItem(Item item, Position2D position)
    {
        return itemSpawnSystem != null && itemSpawnSystem.PlaceExisting(item, position);
    }

    public void ReleaseLodgedItem(Item item, GameObject view, Position2D tile)
    {
        if (item == null || dungeon == null)
            return;

        if (view != null)
            view.transform.SetParent(itemHolder, true);
        else if (itemSpawnSystem != null)
            view = itemSpawnSystem.CreateView(item, tile);

        dungeon.AddGroundItem(item, tile, view);
    }

    public void ArmThrowFromBag(int index)
    {
        useArmed = false;
        throwArmed = true;
        throwFromBag = true;
        throwBagIndex = index;
        throwArmFrame = Time.frameCount;
    }

    public void ArmThrowFromSlot(EquipmentSlot slot)
    {
        useArmed = false;
        throwArmed = true;
        throwFromBag = false;
        throwSlot = slot;
        throwArmFrame = Time.frameCount;
    }

    public void ArmItemUse(int index)
    {
        throwArmed = false;
        useArmed = true;
        useBagIndex = index;
        useArmFrame = Time.frameCount;
    }

    public void UseArmedAt(Position2D tile)
    {
        if (!useArmed || Time.frameCount == useArmFrame || playerCharacter == null)
            return;
        if (useBagIndex < 0 || useBagIndex >= playerCharacter.Inventory.Bag.Count)
            return;

        Item item = playerCharacter.Inventory.Bag[useBagIndex];
        if (!ItemUse.InRange(playerCharacter, item, tile))
            return;

        useArmed = false;
        IAction action = item is RuneStone
            ? new UseRuneAction(playerCharacter, useBagIndex, tile)
            : new UseScrollAction(playerCharacter, useBagIndex, tile);
        BeginPlayerAction(action);
    }

    public void ThrowArmedAt(Position2D tile)
    {
        if (!throwArmed || Time.frameCount == throwArmFrame || playerCharacter == null || dungeon == null)
            return;

        if (projectileSystem == null)
            return;

        Position2D landing = projectileSystem.Impact(playerCharacter.Position, tile, ProjectileStop.BeforeObstacle);
        if (!CanThrowAt(landing))
            return;

        IAction action = throwFromBag
            ? new ThrowAction(playerCharacter, throwBagIndex, landing)
            : new ThrowAction(playerCharacter, throwSlot, landing);
        throwArmed = false;
        BeginPlayerAction(action);
    }

    public bool IsThrowArmed => throwArmed;
    public bool IsAiming => throwArmed || useArmed;
    public bool IsUseArmed => useArmed;

    public bool TryDropItem(Item item)
    {
        if (item == null || itemSpawnSystem == null || !CanDropItem())
            return false;

        return itemSpawnSystem.PlaceExisting(item, playerCharacter.Position);
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
        dungeon.RefreshItemVisibility(visible);
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
        IAction action = new WaitAction(actor);
        yield return action.PerformAction();
    }

    public void NotifyDestroyed(IDamagable destroyed)
    {
        if (destroyed is not IActor actor)
            return;

        if (destroyed is EnemyCharacter)
            TryDropEnemyLoot(actor.Position);

        if (!ReferenceEquals(actor, playerCharacter))
            dungeon.RemoveActor(actor);

        if (destroyed is Component component)
        {
            EnemyController enemy = component.GetComponent<EnemyController>();
            if (enemy != null)
                turnsController.RemoveEnemy(enemy);
        }
    }

    void TryDropEnemyLoot(Position2D tile)
    {
        if (Random.value >= Consts.ENEMY_DROP_CHANCE)
            return;

        Item item = ItemGenerator.GenerateRandom(baseItems);
        if (item == null)
            return;

        PlaceItem(item, tile);
    }
}
