using System.Collections.Generic;

public class BorderModifier : IRoomModifier
{
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int width = roomFields.GetLength(0);
        int height = roomFields.GetLength(1);
        int newWidth = width + 2;
        int newHeight = height + 2;
        FloorFieldType[,] expanded = new FloorFieldType[newWidth, newHeight];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                expanded[x + 1, y + 1] = roomFields[x, y];

        bool[,] outside = Outside(expanded);
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                if (expanded[x, y] != FloorFieldType.EMPTY || !outside[x, y])
                    continue;
                if (TouchesRoom(expanded, x, y))
                    expanded[x, y] = FloorFieldType.SIDE_FIELD;
            }
        }

        return expanded;
    }

    static bool[,] Outside(FloorFieldType[,] fields)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        bool[,] outside = new bool[width, height];
        Queue<Position2D> queue = new Queue<Position2D>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fields[x, y] != FloorFieldType.EMPTY)
                    continue;
                if (x != 0 && y != 0 && x != width - 1 && y != height - 1)
                    continue;

                outside[x, y] = true;
                queue.Enqueue(new Position2D { x = x, y = y });
            }
        }

        while (queue.Count > 0)
        {
            Position2D current = queue.Dequeue();
            TryEnqueue(fields, outside, queue, current.x - 1, current.y);
            TryEnqueue(fields, outside, queue, current.x + 1, current.y);
            TryEnqueue(fields, outside, queue, current.x, current.y - 1);
            TryEnqueue(fields, outside, queue, current.x, current.y + 1);
        }

        return outside;
    }

    static void TryEnqueue(FloorFieldType[,] fields, bool[,] outside, Queue<Position2D> queue, int x, int y)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        if (x < 0 || y < 0 || x >= width || y >= height)
            return;
        if (outside[x, y] || fields[x, y] != FloorFieldType.EMPTY)
            return;

        outside[x, y] = true;
        queue.Enqueue(new Position2D { x = x, y = y });
    }

    static bool TouchesRoom(FloorFieldType[,] fields, int x, int y)
    {
        int width = fields.GetLength(0);
        int height = fields.GetLength(1);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;
                if (fields[nx, ny] != FloorFieldType.EMPTY)
                    return true;
            }
        }

        return false;
    }
}
