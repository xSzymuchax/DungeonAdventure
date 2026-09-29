using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAction
{
    public float Cost {get;}
    public IEnumerator PerformAction();
}
