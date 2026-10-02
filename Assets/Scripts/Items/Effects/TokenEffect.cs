using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "TokenEffect", menuName = "Dungeon/Effect/Token")]
public class TokenEffect : Effect
{
    public string tokenType;
    public int strength = 1;
    public int duration = 3;
    public string label = "Token";
    public bool needsTarget = true;
    public int range = 1;

    public override string Label => string.IsNullOrEmpty(label) ? name : label;
    public override bool NeedsTarget => needsTarget;
    public override int Range => range;

    public override IEnumerator Apply(PlayerCharacter player, Position2D target)
    {
        if (player == null || GameController.Instance == null || GameController.Instance.dungeon == null)
            yield break;
        if (!GameController.Instance.dungeon.TryGetDamagableAt(target, out IDamagable damagable))
            yield break;
        if (damagable is not ITokenHost host)
            yield break;

        IToken token = TokenFactory.Create(tokenType, strength, duration);
        if (token != null)
            host.AddToken(token);
        yield break;
    }
}
