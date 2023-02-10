using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MathHelpers
{
    /// <summary>
    /// A mod operator that works like it should
    /// </summary>
    /// <param name="x"></param>
    /// <param name="m"></param>
    /// <returns>Modulus value</returns>
    public static int Mod(this int x, int m)
    {
        return (x % m + m) % m;
    }

    #region Vector2Int

    /// <summary>
    /// Random anchor for inset such that inset fits inside container
    /// </summary>
    /// <param name="container">Size of container grid</param>
    /// <param name="inset">Size of area to place inside container</param>
    /// <returns></returns>
    public static Vector2Int RandomInsetAnchor(this Vector2Int container, Vector2Int inset)
    {
        if (container.x < inset.x || container.y < inset.y)
        {
            throw new System.ArgumentException($"Inset {inset} does not fit inside container {container}");
        }

        return new Vector2Int(Random.Range(0, container.x - inset.x), Random.Range(0, container.y - inset.y));
    }

    /// <summary>
    /// Ensures that inset with origin at anchor fits inside container, else shifts anchor
    /// </summary>
    /// <param name="anchor">Where inset originates</param>
    /// <param name="container">Size of container grid</param>
    /// <param name="inset">Size of area to place inside container</param>
    /// <returns>Potentially updated anchor</returns>
    public static Vector2Int ClampInside(this Vector2Int anchor, Vector2Int container, Vector2Int inset)
    {
        var x = anchor.x;
        if (x + inset.x >= container.x)
        {
            x -= (x + inset.x) - (container.x - 1);
            if (x < 0)
            {
                throw new System.ArgumentException($"Anchor {anchor} with inset size {inset} does not fit inside {container} (x-dim)");
            }
        }

        var y = anchor.y;
        if (y + inset.y >= container.y)
        {
            y -= (y + inset.y) - (container.y - 1);
            if (y < 0)
            {
                throw new System.ArgumentException($"Anchor {anchor} with inset size {inset} does not fit inside {container} (y-dim)");
            }

        }

        return new Vector2Int(x, y);
    }

    #endregion

    #region int[,]

    /// <summary>
    /// Produce a vector of the shape of the data
    /// </summary>
    /// <param name="data">Data to measure</param>
    /// <returns>Its shape</returns>
    public static Vector2Int Shape(this int[,] data)
    {
        var height = data.GetLength(0);
        var width = data.GetLength(1);

        return new Vector2Int(width, height);
    }

    private static System.Func<int, bool> NotZero = (int value) => value != 0;

    /// <summary>
    /// Count number of positions that match predicate
    /// </summary>
    /// <param name="data">Data to search</param>
    /// <param name="predicate">Predicate to match, defaults to non-zero value</param>
    /// <returns></returns>
    public static int CountMatches(
        this int[,] data,
        System.Func<int, bool> predicate = null
    )
    {
        if (predicate == null)
        {
            predicate = NotZero;
        }

        int count = 0;
        for (int y = 0, maxY = data.GetLength(0); y<maxY; y++)
        {
            for (int x = 0, maxX = data.GetLength(1); x<maxX; x++)
            {
                if (predicate(data[y, x])) count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Evaluates if rect area of data has any position that matches predicate
    /// </summary>
    /// <param name="data">Data to search</param>
    /// <param name="rect">Area of data to evaluate</param>
    /// <param name="predicate">Predicate to match, defaults to non-zero value</param>
    /// <returns></returns>
    public static bool HasMatch(
        this int[,] data, 
        RectInt rect, 
        System.Func<int, bool> predicate = null
    )
    {
        if (predicate == null)
        {
            predicate = NotZero;
        }

        // TODO: This one is suspicious... does it not work?
        Debug.LogWarning("There might be something wrong with this");
        for (int y = rect.yMin; y < rect.yMax; y++)
        {
            for (int x = rect.xMax; x < rect.xMax; x++)
            {
                if (data[y, x] != 0) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns nth coordinates matching predicate
    /// </summary>
    /// <param name="data">Data to search (rows, then columns)</param>
    /// <param name="position">Nth match to report</param>
    /// <param name="predicate">Predicate to match, defaults to non-zero</param>
    /// <returns></returns>
    public static Vector2Int GetNth(
        this int[,] data,
        int position, 
        System.Func<int, bool> predicate = null
    )
    {
        if (predicate == null)
        {
            predicate = NotZero;
        }

        for (int y = 0, maxY = data.GetLength(0); y < maxY; y++)
        {
            for (int x = 0, maxX = data.GetLength(1); x < maxX; x++)
            {
                if (predicate(data[y, x]))
                {
                    if (position == 0)
                    {
                        return new Vector2Int(x, y);
                    } else
                    {
                        position--;
                    }
                }
            }
        }

        throw new System.ArgumentException("Did not find such position");
    }

    #endregion
}
