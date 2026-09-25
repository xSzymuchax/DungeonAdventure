using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class TokenSave
{
    public string tokenType;
    public int strength;
    public int duration;
}

[Serializable]
public class PlayerSave
{
    public int health;
    public float mana;
    public float energy;
    public int maxHealth;
    public float maxMana;
    public float walkCost;
    public float attackCost;
    public int damage;
    public int viewRange;
    public int strength;
    public int knowledge;
    public int satiety;
    public int hydration;
    public int sanity;
    public int maxSatiety;
    public int maxHydration;
    public int maxSanity;
    public int x;
    public int y;
    public TokenSave[] tokens;
}

[Serializable]
public class EnemySave
{
    public string enemyId;
    public int health;
    public float mana;
    public float energy;
    public int maxHealth;
    public float maxMana;
    public float walkCost;
    public float attackCost;
    public int damage;
    public int viewRange;
    public int attackRange;
    public int wakeUpRange;
    public int detectionRange;
    public int x;
    public int y;
    public TokenSave[] tokens;
}

[Serializable]
public class FloorLayout
{
    public int width;
    public int height;
    public int spawnX;
    public int spawnY;
    public int roomCount;
    public int[] fields;
    public bool[] seen;
    public EnemySave[] enemies;
}

[Serializable]
public class CurrentGame
{
    public int currentFloorIndex;
    public List<FloorLayout> floors = new();
    public PlayerSave player;
}

[Serializable]
public class GameSave
{
    public CurrentGame currentGame;
}

public class SaveSystem
{
    private const string FileName = "save.json";

    public CurrentGame currentGame = new();

    public int CurrentFloorIndex => currentGame.currentFloorIndex;
    public int FloorCount => currentGame.floors.Count;
    public PlayerSave Player => currentGame.player;

    public void RememberPlayer(PlayerCharacter player)
    {
        currentGame.player = CapturePlayer(player);
    }

    public void ApplyPlayer(PlayerCharacter player)
    {
        if (player == null || currentGame.player == null)
            return;

        Apply(player, currentGame.player);
    }

    public void ApplyEnemy(EnemyCharacter character, EnemySave save)
    {
        if (character == null || save == null)
            return;

        EnemyStats stats = character.EnemyStats;
        stats.ApplySaved(save.maxHealth, save.maxMana, save.walkCost, save.attackCost, save.damage, save.viewRange);
        stats.ApplySavedRanges(save.attackRange, save.wakeUpRange, save.detectionRange);
        character.RestoreVitals(save.health, save.mana, save.energy);
        TokenSaveUtility.Restore(character, save.tokens);
    }

