using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackSkill : Skill
{
    [SerializeField] protected float manaCost = 0;
    public override float ManaCost => manaCost;
    public abstract int Damage { get; }
    public virtual DamageType HitDamageType => DamageType.Physical;

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

        Character attacker = user as Character;
        DamageResult result = DamageCalculator.Melee(attacker, damagable, HitDamageType, SpellPower(user));
        if (result.Dodged)
            return damagable;

        if (result.Amount > 0)
            damagable.TakeDamage(result.Amount, result.Type, result.Scale);

        if (!damagable.IsDead && damagable is ITokenHost tokenHost)
        {
            foreach (IToken token in CreateTokens())
                tokenHost.AddToken(token);
        }

        return damagable;
    }

    public virtual float Power(int spellLevel, int knowledge)
    {
        return Damage;
    }

    protected virtual float SpellPower(ISkillCaster user)
    {
        int knowledge = 0;
        if (user is IHasStats hasStats && hasStats.Stats is PlayerStats stats)
            knowledge = stats.Knowledge;
        return Power(Level, knowledge);
    }
}
