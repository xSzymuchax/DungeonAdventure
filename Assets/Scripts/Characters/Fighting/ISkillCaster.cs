using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkillCaster : IActor
{
    public double Mana { get; set; }

    public bool HasMana(double amount) => Mana >= amount;
    public void AddMana(double amount);
    public void RemoveMana(double amount);
}
