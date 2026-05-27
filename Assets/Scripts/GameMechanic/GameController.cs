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
    private Dungeon dungeon;
    private GameObject player;
    private PlayerCharacter playerCharacter;
    private TurnsController turnsController;

    public GameObject enemyPrefab;
    public Transform enemyHolder;

    private HashSet<Position2D> lastSeenFields = new();
    private HashSet<IActor> lastSeenActors = new();

    void Start()
    {
        Instance = this;
        turnsController = new(10);

        GenerateDungeon();
        SpawnPlayer();
        SpawnEnemy();

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
    }

    // TODO - 3 same functions - refactor
    public bool CanSeeActor(IActor source, IActor target)
    {
        if (source is not IHasPerception perceptionActor)
            return false;

        Position2D from = dungeon.FindActorPosition(source);
        Position2D to = dungeon.FindActorPosition(target);

        return VisionCalculator.CanSee(from, to, perceptionActor.ViewRange, dungeon);
    }

    public bool CanAttackActor(IActor attacking, IActor target)
    {
        if (attacking is not IHasPerception perceptionActor)
            return false;

        Position2D from = dungeon.FindActorPosition(attacking);
        Position2D to = dungeon.FindActorPosition(target);

        return VisionCalculator.CanSee(from, to, perceptionActor.AttackRange, dungeon);
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

        visible = VisionCalculator.AllVisibleFields(
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
        var currentSeen = dungeon.CheckPlayerSeeActors(playerCharacter, playerCharacter.ViewRange);

        foreach (var s in currentSeen)
            if (!lastSeenActors.Contains(s))
            {
                lastSeenActors = currentSeen;
                return true;
            }

        lastSeenActors = currentSeen;
        return false;
    }

    public bool CanDetectActor(IActor detecting, IActor target)
    {
        if (detecting is not IHasPerception perceptionActor)
            return false;

        Position2D from = dungeon.FindActorPosition(detecting);
        Position2D to = dungeon.FindActorPosition(target);

        return VisionCalculator.CanSee(from, to, perceptionActor.DetectionRange, dungeon);
    }

    public bool CanWakeUpActor(IActor waking, IActor target)
    {
        if (waking is not IHasPerception perceptionActor)
            return false;

        Position2D from = dungeon.FindActorPosition(waking);
        Position2D to = dungeon.FindActorPosition(target);

        return VisionCalculator.CanSee(from, to, perceptionActor.WakeUpRange, dungeon);
    }

    public IEnumerator RequestMoveTo(IWalkable actor, Position2D tile)
    {
        if (!actor.HasEnergy)
            yield break;

        IAction action = new MoveAction(actor, tile, dungeon);
        yield return StartCoroutine(action.PerformAction());
    }

    public List<Position2D> RequestCalculatePath(IWalkable actor, Position2D tile)
    {
        List<Position2D> result = new();
        result = AStar.FindClosestPath(dungeon.GetTileInfos(), dungeon.FindActorPosition(actor), tile, actor.MoveCostManager, MovementDirections.EIGHT);
        result.RemoveAt(0);
        return result;
    }

    public IEnumerator EvaluateTurn()
    {
        yield return turnsController.EvaluateTurn();
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
