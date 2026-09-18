using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill : ScriptableObject, ISkill
{
    public abstract string Name { get; }
    public abstract double ManaCost { get; }
    public abstract int Range { get; }

    public virtual bool CanUse(ISkillCaster user, Position2D target)
    {
        if (user == null)
            return false;
        if (!user.HasMana(ManaCost))
            return false;
        if (user.Position == target)
            return false;

        int distance = user.Position.ChebyshevTo(target);
        return distance > 0 && distance <= Range;
    }

    public abstract IEnumerator Use(ISkillCaster user, Position2D target);
}
