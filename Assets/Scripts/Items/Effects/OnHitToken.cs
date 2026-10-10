using UnityEngine;

[CreateAssetMenu(fileName = "OnHitToken", menuName = "Dungeon/On Hit/Token")]
public class OnHitToken : OnHitEffect
{
    public string tokenType;
    public int strength = 1;
    public int duration = 3;
    public string label;

    public override string Label => string.IsNullOrEmpty(label) ? name : label;

    public override void Apply(Character attacker, IDamagable target)
    {
        if (target == null || target.IsDead || target is not ITokenHost host)
            return;

        IToken token = TokenFactory.Create(tokenType, strength, duration);
        if (token != null)
            host.AddToken(token);
    }
}
