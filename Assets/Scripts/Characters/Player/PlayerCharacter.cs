using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character, IHasPerception
{
    [SerializeField] FireballSkill fireball;

    public int WakeUpRange => GetStats().WakeUpRange;
    public int AttackRange => GetStats().AttackRange;
    public int ViewRange => GetStats().ViewRange;
    public int DetectionRange => GetStats().DetectionRange;

    public ISkill Fireball => fireball;

    protected override void PopulateSkills()
    {
        base.PopulateSkills();
        if (fireball != null)
            skills.Add(fireball);
    }
}
