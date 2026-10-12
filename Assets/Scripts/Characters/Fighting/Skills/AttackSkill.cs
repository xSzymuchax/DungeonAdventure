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

    public virtual IEnumerator StrikeFrom(ISkillCaster user, Position2D origin, Position2D target)
    {
        yield return ApplyHitAndDeath(user, target);
    }

    protected IEnumerator ApplyHitAndDeath(ISkillCaster user, Position2D target)
    {
        IDamagable hit = ApplyHit(user, target);
        if (hit != null && hit.IsDead)
            yield return hit.PlayDeathAnimation();
    }

    public IEnumerator HitWithPower(Position2D target, float power)
    {
        IDamagable hit = ApplyHit(target, power);
        if (hit != null && hit.IsDead)
            yield return hit.PlayDeathAnimation();
    }

    protected IDamagable ApplyHit(ISkillCaster user, Position2D target)
    {
        if (!GameController.Instance.dungeon.TryGetDamagableAt(target, out IDamagable damagable))
            return null;

        Character attacker = user as Character;
        DamageResult result = DamageCalculator.Melee(attacker, damagable, HitDamageType, SpellPower(user));
        return FinishHit(attacker, damagable, result);
    }

    protected IDamagable ApplyHit(Position2D target, float power)
    {
        if (!GameController.Instance.dungeon.TryGetDamagableAt(target, out IDamagable damagable))
            return null;

        float shot = DamageCalculator.AttackBase(HitDamageType, power, 0, 0f);
        DamageResult result = DamageCalculator.Deliver(null, damagable, HitDamageType, shot);
        return FinishHit(null, damagable, result);
    }

    IDamagable FinishHit(Character attacker, IDamagable damagable, DamageResult result)
    {
        if (result.Dodged)
            return damagable;

        if (result.Amount > 0)
            damagable.TakeDamage(result.Amount, result.Type, result.Scale);

        if (HitDamageType == DamageType.Physical)
            OnHitEffect.ApplyAll(AttackOnHit(attacker), attacker, damagable);

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

    static IList<OnHitEffect> AttackOnHit(Character attacker)
    {
        if (attacker is EnemyCharacter enemy)
            return enemy.OnHits;
        if (attacker is not PlayerCharacter player || player.Inventory == null)
            return null;
        if (player.Inventory.Equipped(EquipmentSlot.Weapon) is not EquipmentItem weapon)
            return null;
        return weapon.OnHit;
    }
}
