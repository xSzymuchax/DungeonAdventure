using UnityEngine;

public class SShapeModifier : IRoomModifier
{
    const int Bands = 5;
    readonly bool forceHoles;

    public SShapeModifier(bool forceHoles)
    {
        this.forceHoles = forceHoles;
    }

    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int width = roomFields.GetLength(0);
        int height = roomFields.GetLength(1);
        if (width < Consts.ROOM_PASSAGE_WIDTH * 2 || height < Bands)
            return roomFields;

        bool cutHoles = forceHoles || (width >= 8 && height >= 8);
        FloorFieldType block = cutHoles ? FloorFieldType.EMPTY : FloorFieldType.SIDE_FIELD;
        int[] lengths = Split(height, Bands);
        FloorFieldType[,] result = (FloorFieldType[,])roomFields.Clone();
        int y0 = 0;

        for (int band = 0; band < Bands; band++)
        {
            bool connector = band % 2 == 1;
            bool openOnRight = (band / 2) % 2 == 0;
            for (int y = y0; y < y0 + lengths[band]; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!connector || result[x, y] != FloorFieldType.BASE_FIELD)
                        continue;

                    bool open = openOnRight ? x >= width - Consts.ROOM_PASSAGE_WIDTH : x < Consts.ROOM_PASSAGE_WIDTH;
                    if (!open)
                        result[x, y] = block;
                }
            }

            y0 += lengths[band];
        }

        return result;
    }

    static int[] Split(int length, int parts)
    {
        int[] lengths = new int[parts];
        int each = length / parts;
        int remainder = length % parts;
        for (int i = 0; i < parts; i++)
            lengths[i] = each + (i < remainder ? 1 : 0);
        return lengths;
    }
}
