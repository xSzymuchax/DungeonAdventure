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
    public bool Released { get; private set; }

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

        Released = true;
        bool keep = item is not Ammunition ammo || ammo.SpendUse();
        GameObject view = controller.CreateItemView(item, from);
        float height = view != null ? ItemSpawnSystem.ViewSize * 0.5f : 0f;
        yield return controller.Projectiles.Fly(view, from, landing, height, Arc, MinDuration);

        if (controller.dungeon != null
            && controller.dungeon.TryGetDamagableAt(landing, out IDamagable hit)
            && !ReferenceEquals(hit, thrower))
        {
            Strike(hit);
            if (!keep)
                Landed = true;
            else if (hit is Character character && Sticks(item))
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

        if (!keep)
            Landed = true;
        else
            Landed = controller.PlaceItem(item, landing);
    }

    static bool Sticks(Item thrown)
    {
        if (thrown is Ammunition)
            return true;
        return thrown is EquipmentItem equipment && equipment.Sharp;
    }

    void Strike(IDamagable hit)
    {
        if (hit is Character target && target.RollDodge(thrower, DamageType.Physical))
            return;

        int damage = DamageRoll.Of(ThrownDamage());
        if (damage > 0)
            hit.TakeDamage(damage, DamageType.Physical);
    }

    int ThrownDamage()
    {
        if (item is Ammunition ammo)
            return ammo.Damage + LauncherBonus(ammo);
        if (item is EquipmentItem equipment && equipment.ThrownWeapon)
            return equipment.ThrowDamage;
        if (item is RuneStone rune)
            return rune.ThrowDamage;
        return BluntDamage;
    }

    int LauncherBonus(Ammunition ammo)
    {
        if (ammo.Launcher == WeaponKind.None || thrower is not PlayerCharacter player || player.Inventory == null)
            return 0;
        if (player.Inventory.Equipped(EquipmentSlot.Weapon) is not EquipmentItem weapon || weapon.WeaponKind != ammo.Launcher)
            return 0;

        StatId stat = AmmoStat(ammo.Launcher);
        float total = 0f;
        float factor = ItemUpgrade.Factor(weapon.Level);
        for (int i = 0; i < weapon.Modifiers.Count; i++)
        {
            StatModifier modifier = weapon.Modifiers[i];
            if (modifier != null && modifier.stat == stat)
                total += modifier.value * factor;
        }

        return Mathf.RoundToInt(total);
    }

    static StatId AmmoStat(WeaponKind kind)
    {
        if (kind == WeaponKind.Crossbow)
            return StatId.BoltDamage;
        if (kind == WeaponKind.Blowgun)
            return StatId.DartDamage;
        return StatId.ArrowDamage;
    }
}
