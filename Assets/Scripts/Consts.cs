using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Consts 
{
    // generation
    public static int MAX_RETRIES_AMOUNT_FOR_ROOM_FIT = 100;
    public static float TILE_SIZE = 10;

    // animations
    public static float WALK_ANIMATION_TIME = .1f;

    // game
    public static float GAME_SPEED = 10f;

    // enemies
    public static string GRAPHIC_REPRESENTATION_IN_ACTOR_NAME = "Visual";
    public static float ENEMY_DROP_CHANCE = 0.1f;

    // combat
    public static int RANGED_ATTACK_RANGE = 10;

    // items
    public static int DEFAULT_DURABILITY = 10;
    public static float WEAPON_WEAR_CHANCE = 0.05f;
    public static float ARMOR_WEAR_CHANCE = 0.05f;
    public static float BAG_WEAR_CHANCE = 0.001f;
    public static float UPGRADE_CHANCE = 0.2f;
    public static int MAX_UPGRADE_LEVEL = 3;
    public static int FOOD_STAGE_TURNS = 100;
    public static float FOOD_STAGE_LOSS = 0.3f;
    public static float FOOD_SPOILED_POISON_CHANCE = 0.1f;
    public static float FOOD_ROTTEN_POISON_CHANCE = 0.75f;
    public static float RUNE_CHARGE_PER_TURN = 0.1f;
}
