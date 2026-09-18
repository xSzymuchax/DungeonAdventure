using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingSystem : MonoBehaviour
{
    public bool CanUse(ISkill skill, ISkillCaster caster, Position2D target)
    {
        if (skill == null || caster == null || !caster.HasEnergy)
            return false;
        return skill.CanUse(caster, target);
    }

    public bool CanUse(ISkill skill, ISkillCaster caster, IDamagable target)
    {
        if (target == null || target.IsDead || target is not IHasPosition positioned)
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

    public IEnumerator TickTokens(ITokenHost host)
    {
        if (host == null)
            yield break;

        host.TickTokens();
        if (host is IDamagable damagable && damagable.IsDead)
            yield return damagable.PlayDeathAnimation();
    }
}
