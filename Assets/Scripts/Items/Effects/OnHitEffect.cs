using System.Collections.Generic;
using UnityEngine;

public abstract class OnHitEffect : ScriptableObject
{
    public virtual string Label => name;

    public abstract void Apply(Character attacker, IDamagable target);

    public static void ApplyAll(IList<OnHitEffect> effects, Character attacker, IDamagable target)
    {
        if (effects == null || target == null)
            return;

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null)
                effects[i].Apply(attacker, target);
        }
    }
}
