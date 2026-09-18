using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackSkill : Skill
{
    [SerializeField] protected double manaCost = 0;
    public override double ManaCost => manaCost;
    public abstract int Damage { get; }

    protected virtual IEnumerable<IToken> CreateTokens()
    {
        yield break;
    }

    public override IEnumerator Use(ISkillCaster user, Position2D target)
    {
        yield return ApplyHitAndDeath(user, target);
    }

    protected IEnumerator ApplyHitAndDeath(ISkillCaster user, Position2D target)
    {
        IDamagable hit = ApplyHit(user, target);
        if (hit != null && hit.IsDead)
            yield return hit.PlayDeathAnimation();
    }

    protected IDamagable ApplyHit(ISkillCaster user, Position2D target)
    {
        if (!GameController.Instance.dungeon.TryGetDamagableAt(target, out IDamagable damagable))
            return null;

        int damage = ResolveDamage(user);
        if (damage > 0)
            damagable.TakeDamage(damage);

        if (!damagable.IsDead && damagable is ITokenHost tokenHost)
        {
            foreach (IToken token in CreateTokens())
                tokenHost.AddToken(token);
        }

        return damagable;
    }

    protected virtual int ResolveDamage(ISkillCaster user)
    {
        int characterDamage = 0;
        if (user is IHasStats hasStats && hasStats.Stats != null)
            characterDamage = hasStats.Stats.Damage;
        return characterDamage + Damage;
    }
}
