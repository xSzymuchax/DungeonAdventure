using UnityEngine;

public class DungeonFloorBuilder
{
    public Dungeon Build(Transform parent, DungeonBiomePrefabSet prefabSet, FloorPlan plan)
    {
        Clear(parent);
        GameObject[,] tiles = PlaceTiles(parent, prefabSet, plan.Fields);
        return new Dungeon(plan.Fields, tiles, plan.Spawn, plan.RoomCount);
    }

    public Dungeon Build(Transform parent, DungeonBiomePrefabSet prefabSet, FloorFieldType[,] fields, Position2D spawn, int roomCount)
    {
        return Build(parent, prefabSet, new FloorPlan
        {
            Fields = fields,
            Spawn = spawn,
            RoomCount = roomCount
        });
    }

    static void Clear(Transform parent)
    {
        while (parent.childCount > 0)
            Object.DestroyImmediate(parent.GetChild(0).gameObject);
    }

    static GameObject[,] PlaceTiles(Transform parent, DungeonBiomePrefabSet prefabSet, FloorFieldType[,] fields)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        GameObject[,] result = new GameObject[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject tile = TileFor(prefabSet, fields[x, y]);
                if (tile == null)
                    continue;

                GameObject placed = Object.Instantiate(tile, parent);
                float size = Consts.TILE_SIZE;
                placed.transform.position = parent.position + new Vector3(x * size + 0.5f * size, 0, y * size + 0.5f * size);
                result[x, y] = placed;

                TileInfo info = placed.GetComponent<TileInfo>();
                info.position.x = x;
                info.position.y = y;
                info.type = fields[x, y];
                info.wasSeen = false;
                info.gameObject.SetActive(false);
                info.isOccupied = false;
            }
        }

        return result;
    }

    static GameObject TileFor(DungeonBiomePrefabSet prefabSet, FloorFieldType type)
    {
        switch (type)
        {
            case FloorFieldType.BASE_FIELD:
                return prefabSet.BaseFloorTile;
            case FloorFieldType.SIDE_FIELD:
                return prefabSet.SideTile;
            case FloorFieldType.POSSIBLE_DOOR_FIELD:
                return prefabSet.PossibleDoorTile;
            case FloorFieldType.CORRIDOR_FIELD:
                return prefabSet.CorridorTile;
            case FloorFieldType.DOOR_FIELD:
                return prefabSet.DoorTile;
            case FloorFieldType.SPAWN_FIELD:
                return prefabSet.SpawnTile;
            case FloorFieldType.EXIT_FIELD:
                return prefabSet.ExitTile;
            default:
                return null;
        }
    }
}
