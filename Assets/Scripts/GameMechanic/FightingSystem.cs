using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingSystem : MonoBehaviour
{
    public bool CanUse(ISkill skill, ISkillCaster caster, Position2D target)
    {
        return skill != null && caster != null && caster.HasEnergy && skill.CanUse(caster, target);
    }

    public bool CanUse(ISkill skill, ISkillCaster caster, IDamagable target)
    {
        if (target is not IHasPosition positioned)
            return false;
        return CanUse(skill, caster, positioned.Position);
    }

    public IEnumerator UseSkill(ISkill skill, ISkillCaster caster, Position2D target)
    {
        if (!CanUse(skill, caster, target))
            yield break;

        IAction action = new UseSkillAction(skill, caster, target);
        yield return StartCoroutine(action.PerformAction());
    }

    public IEnumerator UseSkill(ISkill skill, ISkillCaster caster, IDamagable target)
    {
        if (target is not IHasPosition positioned)
            yield break;
        yield return UseSkill(skill, caster, positioned.Position);
    }

    public IEnumerator UseBasicAttack(IFighter attacker, IDamagable target)
    {
        if (attacker is not ISkillCaster caster)
            yield break;

        yield return UseSkill(attacker.BasicAttack, caster, target);
    }

    public void TickTokens(ITokenHost host)
    {
        host?.TickTokens();
    }
}
