using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public float Health { get; }
    public float MaxHealth { get; }
    public event System.Action HealthChanged;
    public bool IsDead => Health <= 0;
    public bool TakeDamage(float amount, DamageType type, float scale);
    public void Heal(float amount);
    public IEnumerator PlayDeathAnimation();
    public DamagePlate DamagePlate { get; }

    public void ShowDamageNumber(float amount, DamageType type, Vector3 world, float scale)
    {
        int shown = Mathf.FloorToInt(amount + 0.5f);
        if (shown <= 0 || DamagePlate == null)
            return;

        world.x += Random.Range(-4f, 4f);
        world.z += Random.Range(-4f, 4f);
        ShowPlate(shown, DamageTypes.ColorOf(type), world, scale);
    }

    public void ShowHealNumber(float amount, Vector3 world)
    {
        int shown = Mathf.FloorToInt(amount + 0.5f);
        if (shown <= 0 || DamagePlate == null)
            return;

        world.x += Random.Range(-4f, 4f);
        world.z += Random.Range(-4f, 4f);
        ShowPlate(shown, DamageTypes.Heal, world, 1f);
    }

    private void ShowPlate(int shown, Color color, Vector3 world, float scale)
    {
        DamagePlate plate = Object.Instantiate(DamagePlate, world, Quaternion.identity);
        plate.Launch(shown, color, scale);
    }
}