    public void RememberFloor(int index, Dungeon dungeon, IEnumerable<EnemyCharacter> enemies, bool replaceEnemies)
    {
        EnemySave[] savedEnemies = replaceEnemies ? CaptureEnemies(enemies) : null;
        StoreFloor(index, dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), CaptureSeen(dungeon), dungeon.RoomCount, savedEnemies);
    }

    public void Save()
    {
        GameSave save = new() { currentGame = currentGame };
        File.WriteAllText(SavePath(), JsonUtility.ToJson(save, true));
    }

    public bool Load()
    {
        string path = SavePath();
        if (!File.Exists(path))
            return false;

        GameSave save = JsonUtility.FromJson<GameSave>(File.ReadAllText(path));
        if (save == null || save.currentGame == null || save.currentGame.floors == null || save.currentGame.floors.Count == 0)
            return false;

        currentGame = save.currentGame;
        if (currentGame.currentFloorIndex < 0 || currentGame.currentFloorIndex >= currentGame.floors.Count)
            currentGame.currentFloorIndex = 0;
        return true;
    }

    public void ResetToSingleFloor(FloorFieldType[,] fields, Position2D spawn, bool[,] seen, int roomCount)
    {
        currentGame = new CurrentGame();
        StoreFloor(0, fields, spawn, seen, roomCount, System.Array.Empty<EnemySave>());
    }

    public void StoreFloor(int index, FloorFieldType[,] fields, Position2D spawn, bool[,] seen, int roomCount, EnemySave[] enemies)
    {
        EnemySave[] keptEnemies = enemies;
        if (keptEnemies == null && index >= 0 && index < currentGame.floors.Count)
            keptEnemies = currentGame.floors[index].enemies;

        FloorLayout layout = ToLayout(fields, spawn, seen, roomCount);
        layout.enemies = keptEnemies ?? System.Array.Empty<EnemySave>();
        if (index == currentGame.floors.Count)
            currentGame.floors.Add(layout);
        else
            currentGame.floors[index] = layout;

        currentGame.currentFloorIndex = index;
    }

    public FloorFieldType[,] GetFields(int index)
    {
        FloorLayout layout = currentGame.floors[index];
        FloorFieldType[,] fields = new FloorFieldType[layout.width, layout.height];
        for (int y = 0; y < layout.height; y++)
        {
            for (int x = 0; x < layout.width; x++)
                fields[x, y] = (FloorFieldType)layout.fields[y * layout.width + x];
        }

        return fields;
    }

    public Position2D GetSpawn(int index)
    {
        FloorLayout layout = currentGame.floors[index];
        return new Position2D { x = layout.spawnX, y = layout.spawnY };
    }

    public bool[,] GetSeen(int index)
    {
        FloorLayout layout = currentGame.floors[index];
        bool[,] seen = new bool[layout.width, layout.height];
        if (layout.seen == null || layout.seen.Length != layout.width * layout.height)
            return seen;

        for (int y = 0; y < layout.height; y++)
        {
            for (int x = 0; x < layout.width; x++)
                seen[x, y] = layout.seen[y * layout.width + x];
        }

        return seen;
    }

    public int GetRoomCount(int index)
    {
        if (index < 0 || index >= currentGame.floors.Count)
            return 0;

        return currentGame.floors[index].roomCount;
    }

    public EnemySave[] GetEnemies(int index)
    {
        if (index < 0 || index >= currentGame.floors.Count)
            return System.Array.Empty<EnemySave>();

        EnemySave[] enemies = currentGame.floors[index].enemies;
        return enemies ?? System.Array.Empty<EnemySave>();
    }

    private static FloorLayout ToLayout(FloorFieldType[,] fields, Position2D spawn, bool[,] seen, int roomCount)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        int[] flat = new int[width * height];
        bool[] seenFlat = new bool[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flat[y * width + x] = (int)fields[x, y];
                seenFlat[y * width + x] = seen != null && x < seen.GetLength(0) && y < seen.GetLength(1) && seen[x, y];
            }
        }

        return new FloorLayout
        {
            width = width,
            height = height,
            spawnX = spawn.x,
            spawnY = spawn.y,
            roomCount = roomCount,
            fields = flat,
            seen = seenFlat
        };
    }

    private static string SavePath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    private static PlayerSave CapturePlayer(PlayerCharacter player)
    {
        PlayerStats stats = player.PlayerStats;
        return new PlayerSave
        {
            health = player.Health,
            mana = (float)player.Mana,
            energy = (float)player.Energy,
            maxHealth = stats.MaxHealth,
            maxMana = (float)stats.MaxMana,
            walkCost = (float)stats.WalkCost,
            attackCost = (float)stats.AttackCost,
            damage = stats.Damage,
            viewRange = stats.ViewRange,
            strength = stats.Strength,
            knowledge = stats.Knowledge,
            satiety = player.Satiety,
            hydration = player.Hydration,
            sanity = player.Sanity,
            maxSatiety = stats.MaxSatiety,
            maxHydration = stats.MaxHydration,
            maxSanity = stats.MaxSanity,
            x = player.Position.x,
            y = player.Position.y,
            tokens = TokenSaveUtility.Capture(player)
        };
    }

    private static void Apply(PlayerCharacter player, PlayerSave save)
    {
        PlayerStats stats = player.PlayerStats;
        stats.ApplySaved(save.maxHealth, save.maxMana, save.walkCost, save.attackCost, save.damage, save.viewRange);
        stats.ApplySavedAttributes(save.strength, save.knowledge, save.maxSatiety, save.maxHydration, save.maxSanity);
        player.RestoreVitals(save.health, save.mana, save.energy);
        player.RestoreNeeds(save.satiety, save.hydration, save.sanity);
        TokenSaveUtility.Restore(player, save.tokens);
    }

    private static EnemySave[] CaptureEnemies(IEnumerable<EnemyCharacter> enemies)
    {
        List<EnemySave> saved = new();
        if (enemies == null)
            return saved.ToArray();

        foreach (EnemyCharacter character in enemies)
        {
            if (character == null)
                continue;

            EnemyStats stats = character.EnemyStats;
            if (stats == null)
                continue;

            saved.Add(new EnemySave
            {
                enemyId = character.EnemyId,
                health = character.Health,
                mana = (float)character.Mana,
                energy = (float)character.Energy,
                maxHealth = stats.MaxHealth,
                maxMana = (float)stats.MaxMana,
                walkCost = (float)stats.WalkCost,
                attackCost = (float)stats.AttackCost,
                damage = stats.Damage,
                viewRange = stats.ViewRange,
                attackRange = stats.AttackRange,
                wakeUpRange = stats.WakeUpRange,
                detectionRange = stats.DetectionRange,
                x = character.Position.x,
                y = character.Position.y,
                tokens = TokenSaveUtility.Capture(character)
            });
        }

        return saved.ToArray();
    }

    private static bool[,] CaptureSeen(Dungeon dungeon)
    {
        TileInfo[,] tiles = dungeon.GetTileInfos();
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);
        bool[,] seen = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                seen[x, y] = tiles[x, y] != null && tiles[x, y].wasSeen;
        }

        return seen;
    }
}

public static class TokenSaveUtility
{
    public static TokenSave[] Capture(Character character)
    {
        List<TokenSave> saved = new();
        foreach (IToken token in character.ActiveTokens)
        {
            if (token == null || token.IsExpired)
                continue;

            saved.Add(new TokenSave
            {
                tokenType = token.TokenType,
                strength = token.Strength,
                duration = token.Duration
            });
        }

        return saved.ToArray();
    }

    public static void Restore(Character character, TokenSave[] saved)
    {
        List<IToken> tokens = new();
        if (saved != null)
        {
            for (int i = 0; i < saved.Length; i++)
            {
                TokenSave token = saved[i];
                if (token == null)
                    continue;

                IToken restored = TokenFactory.Create(token.tokenType, token.strength, token.duration);
                if (restored != null)
                    tokens.Add(restored);
            }
        }

        character.ReplaceTokens(tokens);
    }
}
