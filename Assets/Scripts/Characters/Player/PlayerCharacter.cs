using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character, IHasPerception
{
    [SerializeField] FireballSkill fireball;

    public int WakeUpRange => throw new System.NotImplementedException();

    public int AttackRange => throw new System.NotImplementedException();

    public int ViewRange => 5;

    public int DetectionRange => throw new System.NotImplementedException();

    public ISkill Fireball => fireball;

    protected override void PopulateSkills()
    {
        base.PopulateSkills();
        if (fireball != null)
            skills.Add(fireball);
    }

    protected override void InitMoveCosts()
    {
        moveCostManager = new();
        moveCostManager.AddCost(FloorFieldType.BASE_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.CORRIDOR_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.POSSIBLE_DOOR_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.SPAWN_FIELD, 1);
        moveCostManager.AddCost(FloorFieldType.EXIT_FIELD, 1);
    }
}
