using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public Transform playerHolder;
    public GameObject playerPrefab;

    public Camera playerCamera;
    public GameObject dungeonHolder;

    public DungeonBiomePrefabSet prefabSet;
    private DungeonFloorGenerator dungeonFloorGenerator;
    private Dungeon dungeon;
    private GameObject player;
    private TurnsController turnsController;
    void Start()
    {
        Instance = this;

        GenerateDungeon();
        SpawnPlayer();

        turnsController = new(10);
        turnsController.SetPlayer(player.GetComponent<Character>());

        // player
        dungeon.AddActor(player.GetComponent<PlayerCharacter>(), dungeon.GetSpawnPoint());
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        dungeonHolder.AddComponent<DungeonFloorGenerator>();
        dungeonFloorGenerator = dungeonHolder.GetComponent<DungeonFloorGenerator>();
        dungeonFloorGenerator.SetDungeonParameters(20, 20, 5, 4, 8, prefabSet);
        dungeon = dungeonFloorGenerator.GetGeneratedFloor();
        DestroyImmediate(dungeonHolder.GetComponent<DungeonFloorGenerator>());
    }

    public void LoadPrefabSet(DungeonBiomePrefabSet dungeonBiomePrefabSet)
    {
        prefabSet = dungeonBiomePrefabSet;
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

    // TODO - fix it after more logic is added
    public bool IsPlayerTurn()
    {
        return true;
    }

    // TODO -  cost of action should be related to character asking
    public IEnumerator RequestMoveTo(IWalkable actor, TileInfo tile)
    {
        if (!actor.HasEnergy)
            yield break;

        List<Position2D> path = AStar.FindPath(dungeon.GetFieldTypes(), dungeon.FindActorPosition(actor), tile.position, actor.MoveCostManager, MovementDirections.EIGHT);

        foreach (Position2D p in path)
        {
            IAction action = new MoveAction(actor, p, dungeon);
            yield return StartCoroutine(action.PerformAction());
            turnsController.CheckEnemiesTurn();
        }
    }
}
