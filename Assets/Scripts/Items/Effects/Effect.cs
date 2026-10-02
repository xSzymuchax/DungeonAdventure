using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    public virtual string Label => name;
    public abstract bool NeedsTarget { get; }
    public virtual int Range => 1;

    public abstract IEnumerator Apply(PlayerCharacter player, Position2D target);

    public static IEnumerator ApplyAll(IList<Effect> effects, PlayerCharacter player, Position2D target)
    {
        if (effects == null)
            yield break;

        for (int i = 0; i < effects.Count; i++)
        {
            Effect effect = effects[i];
            if (effect != null)
                yield return effect.Apply(player, target);
        }
    }
}
