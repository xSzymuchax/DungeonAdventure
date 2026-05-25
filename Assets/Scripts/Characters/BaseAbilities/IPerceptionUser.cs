using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPerceptionUser 
{
    bool CanAttack(IActor target);
    bool CanSee(IActor target);
    bool CanDetect(IActor target);
    bool CanWakeUp(IActor target);
}
