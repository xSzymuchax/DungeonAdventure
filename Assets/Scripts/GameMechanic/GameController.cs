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
    IDungeonGenerator dungeonGenerator = new ForestDungeonGenerator();
    readonly DungeonFloorBuilder floorBuilder = new DungeonFloorBuilder();
    public Dungeon dungeon;
    private GameObject player;
    private PlayerCharacter playerCharacter;
    private TurnsController turnsController;

    public EnemyCatalog enemyCatalog;
    public Transform enemyHolder;
    public GameObject itemModel;
    public ItemCatalog baseItems;
    public InventoryDisplay inventoryDisplay;
    public QuickBar quickBar;
    public UpgradeWorkbenchPanel upgradePanel;
    public UpgradeRecipe[] upgradeRecipes;
    public RunecraftWorkbenchPanel runePanel;
    public RuneRecipe[] runeRecipes;
    public SpellBlast spellBlast;
    public TokensDisplay tokensDisplay;
    LockPuzzle lockPuzzle;

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
    bool inspectArmed;
    int inspectArmFrame;
    bool throwFromBag;
    int throwBagIndex;
    EquipmentSlot throwSlot;
    int throwArmFrame;
    bool useArmed;
    bool useFromBag;
    int useBagIndex;
    EquipmentSlot useSlot;
    int useArmFrame;
    GameObject debugBench;
    GameObject debugRune;
    bool resolvingTurn;

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
        BindTokens();
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
        BindTokens();
        EnterFloor(dungeon, dungeon.GetSpawnPoint(), true, null, null);
    }

    public void SetDungeonGenerator(IDungeonGenerator generator)
    {
        if (generator != null)
            dungeonGenerator = generator;
    }

    public void GenerateDungeon()
    {
        dungeon = Build(dungeonGenerator);
    }

    public void GenerateDebugMap()
    {
        dungeon = Build(new DebugMapGenerator());
    }

    Dungeon Build(IDungeonGenerator generator)
    {
        FloorPlan plan = generator.Generate(new DungeonRequest
        {
            Width = 50,
            Height = 50,
            RoomCount = 10,
            MinRoomSize = 4,
            MaxRoomSize = 7
        });
        return floorBuilder.Build(dungeonHolder.transform, mapPrefabSet, plan);
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
        ClearDebugBench();
        saveSystem.RememberFloor(saveSystem.CurrentFloorIndex, dungeon, enemySpawnSystem.FloorEnemies(), true);
        enemySpawnSystem.Clear();
        itemSpawnSystem.Clear();
    }

    private void EnterFloor(Dungeon floor, Position2D arrival, bool freshContent, EnemySave[] savedEnemies, ItemSave[] savedItems)
    {
        ClearDebugBench();
        dungeon = floor;
        movementSystem.dungeonFloor = dungeon;
        enemySpawnSystem.Bind(dungeon);
        itemSpawnSystem.Bind(dungeon);
        projectileSystem.Bind(dungeon);
        PlacePlayer(arrival);
        if (freshContent && debugMap)
        {
            Position2D dummy = new Position2D { x = 5, y = 5 };
            Position2D benchTile = new Position2D { x = 6, y = 5 };
            Position2D runeTile = new Position2D { x = 7, y = 5 };
            itemSpawnSystem.SpawnAll(dummy, benchTile, runeTile);
            enemySpawnSystem.SpawnDummy(EnemySpawnSystem.DebugEnemyId, dummy);
            SpawnDebugBench(benchTile);
            SpawnDebugRuneBench(runeTile);
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
        PlacePlanItems();
        CheckPlayerPerception();
    }

    public void ShowLockPuzzle(Door door)
    {
        if (lockPuzzle == null)
        {
            lockPuzzle = gameObject.AddComponent<LockPuzzle>();
            Canvas canvas = inventoryDisplay != null ? inventoryDisplay.GetComponentInParent<Canvas>() : null;
            lockPuzzle.Ensure(canvas != null ? canvas.transform : transform);
        }

        lockPuzzle.Show(door);
    }

    void PlacePlanItems()
    {
        if (dungeon == null || itemSpawnSystem == null)
            return;

        for (int i = 0; i < dungeon.SteelKeys.Count; i++)
            itemSpawnSystem.PlaceExisting(DoorItems.SteelKey(), dungeon.SteelKeys[i]);
    }

    public void BeginPlayerAction(IAction action)
    {
        StartCoroutine(RunPlayerAction(action));
    }

    public void CloseBenchesExcept(CraftBench current)
    {
        if (upgradePanel != null && upgradePanel.Bench != current)
            upgradePanel.Close();
        if (runePanel != null && runePanel.Bench != current)
            runePanel.Close();
    }

    public void BeginWorkbenchConfirm(PlayerCharacter player, CraftBench bench)
    {
        StartCoroutine(ConfirmWorkbench(player, bench));
    }

    IEnumerator ConfirmWorkbench(PlayerCharacter player, CraftBench bench)
    {
        if (player == null || bench == null)
            yield break;

        if (player.IsFrozen())
        {
            yield return RunPlayerAction(new WaitAction(player));
            yield break;
        }

        float before = player.Energy;
        ConfirmUpgradeAction action = new ConfirmUpgradeAction(player, bench);
        yield return action.PerformAction();
        if (player.IsDead || player.Energy >= before || resolvingTurn)
            yield break;

        yield return EvaluateTurn();
    }

    public IEnumerator PlayAction(IAction action)
    {
        yield return RunPlayerAction(action);
    }

    public CraftBench WorkbenchAt(Position2D tile)
    {
        if (dungeon == null || !dungeon.TileExists(tile.x, tile.y))
            return null;

        TileInfo info = dungeon.GetTileInfos()[tile.x, tile.y];
        if (info == null)
            return null;
        return info.tileObject as CraftBench;
    }

    void SpawnDebugBench(Position2D tile)
    {
        ClearBench(ref debugBench);
        GameObject body = CreateBenchBody(tile, "UpgradeWorkbench", Color.white);
        if (body == null || upgradePanel == null)
        {
            if (body != null)
                Destroy(body);
            return;
        }

        TileInfo info = dungeon.GetTileInfos()[tile.x, tile.y];
        Workbench bench = body.AddComponent<Workbench>();
        bench.Setup(info, upgradePanel, upgradeRecipes);
        info.isOccupied = true;
        debugBench = body;
    }

    void SpawnDebugRuneBench(Position2D tile)
    {
        ClearBench(ref debugRune);
        GameObject body = CreateBenchBody(tile, "RunecraftingWorkbench", new Color(0.55f, 0.7f, 0.85f));
        if (body == null || runePanel == null)
        {
            if (body != null)
                Destroy(body);
            return;
        }

        TileInfo info = dungeon.GetTileInfos()[tile.x, tile.y];
        RunecraftWorkbench bench = body.AddComponent<RunecraftWorkbench>();
        bench.Setup(info, runePanel, runeRecipes, spellBlast);
        info.isOccupied = true;
        debugRune = body;
    }

    GameObject CreateBenchBody(Position2D tile, string name, Color color)
    {
        if (dungeon == null || !dungeon.TileExists(tile.x, tile.y))
            return null;

        TileInfo info = dungeon.GetTileInfos()[tile.x, tile.y];
        if (info == null)
            return null;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = name;
        float size = Consts.TILE_SIZE * 0.6f;
        body.transform.localScale = new Vector3(size, size, size);
        body.transform.localPosition = Vector3.up * (size * 0.5f);
        Renderer renderer = body.GetComponent<Renderer>();
        MeshRenderer tileRenderer = info.GetComponent<MeshRenderer>();
        if (renderer != null && tileRenderer != null && tileRenderer.sharedMaterial != null)
        {
            Material mat = new Material(tileRenderer.sharedMaterial);
            Color tint = color;
            tint.a = 1f;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", tint);
            renderer.material = mat;
        }
        else if (renderer != null)
            renderer.material.color = color;
        return body;
    }

    void ClearDebugBench()
    {
        ClearBench(ref debugBench);
        ClearBench(ref debugRune);
        if (upgradePanel != null)
            upgradePanel.Close();
        if (runePanel != null)
            runePanel.Close();
    }

    void ClearBench(ref GameObject body)
    {
        if (body == null)
            return;

        CraftBench bench = body.GetComponent<CraftBench>();
        if (bench != null && playerCharacter != null)
            bench.ReturnAll(playerCharacter.Inventory);
        if (bench != null && bench.Tile != null)
            bench.Tile.isOccupied = false;
        if (bench != null)
            bench.Lift();
        Destroy(body);
        body = null;
    }

    IEnumerator RunPlayerAction(IAction action)
    {
        if (playerCharacter == null || playerCharacter.Energy <= 0 || action == null)
            yield break;

        bool frozen = playerCharacter.IsFrozen();
        if (playerCharacter.CurrentPath != null)
            playerCharacter.CurrentPath.Clear();

        yield return fightingSystem.TickTokens(playerCharacter);
        if (playerCharacter.IsDead || playerCharacter.Energy <= 0)
            yield break;

        float energy = playerCharacter.Energy;
        if (frozen)
            action = new WaitAction(playerCharacter);
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
        if (quickBar != null)
            quickBar.Bind(playerCharacter.Inventory, inventoryDisplay, itemModel);
    }

    void BindTokens()
    {
        if (tokensDisplay == null || playerCharacter == null)
            return;

        tokensDisplay.Bind(playerCharacter);
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
        inspectArmed = false;
        throwArmed = true;
        throwFromBag = true;
        throwBagIndex = index;
        throwArmFrame = Time.frameCount;
    }

    public void ArmThrowFromSlot(EquipmentSlot slot)
    {
        useArmed = false;
        inspectArmed = false;
        throwArmed = true;
        throwFromBag = false;
        throwSlot = slot;
        throwArmFrame = Time.frameCount;
    }

    public void ArmItemUse(int index)
    {
        throwArmed = false;
        inspectArmed = false;
        useArmed = true;
        useFromBag = true;
        useBagIndex = index;
        useArmFrame = Time.frameCount;
    }

    public void ArmItemUse(EquipmentSlot slot)
    {
        throwArmed = false;
        inspectArmed = false;
        useArmed = true;
        useFromBag = false;
        useSlot = slot;
        useArmFrame = Time.frameCount;
    }

    public void UseArmedAt(Position2D tile)
    {
        if (!useArmed || Time.frameCount == useArmFrame || playerCharacter == null)
            return;

        Item item = ArmedItem();
        if (item == null || !ItemUse.InRange(playerCharacter, item, tile))
            return;

        useArmed = false;
        BeginPlayerAction(UseAction(item, tile));
    }

    Item ArmedItem()
    {
        if (useFromBag)
        {
            if (useBagIndex < 0 || useBagIndex >= playerCharacter.Inventory.Bag.Count)
                return null;
            return playerCharacter.Inventory.Bag[useBagIndex];
        }

        return playerCharacter.Inventory.Equipped(useSlot);
    }

    IAction UseAction(Item item, Position2D tile)
    {
        if (DoorItems.IsTool(item))
            return new UseDoorToolAction(playerCharacter, useBagIndex, tile);
        if (item is RuneStone)
            return new UseRuneAction(playerCharacter, useBagIndex, tile);
        if (item is StaffItem)
            return new UseStaffAction(playerCharacter, useSlot, tile);
        return new UseScrollAction(playerCharacter, useBagIndex, tile);
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

    public bool WorkbenchOpen =>
        (upgradePanel != null && upgradePanel.IsOpen) ||
        (runePanel != null && runePanel.IsOpen) ||
        (lockPuzzle != null && lockPuzzle.IsOpen);
    public bool IsThrowArmed => throwArmed;
    public bool IsInspectArmed => inspectArmed;
    public bool IsAiming => throwArmed || useArmed || inspectArmed;
    public bool IsUseArmed => useArmed;

    public void ArmInspect()
    {
        if (inspectArmed)
        {
            inspectArmed = false;
            return;
        }

        throwArmed = false;
        useArmed = false;
        inspectArmed = true;
        inspectArmFrame = Time.frameCount;
    }

    public void Inspect(RaycastHit[] hits)
    {
        if (!inspectArmed || Time.frameCount == inspectArmFrame)
            return;

        inspectArmed = false;
        if (tokensDisplay == null || tokensDisplay.Window == null || hits == null || hits.Length == 0)
            return;

        TileInfo tile = TileOf(Closest(hits));
        if (tile == null)
            return;

        InformationWindow window = tokensDisplay.Window;
        if (dungeon != null && dungeon.TryGetActorAt(tile.position, out IActor actor) && actor is EnemyCharacter enemy && !enemy.IsDead)
        {
            window.ShowSubject(enemy.Title, enemy.Description(), enemy.Picture(), enemy.ActiveTokens);
            return;
        }

        if (tile.tileObject is CraftBench bench)
        {
            window.ShowSubject(bench.Title, bench.Description(), bench.Picture(), null);
            return;
        }

        groundScratch.Clear();
        if (dungeon != null)
            dungeon.CopyGroundItems(tile.position, groundScratch);
        window.ShowSubject(tile.Title(), tile.Description(groundScratch), tile.Picture(), null);
    }

    readonly List<Item> groundScratch = new();

    static RaycastHit Closest(RaycastHit[] hits)
    {
        int best = 0;
        for (int i = 1; i < hits.Length; i++)
        {
            if (hits[i].distance < hits[best].distance)
                best = i;
        }

        return hits[best];
    }

    TileInfo TileOf(RaycastHit hit)
    {
        if (hit.collider == null)
            return null;

        TileInfo tile = hit.collider.GetComponentInParent<TileInfo>();
        if (tile != null)
            return tile;

        Character character = hit.collider.GetComponentInParent<Character>();
        if (character != null)
            return TileAt(character.Position);

        CraftBench bench = hit.collider.GetComponentInParent<CraftBench>();
        if (bench != null)
            return TileAt(bench.Position);

        return null;
    }

    TileInfo TileAt(Position2D position)
    {
        if (dungeon == null || !dungeon.TileExists(position.x, position.y))
            return null;

        return dungeon.GetTileInfos()[position.x, position.y];
    }

    public bool TryDropItem(Item item)
    {
        if (item == null || itemSpawnSystem == null || !CanDropItem())
            return false;

        return itemSpawnSystem.PlaceExisting(item, playerCharacter.Position);
    }

    private Dungeon RebuildSavedFloor(int index)
    {
        FloorFieldType[,] fields = saveSystem.GetFields(index);
        Dungeon built = floorBuilder.Build(dungeonHolder.transform, mapPrefabSet, fields, saveSystem.GetSpawn(index), saveSystem.GetRoomCount(index));
        built.RestoreSeen(saveSystem.GetSeen(index));
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
        if (resolvingTurn)
            yield break;

        resolvingTurn = true;
        yield return turnsController.EvaluateTurn();
        yield return movementSystem.PlayPendingAnimations();
        if (turnsController.TurnElapsed && playerCharacter != null && !playerCharacter.IsDead && enemySpawnSystem.TrySpawnAmbient())
            CheckPlayerPerception();
        resolvingTurn = false;
        if (playerCharacter != null && !playerCharacter.IsDead && playerCharacter.Energy <= 0)
            yield return EvaluateTurn();
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
