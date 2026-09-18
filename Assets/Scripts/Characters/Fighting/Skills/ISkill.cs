using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
    public string Name { get; }
    public double ManaCost { get; }
    public int Range { get; }
    public bool CanUse(ISkillCaster user, Position2D target);
    public IEnumerator Use(ISkillCaster user, Position2D target);
}
