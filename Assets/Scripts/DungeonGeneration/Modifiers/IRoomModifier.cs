using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields);
}
