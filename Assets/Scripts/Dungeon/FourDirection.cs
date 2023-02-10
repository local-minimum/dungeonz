using System.Collections.Generic;
using UnityEngine;

public enum FourDirection { North, East, South, West };

public static class FourDirectionExtensions {


    private static readonly List<FourDirection> directions = new List<FourDirection>
    {
        FourDirection.North,
        FourDirection.East,
        FourDirection.South,
        FourDirection.West,
    };

    public static Vector2Int RotateVector(this FourDirection direction, Vector2Int vector)
    {
        switch (direction)
        {
            case FourDirection.North:
                return vector;
            case FourDirection.South:
                return new Vector2Int(-vector.x, -vector.y);
            case FourDirection.East:
                return new Vector2Int(vector.y, -vector.x);
            case FourDirection.West:
                return new Vector2Int(-vector.y, vector.x);
            default:
                throw new System.ArgumentException($"{direction} is not a real direction");
        }
    }

    /// <summary>
    /// Rotate a direction clock-wise
    /// </summary>
    /// <param name="direction">The start direction</param>
    /// <param name="steps">Number of 90 degrees steps</param>
    /// <returns>The new direction</returns>
    public static FourDirection RotateCW(this FourDirection direction, int steps = 1)
    {
        int pos = directions.IndexOf(direction) + steps;
        return directions[pos.Mod(steps)];
    }

    /// <summary>
    /// Rotate a direction counter clock-wise
    /// </summary>
    /// <param name="direction">The start direction</param>
    /// <param name="steps">Number of 90 degrees steps</param>
    /// <returns>The new direction</returns>
    public static FourDirection RotateCCW(this FourDirection direction, int steps = 1) => direction.RotateCW(-steps);    
}
