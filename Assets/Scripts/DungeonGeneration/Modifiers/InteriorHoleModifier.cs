using UnityEngine;

public class InteriorHoleModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        if (!FloorBounds(roomFields, out int xMin, out int yMin, out int xMax, out int yMax))
            return roomFields;

        int floorWidth = xMax - xMin + 1;
        int floorHeight = yMax - yMin + 1;
        int holeWidth = Random.Range(1, floorWidth + 1);
        int holeHeight = Random.Range(1, floorHeight + 1);
        if (holeWidth == floorWidth && holeHeight == floorHeight)
        {
            if (floorWidth > 1 && (floorHeight == 1 || Random.value < 0.5f))
                holeWidth--;
            else if (floorHeight > 1)
                holeHeight--;
            else
                return roomFields;
        }

        int x0 = xMin + Random.Range(0, floorWidth - holeWidth + 1);
        int y0 = yMin + Random.Range(0, floorHeight - holeHeight + 1);
        FloorFieldType[,] result = (FloorFieldType[,])roomFields.Clone();

        for (int x = x0; x < x0 + holeWidth; x++)
            for (int y = y0; y < y0 + holeHeight; y++)
                if (result[x, y] == FloorFieldType.BASE_FIELD)
                    result[x, y] = FloorFieldType.EMPTY;

        return result;
    }

    static bool FloorBounds(FloorFieldType[,] fields, out int xMin, out int yMin, out int xMax, out int yMax)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        xMin = width;
        yMin = height;
        xMax = -1;
        yMax = -1;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fields[x, y] != FloorFieldType.BASE_FIELD)
                    continue;
                if (x < xMin) xMin = x;
                if (y < yMin) yMin = y;
                if (x > xMax) xMax = x;
                if (y > yMax) yMax = y;
            }
        }

        return xMax >= xMin && yMax >= yMin;
    }
}
