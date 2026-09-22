using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleDoorMarkerModifier : IRoomModifier
{
    private int maxMarkerCount;
    public PossibleDoorMarkerModifier(int maxMarkerCount) { this.maxMarkerCount = maxMarkerCount; }
    public FloorFieldType[,] Apply(FloorFieldType[,] roomFields)
    {
        int width = roomFields.GetLength(0);
        int heigth = roomFields.GetLength(1);
        
        List<Position2D> neighours = new()
        {
            new() { x=1,y=0},
            new() { x=-1,y=0},
            new() { x=0,y=1},
            new() { x=0,y=-1}
        };

        List<Position2D> foundPositions = new();

        for (int i=0;i<width;i++)
            for (int j = 0; j < heigth; j++)
            {
                if (roomFields[i, j] != FloorFieldType.SIDE_FIELD)
                    continue;

                int notEmpties = 0;
                foreach (Position2D n in neighours)
                {
                    int x = i + n.x;
                    int y = j + n.y;

                    if (x < 0 || y < 0 || x >= width || y >= heigth)
                        continue;

                    if (roomFields[x, y] != FloorFieldType.EMPTY)
                        notEmpties++;
                }

                if (notEmpties == 3)
                    foundPositions.Add(new() { x = i, y = j }); 
            }

        int amountOfPossibleDoorPoints = Random.Range(1, maxMarkerCount + 1);
        List<Position2D> placedDoors = new();

        while (placedDoors.Count < amountOfPossibleDoorPoints && foundPositions.Count > 0)
        {
            int index = Random.Range(0, foundPositions.Count);
            Position2D candidate = foundPositions[index];
            foundPositions.RemoveAt(index);

            if (TouchesPlacedDoor(candidate, placedDoors))
                continue;

            roomFields[candidate.x, candidate.y] = FloorFieldType.POSSIBLE_DOOR_FIELD;
            placedDoors.Add(candidate);
        }

        return roomFields;
    }

    private static bool TouchesPlacedDoor(Position2D candidate, List<Position2D> placedDoors)
    {
        foreach (Position2D door in placedDoors)
        {
            int dx = Mathf.Abs(candidate.x - door.x);
            int dy = Mathf.Abs(candidate.y - door.y);
            if (dx <= 1 && dy <= 1)
                return true;
        }

        return false;
    }
}
