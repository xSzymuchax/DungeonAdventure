using UnityEngine;

public class LargerMapFloorModifier : IFloorModifier
{
    public void Apply(DungeonRequest request)
    {
        request.Width = Grow(request.Width);
        request.Height = Grow(request.Height);
        request.RoomCount = Grow(request.RoomCount);
    }

    static int Grow(int value)
    {
        return Mathf.RoundToInt(value * 1.5f);
    }
}
