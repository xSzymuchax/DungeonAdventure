using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRoomModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int width = roomFields.GetLength(0);
        int height = roomFields.GetLength(1);

        while (true)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            if (roomFields[x, y] == FloorFieldType.BASE_FIELD)
            {
                roomFields[x, y] = FloorFieldType.SPAWN_FIELD;
                return roomFields;
            }
                
        }
    }
}
