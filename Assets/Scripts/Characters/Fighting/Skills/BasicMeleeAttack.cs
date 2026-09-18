using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMeleeAttack : AttackSkill
{
    public override string Name => "Melee";
    public override int Range => 1;
    public override int Damage => 0;

    public override bool CanUse(ISkillCaster user, Position2D target)
    {
        if (!base.CanUse(user, target))
            return false;

        return GameController.Instance.dungeon.TryGetDamagableAt(target, out _);
    }
}
