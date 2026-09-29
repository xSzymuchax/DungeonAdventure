using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkillCaster : IActor
{
    public float Mana { get; set; }

    public bool HasMana(float amount) => Mana >= amount;
    public void AddMana(float amount);
    public void RemoveMana(float amount);
}
