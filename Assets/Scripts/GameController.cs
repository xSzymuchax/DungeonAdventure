using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public Transform playerHolder;
    public GameObject player;
    public Camera playerCamera;
    public GameObject dungeonHolder;

    public DungeonBiomePrefabSet prefabSet;
    private DungeonFloorGenerator dungeonFloorGenerator;
    private Dungeon dungeon;

    void Start()
    {
        Instance = this;

        GenerateDungeon();
        SpawnPlayer();
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

    // TODO
    public void LoadPrefabSet(DungeonBiomePrefabSet dungeonBiomePrefabSet)
    {

    }

    private void SpawnPlayer()
    {
        GameObject p = Instantiate(player, playerHolder);
        playerCamera.GetComponent<CameraController>().SetTarget(p.transform);
        p.GetComponent<PlayerController>().SetRaySource(playerCamera.transform);

        Position2D spawn = dungeon.GetSpawnPoint();
        p.transform.position = dungeon.GetFieldObjects()[spawn.x, spawn.y].GetComponent<Transform>().position;
    }

    // TODO - fix it after more logic is added
    public bool IsPlayerTurn()
    {
        return true;
    }

    // TODO - add some interface, make it possible for diffrent things to move
    public void RequestMoveTo()
    {

    }

    public void DEBUGMovePlayer(Position2D target)
    {

    }
}
