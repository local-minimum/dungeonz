using System.Collections.Generic;
using System.Linq;
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
    Vector2Int minBaseShape = new Vector2Int(3, 3);

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

        for (int y = offset.y, yMax = offset.y + shape.y; y < yMax; y++)
        {
            for (int x = offset.x, xMax = offset.x + shape.x; x < xMax; x++)
            {
                data[y, x] = value;
            }
        }

        return true;
    }

    private bool IsDoorAnchorLocation(int[,] data, Vector2Int position, Vector2Int dataShape) {
        if (position.x == 0 || position.x >= dataShape.x - 1 || position.y == 0 || position.y >= dataShape.y - 1) return false;

        int[] rowHits = new int[3];
        int[] colHits = new int[3];

        for (int y = position.y - 1, maxY = position.y + 1, yi=0; y <= maxY; y++, yi++)
        {
            for (int x = position.x - 1, maxX = position.x + 1, xi=0; x <= maxX; x++, xi++)
            {
                if (data[y, x] == (int) BlockTileTypes.Room)
                {
                    rowHits[yi] += 1;
                    colHits[xi] += 1;
                }
            }
        }

        int nFullRows = rowHits.Count(v => v == 3);
        int nEmptyRows = rowHits.Count(v => v == 0);
        int nFullCols = colHits.Count(v => v == 3);
        int nEmptyCols = colHits.Count(v => v == 0);

        // Debug.Log($"Rows: {nFullRows} / {nEmptyRows}, Cols: {nFullCols} / {nEmptyCols}");

        if (nFullRows == 2 && nEmptyRows == 1)
        {
            return true;
        }
        else if (nFullCols == 2 && nEmptyCols == 1)
        {
            return true;
        }
        return false;
    }
    
    private List<Vector2Int> PossibleDoorAnchors(int[,] data)
    {
        var dataShape = data.Shape();
        return data
            .GetAll((value) => value == (int)BlockTileTypes.Room)
            .Where(coords => IsDoorAnchorLocation(data, coords, dataShape))
            .ToList();
    }

    private Vector2Int DoorFromAnchor(int[,] data, Vector2Int anchor) =>    
        FourDirections
            .AsDirections()
            .Select(offset =>
            {
                var coords = anchor + offset;
                var value = data[coords.y, coords.x];
                return new KeyValuePair<Vector2Int, int>(coords, value);
            })
            .First(kvp => kvp.Value == (int)BlockTileTypes.Nothing)
            .Key;            
    

    public DungeonBlock Generate()
    {
        var data = new int[blockShape.y, blockShape.x];
        var nShapes = numberOfBaseShapePossibilities[Random.Range(0, numberOfBaseShapePossibilities.Length)];
        var nDoors = doorsPossibilities[Random.Range(0, doorsPossibilities.Length)];

        int actualShapes = 0;

        for (int i = 0; i<nShapes; i++)
        {
            actualShapes += RoomFill(data, RandomShape, (int) BlockTileTypes.Room, i > 0) ? 1 : 0;
        }

        var doors = new List<Vector2Int>();
        var doorCandidates = PossibleDoorAnchors(data);

        for (int i = 0; i<nDoors; i++)
        {
            var doorAnchor = doorCandidates[Random.Range(0, doorCandidates.Count)];
            var door = DoorFromAnchor(data, doorAnchor);

            doorCandidates = doorCandidates.Where(c => (c - door).ManhattanDistance() > 2).ToList();

            data[door.y, door.x] = (int)BlockTileTypes.Door;
            doors.Add(door);

            if (doorCandidates.Count == 0)
            {
                i = nDoors;
            }
        }

        return new DungeonBlock(data);
    }
}
