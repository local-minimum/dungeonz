using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FourDirection { North, East, South, West };

public static class FourDirections {
    private static readonly List<FourDirection> directions = new List<FourDirection>
    {
        FourDirection.North,
        FourDirection.East,
        FourDirection.South,
        FourDirection.West,
    };

    /// <summary>
    /// Convert direction to a unit vector
    /// </summary>
    /// <param name="direction">The direction</param>
    /// <returns>Unit int vector</returns>
    public static Vector2Int AsVector(this FourDirection direction)
    {
        switch (direction)
        {
            case FourDirection.North:
                return Vector2Int.up;
            case FourDirection.West:
                return Vector2Int.left;
            case FourDirection.South:
                return Vector2Int.down;
            case FourDirection.East:
                return Vector2Int.right;
            default:
                throw new System.ArgumentException($"{direction} is not a real direction");
        }
    }

    /// <summary>
    /// Get all cardinal directions
    /// </summary>
    /// <returns>Enumerable of unit vectors</returns>
    public static IEnumerable<Vector2Int> AsDirections() => directions.Select(d => d.AsVector());

    /// <summary>
    /// Rotate vector according to which direction is "up"
    /// </summary>
    /// <param name="upDirection">Which direction up is facing</param>
    /// <param name="vector">The vector when up faces north</param>
    /// <returns></returns>
    public static Vector2Int RotateVector(this FourDirection upDirection, Vector2Int vector)
    {
        switch (upDirection)
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
                throw new System.ArgumentException($"{upDirection} is not a real direction");
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
