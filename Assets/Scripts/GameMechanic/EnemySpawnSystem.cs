using UnityEngine;

public class EnemySpawnSystem
{
    public const float AmbientChance = 0.01f;
    public const string AmbientEnemyId = "base_enemy";

    private Dungeon dungeon;
    private TurnsController turns;
    private PlayerCharacter player;
    private readonly EnemyCatalog catalog;
    private readonly Transform enemyHolder;
    private readonly SaveSystem saveSystem;

    public EnemySpawnSystem(EnemyCatalog catalog, Transform enemyHolder, TurnsController turns, SaveSystem saveSystem)
    {
        this.catalog = catalog;
        this.enemyHolder = enemyHolder;
        this.turns = turns;
        this.saveSystem = saveSystem;
    }

    public void Bind(Dungeon dungeon)
    {
        this.dungeon = dungeon;
    }

    public void BindTurns(TurnsController turns)
    {
        this.turns = turns;
    }

    public void SetPlayer(PlayerCharacter player)
    {
        this.player = player;
    }

    public void SpawnInitial()
    {
        if (dungeon == null)
            return;

        int count = dungeon.RoomCount / 2;
        for (int i = 0; i < count; i++)
        {
            if (!TrySpawnWithinCap())
                break;
        }
    }

    public bool TrySpawnAmbient()
    {
        if (dungeon == null || Random.value >= AmbientChance)
            return false;

        return TrySpawnWithinCap();
    }

    public bool SpawnAt(string enemyId, Position2D position)
    {
        if (dungeon == null)
            return false;

        GameObject enemyGo = InstantiateEnemy(enemyId, out EnemyCharacter character);
        if (enemyGo == null)
            return false;

        PlaceLiving(character, enemyGo, position);
        return true;
    }

    public void SpawnSaved(EnemySave[] enemies)
    {
        if (dungeon == null || enemies == null)
            return;

        for (int i = 0; i < enemies.Length; i++)
            SpawnSaved(enemies[i]);
    }

    private void SpawnSaved(EnemySave save)
    {
        if (save == null || !TileExists(save.x, save.y))
            return;

        GameObject enemyGo = InstantiateEnemy(save.enemyId, out EnemyCharacter character);
        if (enemyGo == null)
            return;

        saveSystem.ApplyEnemy(character, save);
        Position2D position = new() { x = save.x, y = save.y };
        if (character.IsDead)
        {
            character.Position = position;
            enemyGo.transform.position = dungeon.GetTileWorldPosition(position);
            character.HideDestroyed();
            return;
        }

        PlaceLiving(character, enemyGo, position);
    }

    private GameObject InstantiateEnemy(string enemyId, out EnemyCharacter character)
    {
        character = null;
        if (catalog == null || !catalog.TryGet(enemyId, out GameObject prefab))
        {
            Debug.Log("cant spawn enemy " + enemyId);
            return null;
        }

        GameObject enemyGo = Object.Instantiate(prefab, enemyHolder);
        character = enemyGo.GetComponent<EnemyCharacter>();
        return enemyGo;
    }

    private void PlaceLiving(EnemyCharacter character, GameObject enemyGo, Position2D position)
    {
        character.Position = position;
        enemyGo.transform.position = dungeon.GetTileWorldPosition(position);
        dungeon.AddActor(character, position, enemyGo);

        TileInfo tile = dungeon.GetTileInfos()[position.x, position.y];
        if (tile != null)
            tile.isOccupied = true;

        EnemyController controller = enemyGo.GetComponent<EnemyController>();
        controller.SetChasedCharacter(player);
        turns.AddEnemy(controller);
    }

    private bool TileExists(int x, int y)
    {
        FloorFieldType[,] fields = dungeon.GetFieldTypes();
        return x >= 0 && y >= 0 && x < fields.GetLength(0) && y < fields.GetLength(1) && fields[x, y] != FloorFieldType.EMPTY;
    }

    private bool TrySpawnWithinCap()
    {
        int cap = dungeon.RoomCount / 2;
        if (dungeon.CountLivingEnemies() >= cap)
            return false;
        if (!dungeon.TryGetRandomFreeBaseField(out Position2D tile))
            return false;

        return SpawnAt(AmbientEnemyId, tile);
    }
}
