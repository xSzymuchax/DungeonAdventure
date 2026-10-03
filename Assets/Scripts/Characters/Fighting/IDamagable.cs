using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public float Health { get; }
    public float MaxHealth { get; }
    public event System.Action HealthChanged;
    public bool IsDead => Health <= 0;
    public bool TakeDamage(float amount);
    public bool TakeDamage(float amount, DamageType type);
    public void Heal(float amount);
    public IEnumerator PlayDeathAnimation();
}
