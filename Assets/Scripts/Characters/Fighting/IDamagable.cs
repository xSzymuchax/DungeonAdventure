using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public int Health { get; }
    public int MaxHealth { get; }
    public event System.Action HealthChanged;
    public bool IsDead => Health <= 0;
    public bool TakeDamage(int amount);
    public void Heal(int amount);
    public IEnumerator PlayDeathAnimation();
}
