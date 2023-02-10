using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomBlockGenerator
{
    [SerializeField]
    Vector2Int blockShape = new Vector2Int(7, 7);

    [SerializeField, Tooltip("Randomly selects an option, repeating an option makes it more probable")]
    private int[] doorsPossibilities = new int[] { 1, 1, 1, 1, 2, 2, 2, 3, 4 };

    [SerializeField, Tooltip("Randomly selects an option, repeating an option makes it more probable")]
    private int[] numberOfBaseShapePossibilities = new int[] { 1, 1, 2, 2, 3, 4, 5 };

    [SerializeField]
    Vector2Int minBaseShape = new Vector2Int(2, 2);

    [SerializeField]
    Vector2Int maxBaseShape = new Vector2Int(5, 5);

    Vector2Int RandomShape => new Vector2Int(
        Random.Range(minBaseShape.x, maxBaseShape.x),
        Random.Range(minBaseShape.y, maxBaseShape.y)
    );

    bool RoomFill(int[,] data, Vector2Int shape, int value, bool connectPrevious)
    {
        Vector2Int offset = blockShape.RandomInsetAnchor(shape);
        if (connectPrevious && !data.HasMatch(new RectInt(offset, shape)))
        {
            var fillCount = data.CountMatches();
            if (fillCount > 0)
            {
                var anchor = data.GetNth(Random.Range(0, fillCount));
                offset = anchor.ClampInside(blockShape, shape);
            } else
            {
                Debug.LogError("Cannot connect to previous if nothing is filled");
            }
        }
        
        for (int y = offset.y, yMax = offset.y + shape.y; y< yMax; y++)
        {
            for (int x = offset.x, xMax = offset.x + shape.x; x<xMax; x++)
            {
                data[y, x] = value;
            }
        }

        return true;
    }

    public DungeonBlock Generate()
    {
        var data = new int[blockShape.y, blockShape.x];
        var nShapes = numberOfBaseShapePossibilities[Random.Range(0, numberOfBaseShapePossibilities.Length)];
        var nDoors = doorsPossibilities[Random.Range(0, doorsPossibilities.Length)];

        int actualShapes = 0;

        for (int i = 0; i<nShapes; i++)
        {
            // TODO: Use something smarter than just numbers for floor, door types...
            actualShapes += RoomFill(data, RandomShape, 1, i > 0) ? 1 : 0;
        }

        // TODO: Add doors

        return new DungeonBlock(data);
    }
}
