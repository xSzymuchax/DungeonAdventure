using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public int Health { get; set; }
    public bool TakeDamage(int amount);
}
