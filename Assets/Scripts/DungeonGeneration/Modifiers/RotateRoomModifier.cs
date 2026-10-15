using UnityEngine;

public class RotateRoomModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int turns = Random.Range(0, 4);
        FloorFieldType[,] current = roomFields;
        for (int turn = 0; turn < turns; turn++)
            current = RotateClockwise(current);
        return current;
    }

    static FloorFieldType[,] RotateClockwise(FloorFieldType[,] source)
    {
        int width = source.GetLength(0);
        int height = source.GetLength(1);
        FloorFieldType[,] rotated = new FloorFieldType[height, width];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                rotated[height - 1 - y, x] = source[x, y];
        return rotated;
    }
}
