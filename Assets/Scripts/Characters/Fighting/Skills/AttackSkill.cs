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
        ApplyHit(target);
        yield return null;
    }

    protected void ApplyHit(Position2D target)
    {
        if (!GameController.Instance.dungeon.TryGetDamagableAt(target, out IDamagable damagable))
            return;

        if (Damage > 0)
            damagable.TakeDamage(Damage);

        if (damagable is ITokenHost tokenHost)
        {
            foreach (IToken token in CreateTokens())
                tokenHost.AddToken(token);
        }
    }
}
