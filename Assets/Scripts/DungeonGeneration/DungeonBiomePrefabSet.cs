using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CreateBiomePrefabSet", menuName = "Dungeon/Biome prefab set")]
public class DungeonBiomePrefabSet : ScriptableObject
{
    public GameObject BaseFloorTile;
    public GameObject PossibleDoorTile;
    public GameObject SideTile;
    public GameObject CorridorTile;
    public GameObject DoorTile;
    public GameObject SpawnTile;
    public GameObject ExitTile;
}
