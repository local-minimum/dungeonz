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
    /// <returns></returns>
    public static int Mod(this int x, int m)
    {
        return (x % m + m) % m;
    }

    #region Vector2Int

    public static Vector2Int RandomShapeOffset(this Vector2Int container, Vector2Int inset)
    {
        if (container.x < inset.x || container.y < inset.y)
        {
            throw new System.ArgumentException($"Inset {inset} does not fit inside container {container}");
        }

        return new Vector2Int(Random.Range(0, container.x - inset.x), Random.Range(0, container.y - inset.y));
    }

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

    public static int CountNonZero(this int[,] data)
    {
        int count = 0;
        for (int y = 0, maxY = data.GetLength(0); y<maxY; y++)
        {
            for (int x = 0, maxX = data.GetLength(1); x<maxX; x++)
            {
                if (data[y, x] != 0) count++;
            }
        }
        return count;
    }

    // TODO: This one is suspicious... does it not work?
    public static bool HasNonZero(this int[,] data, RectInt rect)
    {
        for (int y = rect.yMin; y < rect.yMax; y++)
        {
            for (int x = rect.xMax; x < rect.xMax; x++)
            {
                if (data[y, x] != 0) return true;
            }
        }
        return false;
    }

    public static Vector2Int GetNthFilled(this int[,] data, int position)
    {
        for (int y = 0, maxY = data.GetLength(0); y < maxY; y++)
        {
            for (int x = 0, maxX = data.GetLength(1); x < maxX; x++)
            {
                if (data[y, x] != 0)
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
