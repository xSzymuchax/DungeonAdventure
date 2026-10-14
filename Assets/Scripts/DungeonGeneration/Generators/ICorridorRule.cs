using System.Collections.Generic;

public interface ICorridorRule
{
    void Connect(FloorFieldType[,] fields, IReadOnlyList<Room> rooms);
}
