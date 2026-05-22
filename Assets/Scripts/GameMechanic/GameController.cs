//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private TurnsController turnsController;

    public GameObject enemyPrefab;
    public Transform enemyHolder;

    void Start()
    {
        Instance = this;
        turnsController = new(10);

        GenerateDungeon();
        SpawnPlayer();
        SpawnEnemy();

        turnsController.SetPlayer(player.GetComponent<Character>());

        // player
        dungeon.AddActor(player.GetComponent<PlayerCharacter>(), dungeon.GetSpawnPoint());
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
                dungeon.AddActor(enemyGo.GetComponent<EnemyCharacter>(), new() { x=x,y=y});
                turnsController.AddEnemy(enemyGo.GetComponent<EnemyController>());
                enemyGo.transform.position = dungeon.GetFieldObjects()[x, y].GetComponent<Transform>().position;
                enemyGo.GetComponent<EnemyController>().SetChasedCharacter(player.GetComponent<IActor>());
                break;
            }
        }
    }

    [ContextMenu("Generate Dungeon")]
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
    }

    // TODO -  cost of action should be related to character asking
    public IEnumerator RequestMoveTo(IWalkable actor, Position2D tile)
    {
        if (!actor.HasEnergy)
            yield break;

        List<Position2D> path = AStar.FindPath(dungeon.GetFieldTypes(), dungeon.FindActorPosition(actor), tile, actor.MoveCostManager, MovementDirections.EIGHT);

        foreach (Position2D p in path.Skip(1))
        {
            IAction action = new MoveAction(actor, p, dungeon);
            yield return StartCoroutine(action.PerformAction());
            //yield return turnsController.EvaluateTurn();
            if (!actor.HasEnergy)
                yield break;
        }
    }

    public IEnumerator EvaluateTurn()
    {
        yield return turnsController.EvaluateTurn();
    }

    public Position2D GetPositionOfActor(IActor actor)
    {
        return dungeon.FindActorPosition(actor);
    }
}
