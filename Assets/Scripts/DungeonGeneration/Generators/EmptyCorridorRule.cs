using System.Collections.Generic;

public class EmptyCorridorRule : ICorridorRule
{
    public void Connect(FloorFieldType[,] fields, IReadOnlyList<Room> rooms)
    {
    }
}
