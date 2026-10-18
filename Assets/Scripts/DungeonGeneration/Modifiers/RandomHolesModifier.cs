using System.Collections.Generic;
using UnityEngine;

public class RandomHolesModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int width = roomFields.GetLength(0);
        int height = roomFields.GetLength(1);
        List<Position2D> floor = new List<Position2D>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (roomFields[x, y] == FloorFieldType.BASE_FIELD)
                    floor.Add(new Position2D { x = x, y = y });
            }
        }

        int holes = floor.Count / 2;
        FloorFieldType[,] result = (FloorFieldType[,])roomFields.Clone();
        for (int i = 0; i < holes; i++)
        {
            int index = Random.Range(0, floor.Count);
            Position2D cell = floor[index];
            floor.RemoveAt(index);
            result[cell.x, cell.y] = FloorFieldType.EMPTY;
        }

        return result;
    }
}
