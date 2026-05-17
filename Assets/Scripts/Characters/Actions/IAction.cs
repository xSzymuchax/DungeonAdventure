using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAction
{
    public double Cost {get;}
    public void PerformAction();
}
