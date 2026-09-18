using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFighter : IActor
{
    public ISkill BasicAttack { get; }
    public IReadOnlyList<ISkill> Skills { get; }
}
