using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballSkill", menuName = "Skills/Fireball")]
public class FireballSkill : Projectile
{
    public GameObject fireballPrefab;
    [SerializeField] int baseDamage = 5;
    [SerializeField] int level = 1;
    int castLevel;

    public override string Name => "Fireball";
    public override int Range => Consts.RANGED_ATTACK_RANGE;
    public override int Level => Mathf.Clamp(castLevel > 0 ? castLevel : level, 1, MaxLevel);
    public override int Damage => Scaled(Level);

    public override float Power(int spellLevel, int knowledge)
    {
        return Scaled(spellLevel) + knowledge;
    }

    int Scaled(int spellLevel)
    {
        return baseDamage * spellLevel;
    }

    public override void BeginCast(int spellLevel)
    {
        castLevel = Mathf.Clamp(spellLevel, 1, MaxLevel);
    }

    public override void EndCast()
    {
        castLevel = 0;
    }

    void OnValidate()
    {
        level = Mathf.Clamp(level, 1, MaxLevel);
        if (baseDamage < 0)
            baseDamage = 0;
    }
    public override DamageType HitDamageType => DamageType.Fire;

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
