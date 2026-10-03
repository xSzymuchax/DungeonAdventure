using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill : ScriptableObject, ISkill
{
    public const int MaxLevel = 5;

    public abstract string Name { get; }
    public abstract float ManaCost { get; }
    public abstract int Range { get; }
    public virtual int Level => 1;

    protected static int KnowledgeOf(ISkillCaster user)
    {
        if (user is IHasStats hasStats && hasStats.Stats is PlayerStats stats)
            return stats.Knowledge;
        return 0;
    }

    public virtual void BeginCast(int spellLevel)
    {
    }

    public virtual void EndCast()
    {
    }

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
