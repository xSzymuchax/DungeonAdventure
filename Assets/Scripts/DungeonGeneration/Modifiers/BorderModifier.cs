using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BorderModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int newWidth = roomFields.GetLength(0) + 2;
        int newHeigth = roomFields.GetLength(1) + 2;
        FloorFieldType[,] newRoom = new FloorFieldType[newWidth, newHeigth];
        
        for (int i = 1; i < newWidth-1; i++)
        {
            for (int j = 1; j < newHeigth-1; j++)
            {
                newRoom[i, j] = roomFields[i-1, j-1];
            }
        }

        for (int i = 0; i < newWidth; i++)
        {
            for (int j = 0; j < newHeigth; j++)
            {
                if (newRoom[i,j] == FloorFieldType.EMPTY)
                {
                    for (int di = i-1; di <= i+1; di++)
                    {
                        for (int dj = j-1; dj <= j+1; dj++)
                        {
                            if (di < 0 || di >= newWidth || dj < 0 || dj >= newHeigth)
                                continue;

                            if (newRoom[di,dj] == FloorFieldType.BASE_FIELD)
                            {
                                newRoom[i,j] = FloorFieldType.SIDE_FIELD;
                                break;
                            }    
                        }
                    }
                }
            }
        }

        return newRoom;
    }
}
