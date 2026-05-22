using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyController 
{
    public IActor Actor { get; }
    public IEnumerator MakeMove();
}
