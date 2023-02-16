using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DungeonBlock
{
    public readonly Vector2Int Shape;

    [Tooltip("Local block grid rotaion pivot point")]
    public readonly Vector2Int Pivot;

    [Tooltip("Dungeon grid offset")]
    public Vector2Int Anchor;

    [SerializeField]
    private int[,] _data;


    [Tooltip("North is 'normal' rotation. East is rotated 90 degrees clock-wise...")]
    public FourDirection Orientation;

    public DungeonBlock(int[,] data)
    {
        Shape = data.Shape();
        Pivot = new Vector2Int(Mathf.FloorToInt(Shape.x / 2f), Mathf.FloorToInt(Shape.y / 2f));
        Anchor = Vector2Int.zero;
        Orientation = FourDirection.North;
        _data = data;        
    }

    public override string ToString() => $"<Block {Shape} @ {Anchor} facing {Orientation} (pivot {Pivot})>";


    public void UpdateData(int[,] data)
    {        
        var shape = data.Shape();
        if (shape != Shape)
        {
            throw new System.ArgumentException(
                $"Data does not conform to expected shape ({shape} != {Shape})"
            );
        }
        _data = data;
    }

    public Vector2Int DungeonShape
    {
        get
        {
            switch (Orientation)
            {
                case FourDirection.North:
                case FourDirection.South:
                    return Shape;
                case FourDirection.West:
                case FourDirection.East:
                    var s = Shape;
                    return new Vector2Int(s.y, s.x);
                default:
                    throw new System.ArgumentException($"{Orientation} is not a known orientation");
            }
        }
    }

    /// <summary>
    /// Transpose a block coordinates pair into a dungeon coordinates pair
    /// </summary>
    /// <param name="coordinates">Transposed coordinates</param>
    /// <returns></returns>
    public Vector2Int BlockCoordinatesToDungeon(Vector2Int coordinates) => 
        Orientation.RotateVector(coordinates - Pivot) + Anchor;
    
    /// <summary>
    /// The extent given the current rotation in dungeon coordinates
    /// </summary>
    public RectInt DungeonExtent
    {
        get
        {
            Vector2Int nw = BlockCoordinatesToDungeon(Vector2Int.zero);
            Vector2Int se = BlockCoordinatesToDungeon(Shape - Vector2Int.one);
            return new RectInt(
                Mathf.Min(nw.x, se.x), 
                Mathf.Min(nw.y, se.y), 
                Mathf.Abs(nw.x - se.x) + 1, 
                Mathf.Abs(nw.y - se.y) + 1
            );
        }
    }

    /// <summary>
    /// If the base shapes of two blocks overlap (not neccesarily having colliding features)
    /// </summary>
    /// <param name="other">The other block</param>
    /// <returns>If overlapping</returns>
    public bool Intersects(DungeonBlock other)
    {
        Debug.Log(DungeonExtent);
        return DungeonExtent.Overlaps(other.DungeonExtent);
    }

    /// <summary>
    /// Get all filled positions as dungeon coordinates
    /// </summary>
    /// <returns>Enumerable of coordinates and their values</returns>
    public IEnumerable<KeyValuePair<Vector2Int, int>> FilledDungeonPositions()
    {
        for (int y = 0; y<Shape.y; y++)
        {
            for (int x = 0; x<Shape.x; x++)
            {
                int value = _data[y, x];
                if (value != (int)BlockTileTypes.Nothing)
                {
                    yield return new KeyValuePair<Vector2Int, int>(
                        BlockCoordinatesToDungeon(new Vector2Int(x, y)),
                        value
                    );
                }
            }
        }
    }

    /// <summary>
    /// If the base shape have any colliding features
    /// </summary>
    /// <param name="other">The other block</param>
    /// <param name="positions">Dungeon grid coordinates for collisions</param>
    /// <returns>If colliding</returns>
    public bool CollidesWith(DungeonBlock other, out IEnumerable<Vector2Int> positions)
    {
        if (!Intersects(other))
        {
            positions = new List<Vector2Int>();
            return false;
        }

        var tmp = new List<Vector2Int>();

        var myPositions = FilledDungeonPositions()
            .Select(kvp => kvp.Key)
            .ToArray();

        var collidingPositions = other
            .FilledDungeonPositions()
            .Where(kvp => myPositions.Contains(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToArray();

        positions = collidingPositions;
        return collidingPositions.Length > 0;
    }
}
