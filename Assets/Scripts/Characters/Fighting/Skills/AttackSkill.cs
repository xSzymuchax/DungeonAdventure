using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackSkill : Skill
{
    [SerializeField] protected float manaCost = 0;
    public override float ManaCost => manaCost;
    public abstract int Damage { get; }
    protected virtual DamageType HitDamageType => DamageType.Physical;

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
        if (damagable is Character defender && defender.RollDodge(attacker, HitDamageType))
            return damagable;

        int damage = ResolveDamage(user);
        if (damage > 0)
            damagable.TakeDamage(damage, HitDamageType);

        if (!damagable.IsDead && damagable is ITokenHost tokenHost)
        {
            foreach (IToken token in CreateTokens())
                tokenHost.AddToken(token);
        }

        return damagable;
    }

    protected virtual int ResolveDamage(ISkillCaster user)
    {
        CharacterStats stats = user is IHasStats hasStats ? hasStats.Stats : null;
        if (DamageTypes.IsMagical(HitDamageType))
        {
            float amplify = stats != null ? stats.MagicAmplify : 0f;
            float power = Mathf.Max(0f, SpellPower(user));
            float boosted = power * (1f + amplify / 100f);
            return DamageRoll.Of(boosted);
        }

        float attack = stats != null ? stats.Damage : 0f;
        return DamageRoll.Of(attack);
    }

    protected virtual float SpellPower(ISkillCaster user)
    {
        return Damage;
    }
}
