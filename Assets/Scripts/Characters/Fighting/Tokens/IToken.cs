using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToken
{
    public bool IsExpired { get; }
    public void Tick(IDamagable target);
}
