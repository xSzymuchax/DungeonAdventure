using System.Collections;
using UnityEngine;

public abstract class Projectile : AttackSkill
{
    const float FlightHeight = 3f;
    const float MinDuration = 0.08f;

    protected virtual GameObject CreateView()
    {
        return null;
    }

    public override IEnumerator Use(ISkillCaster user, Position2D target)
    {
        ProjectileSystem system = GameController.Instance != null ? GameController.Instance.Projectiles : null;
        Position2D impact = system != null
            ? system.Impact(user.Position, target, ProjectileStop.OnObstacle)
            : target;
        if (system != null)
            yield return system.Fly(CreateView(), user.Position, impact, FlightHeight, 0f, MinDuration);
        yield return ApplyHitAndDeath(user, impact);
    }
}
