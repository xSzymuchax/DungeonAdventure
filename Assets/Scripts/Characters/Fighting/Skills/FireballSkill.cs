using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballSkill", menuName = "Skills/Fireball")]
public class FireballSkill : Projectile
{
    public GameObject fireballPrefab;
    [SerializeField] int baseDamage = 5;
    [SerializeField] int level = 1;

    public override string Name => "Fireball";
    public override int Range => Consts.RANGED_ATTACK_RANGE;
    public override int Damage => baseDamage * Mathf.Max(1, level);
    protected override DamageType HitDamageType => DamageType.Fire;

    protected override IEnumerable<IToken> CreateTokens()
    {
        yield return new BurningToken();
    }

    protected override GameObject CreateView()
    {
        if (fireballPrefab == null)
            return null;

        return Instantiate(fireballPrefab);
    }
}
