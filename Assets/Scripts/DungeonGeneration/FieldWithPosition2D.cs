using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldWithPosition2D
{
    public int x;
    public int y;
    public FloorFieldType fieldType;

    public FieldWithPosition2D(int x, int y, FloorFieldType fieldType)
    {
        this.x = x;
        this.y = y;
        this.fieldType = fieldType;
    }
}
