using UnityEngine;

public class LShapeModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int width = roomFields.GetLength(0);
        int height = roomFields.GetLength(1);
        if (width < Consts.ROOM_PASSAGE_WIDTH * 2 || height < Consts.ROOM_PASSAGE_WIDTH * 2)
            return roomFields;

        int cutWidth = Random.Range(1, width - Consts.ROOM_PASSAGE_WIDTH + 1);
        int cutHeight = Random.Range(1, height - Consts.ROOM_PASSAGE_WIDTH + 1);
        FloorFieldType[,] result = (FloorFieldType[,])roomFields.Clone();
        for (int x = 0; x < cutWidth; x++)
            for (int y = 0; y < cutHeight; y++)
                result[x, y] = FloorFieldType.EMPTY;

        return result;
    }
}
