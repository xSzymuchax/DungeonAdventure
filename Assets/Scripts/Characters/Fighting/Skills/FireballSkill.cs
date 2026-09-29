using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballSkill", menuName = "Skills/Fireball")]
public class FireballSkill : Projectile
{
    public GameObject fireballPrefab;

    public override string Name => "Fireball";
    public override int Range => 6;
    public override int Damage => 0;

    protected override IEnumerable<IToken> CreateTokens()
    {
        yield return new BurningToken(1, 3);
    }

    protected override GameObject CreateView()
    {
        if (fireballPrefab == null)
            return null;

        return Instantiate(fireballPrefab);
    }
}
