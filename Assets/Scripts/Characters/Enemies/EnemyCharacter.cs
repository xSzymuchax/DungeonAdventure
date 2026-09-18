using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character, IHasPerception
{
    public int ViewRange => GetStats().ViewRange;
    public int WakeUpRange => GetStats().WakeUpRange;
    public int AttackRange => GetStats().AttackRange;
    public int DetectionRange => GetStats().DetectionRange;

    protected override void OnDamaged()
    {
        EnemyController controller = GetComponent<EnemyController>();
        controller?.OnDamaged();
    }
}
