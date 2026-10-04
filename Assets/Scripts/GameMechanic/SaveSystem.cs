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
    public float health;
    public float mana;
    public float energy;
    public float maxHealth;
    public float maxMana;
    public float walkCost;
    public float attackCost;
    public int damage;
    public int viewRange;
    public int strength;
    public int knowledge;
    public float satiety;
    public float hydration;
    public float sanity;
    public float maxSatiety;
    public float maxHydration;
    public float maxSanity;
    public int x;
    public int y;
    public TokenSave[] tokens;
    public ItemSave[] inventory;
    public ItemSave[] equipped;
}

[Serializable]
public class EnemySave
{
    public string enemyId;
    public float health;
    public float mana;
    public float energy;
    public float maxHealth;
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
    public ItemSave[] items;
}

[Serializable]
public class ItemSave
{
    public string id;
    public string displayName;
    public int kind;
    public int slot;
    public int x;
    public int y;
    public StatModifier[] modifiers;
    public StatRequirement[] requirements;
    public bool thrownWeapon;
    public int throwDamage;
    public bool sharp;
    public int count;
    public float charge;
    public int upgradeLevel;
    public int durability;
    public int maxDurability;
    public int age;
    public int spellLevel;
    public int runeLevel;
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
        StoreFloor(index, dungeon.GetFieldTypes(), dungeon.GetSpawnPoint(), CaptureSeen(dungeon), dungeon.RoomCount, savedEnemies, CaptureGroundItems(dungeon));
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
        StoreFloor(0, fields, spawn, seen, roomCount, System.Array.Empty<EnemySave>(), System.Array.Empty<ItemSave>());
    }

    public void StoreFloor(int index, FloorFieldType[,] fields, Position2D spawn, bool[,] seen, int roomCount, EnemySave[] enemies, ItemSave[] items)
    {
        EnemySave[] keptEnemies = enemies;
        if (keptEnemies == null && index >= 0 && index < currentGame.floors.Count)
            keptEnemies = currentGame.floors[index].enemies;

        ItemSave[] keptItems = items;
        if (keptItems == null && index >= 0 && index < currentGame.floors.Count)
            keptItems = currentGame.floors[index].items;

        FloorLayout layout = ToLayout(fields, spawn, seen, roomCount);
        layout.enemies = keptEnemies ?? System.Array.Empty<EnemySave>();
        layout.items = keptItems ?? System.Array.Empty<ItemSave>();
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

    public ItemSave[] GetItems(int index)
    {
        if (index < 0 || index >= currentGame.floors.Count)
            return System.Array.Empty<ItemSave>();

        ItemSave[] items = currentGame.floors[index].items;
        return items ?? System.Array.Empty<ItemSave>();
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
            mana = player.Mana,
            energy = player.Energy,
            maxHealth = stats.MaxHealth,
            maxMana = stats.MaxMana,
            walkCost = stats.WalkCost,
            attackCost = stats.AttackCost,
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
            tokens = TokenSaveUtility.Capture(player),
            inventory = CaptureHeld(player.Inventory.Bag),
            equipped = CaptureEquipped(player.Inventory)
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
        player.Inventory.Replace(save.inventory, save.equipped);
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
                mana = character.Mana,
                energy = character.Energy,
                maxHealth = stats.MaxHealth,
                maxMana = stats.MaxMana,
                walkCost = stats.WalkCost,
                attackCost = stats.AttackCost,
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

    private static ItemSave[] CaptureGroundItems(Dungeon dungeon)
    {
        List<(Item item, Position2D position)> ground = dungeon.GroundItems();
        ItemSave[] saved = new ItemSave[ground.Count];
        for (int i = 0; i < ground.Count; i++)
            saved[i] = ToSave(ground[i].item, ground[i].position.x, ground[i].position.y);
        return saved;
    }

    private static ItemSave[] CaptureHeld(IReadOnlyList<Item> items)
    {
        ItemSave[] saved = new ItemSave[items.Count];
        for (int i = 0; i < items.Count; i++)
            saved[i] = ToSave(items[i], 0, 0);
        return saved;
    }

    private static ItemSave[] CaptureEquipped(PlayerInventory inventory)
    {
        List<ItemSave> saved = new();
        EquipmentSlot[] slots = { EquipmentSlot.Helmet, EquipmentSlot.Armor, EquipmentSlot.Amulet, EquipmentSlot.Weapon, EquipmentSlot.Shield };
        for (int i = 0; i < slots.Length; i++)
        {
            Item item = inventory.Equipped(slots[i]);
            if (item != null)
                saved.Add(ToSave(item, 0, 0));
        }

        return saved.ToArray();
    }

    private static ItemSave ToSave(Item item, int x, int y)
    {
        return new ItemSave
        {
            id = item.Id,
            displayName = item.DisplayName,
            kind = (int)item.Kind,
            slot = item is EquipmentItem equipment ? (int)equipment.Slot : 0,
            x = x,
            y = y,
            modifiers = item is EquipmentItem equipped ? equipped.Modifiers.ToArray() : null,
            requirements = item is EquipmentItem required ? required.Requirements.ToArray() : null,
            thrownWeapon = item is EquipmentItem thrown && thrown.ThrownWeapon,
            throwDamage = item is EquipmentItem damage ? damage.ThrowDamage : 0,
            sharp = item is EquipmentItem sharp && sharp.Sharp,
            count = item.Count,
            charge = item is RuneStone rune ? rune.Charge : item is StaffItem staff && staff.Spell != null ? staff.Spell.Charge : 0f,
            upgradeLevel = item is EquipmentItem upgraded ? upgraded.Level : 0,
            durability = item.TracksDurability ? item.Durability : 0,
            maxDurability = item.TracksDurability ? item.MaxDurability : 0,
            age = item is Food food ? food.Age : 0,
            spellLevel = item is Scroll scroll ? scroll.SpellLevel
                : item is RuneStone runeSpell ? runeSpell.SpellLevel
                : item is StaffItem staffSpell && staffSpell.Spell != null ? staffSpell.Spell.SpellLevel
                : 0,
            runeLevel = item is RuneStone runeLevel ? runeLevel.Level
                : item is StaffItem staffLevel && staffLevel.Spell != null ? staffLevel.Spell.RuneLevel
                : 0
        };
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
