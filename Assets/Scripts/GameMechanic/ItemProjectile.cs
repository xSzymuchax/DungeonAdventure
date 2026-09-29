using System.Collections;
using UnityEngine;

public class ItemProjectile
{
    const float Arc = 6f;
    const float MinDuration = 0.15f;
    const int BluntDamage = 1;

    readonly Item item;
    readonly Character thrower;

    public bool Landed { get; private set; }

    public ItemProjectile(Item item, Character thrower)
    {
        this.item = item;
        this.thrower = thrower;
    }

    public IEnumerator Launch(Position2D from, Position2D to)
    {
        Landed = false;
        GameController controller = GameController.Instance;
        if (item == null || controller == null || controller.Projectiles == null)
            yield break;

        Position2D landing = controller.Projectiles.Impact(from, to, ProjectileStop.BeforeObstacle);
        if (!controller.CanThrowAt(landing))
            yield break;

        GameObject view = controller.CreateItemView(item, from);
        float height = view != null ? ItemSpawnSystem.ViewSize * 0.5f : 0f;
        yield return controller.Projectiles.Fly(view, from, landing, height, Arc, MinDuration);

        if (controller.dungeon != null
            && controller.dungeon.TryGetDamagableAt(landing, out IDamagable hit)
            && !ReferenceEquals(hit, thrower))
        {
            Strike(hit);
            if (item.Sharp && hit is Character character)
            {
                character.Lodge(item, controller.CreateItemView(item, landing));
                Landed = true;
            }
            else
            {
                Landed = controller.PlaceItem(item, landing);
            }

            if (hit.IsDead)
                yield return hit.PlayDeathAnimation();
            yield break;
        }

        Landed = controller.PlaceItem(item, landing);
    }

    void Strike(IDamagable hit)
    {
        int damage = item.ThrownWeapon ? item.ThrowDamage + ThrowerDamage() : BluntDamage;
        if (hit is IHasStats target && target.Stats != null)
            damage -= target.Stats.Defense;
        if (damage > 0)
            hit.TakeDamage(damage);
    }

    int ThrowerDamage()
    {
        if (thrower == null || thrower.Stats == null)
            return 0;
        return thrower.Stats.Damage;
    }
}
