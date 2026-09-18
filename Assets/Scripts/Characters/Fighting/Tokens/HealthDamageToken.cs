using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDamageToken : IToken
{
    public int DamagePerTurn { get; }
    public int RemainingTurns { get; private set; }
    public bool IsExpired => RemainingTurns <= 0;

    public HealthDamageToken(int damagePerTurn, int durationTurns)
    {
        DamagePerTurn = damagePerTurn;
        RemainingTurns = durationTurns;
    }

    public void Tick(IDamagable target)
    {
        if (IsExpired || target == null)
            return;

        target.TakeDamage(DamagePerTurn);
        RemainingTurns--;
    }
}
