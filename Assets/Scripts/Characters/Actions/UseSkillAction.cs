using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseSkillAction : IAction
{
    public double Cost => Consts.GAME_SPEED;

    private readonly ISkill skill;
    private readonly ISkillCaster user;
    private readonly Position2D target;

    public UseSkillAction(ISkill skill, ISkillCaster user, Position2D target)
    {
        this.skill = skill;
        this.user = user;
        this.target = target;
    }

    public IEnumerator PerformAction()
    {
        Debug.Log("UseSkillAction " + skill.Name);

        if (!user.HasEnergy || !skill.CanUse(user, target))
            yield break;

        user.RemoveEnergy(Cost);
        user.RemoveMana(skill.ManaCost);
        yield return skill.Use(user, target);
    }
}
