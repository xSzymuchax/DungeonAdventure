using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballSkill", menuName = "Skills/Fireball")]
public class FireballSkill : AttackSkill
{
    public GameObject fireballPrefab;

    public override string Name => "Fireball";
    public override int Range => 6;
    public override int Damage => 0;

    protected override IEnumerable<IToken> CreateTokens()
    {
        yield return new HealthDamageToken(1, 3);
    }

    public override IEnumerator Use(ISkillCaster user, Position2D target)
    {
        Position2D impact = FindImpact(user.Position, target);
        yield return FlyTo(user.Position, impact);
        yield return ApplyHitAndDeath(user, impact);
    }

    private Position2D FindImpact(Position2D from, Position2D to)
    {
        List<Position2D> line = from.LineTo(to);
        Dungeon dungeon = GameController.Instance.dungeon;

        for (int i = 1; i < line.Count; i++)
        {
            if (dungeon.IsWall(line[i]))
                return line[i];
        }

        return to;
    }

    private IEnumerator FlyTo(Position2D from, Position2D to)
    {
        if (fireballPrefab == null)
            yield break;

        Dungeon dungeon = GameController.Instance.dungeon;
        Vector3 start = dungeon.GetTileWorldPosition(from) + Vector3.up * 3f;
        Vector3 end = dungeon.GetTileWorldPosition(to) + Vector3.up * 3f;
        float duration = Mathf.Max(0.08f, Consts.WALK_ANIMATION_TIME * from.ChebyshevTo(to));

        GameObject orb = Instantiate(fireballPrefab);
        orb.transform.position = start;

        float progress = 0f;
        while (progress < duration)
        {
            progress += Time.deltaTime;
            orb.transform.position = Vector3.Lerp(start, end, progress / duration);
            yield return null;
        }

        orb.transform.position = end;
        Destroy(orb);
    }
}
