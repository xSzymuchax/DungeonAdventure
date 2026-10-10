using UnityEngine;

[CreateAssetMenu(fileName = "OnHitDamage", menuName = "Dungeon/On Hit/Damage")]
public class OnHitDamage : OnHitEffect
{
    public DamageType type = DamageType.Poison;
    public int amount = 1;
    public string label;

    public override string Label => string.IsNullOrEmpty(label) ? name : label;

    public override void Apply(Character attacker, IDamagable target)
    {
        if (target == null || amount <= 0)
            return;

        DamageResult result = DamageCalculator.Direct(target, type, amount);
        if (result.Amount > 0)
            target.TakeDamage(result.Amount, result.Type, result.Scale);
    }
}
