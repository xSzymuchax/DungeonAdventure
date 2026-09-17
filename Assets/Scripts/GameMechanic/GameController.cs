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
    public Transform enemyHolder;

    private HashSet<Position2D> lastSeenFields = new();
    private HashSet<IActor> lastSeenActors = new();
    
    public MovementSystem movementSystem;
    public VisionSystem visionSystem;
    public FightingSystem fightingSystem; 

    void Start()
    {
        Instance = this;
        turnsController = new(10);

        GenerateDungeon();

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
    }

    public PlayerCharacter GetPlayerReference()
    {
        return playerCharacter;
    }

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
}
