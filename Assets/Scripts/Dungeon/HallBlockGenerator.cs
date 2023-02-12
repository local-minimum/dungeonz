using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class HallBlockGenerator : IBlockGenerator
{
    [SerializeField]
    Vector2Int blockShape = new Vector2Int(9, 9);

    [SerializeField]
    private int[] exitOptions = new int[] { 1, 2, 2, 3, 3, 3, 3, 3, 4 };

    [SerializeField]
    private int exitRings = 2;

    [SerializeField, Range(2, 10)]
    private int closestExitProximity = 7;

    public List<Vector2Int> AddExits(int[,] data)
    {
        int nExits = exitOptions[Random.Range(0, exitOptions.Length)];
        var exitCandidates = blockShape
            .GetOuterRingCoordinates(exitRings)
            .ToList();
        var exits = new List<Vector2Int>();

        for (int i = 0; i < nExits; i++)
        {
            var exit = exitCandidates[Random.Range(0, exitCandidates.Count)];

            exitCandidates = exitCandidates
                .Where(c => (c - exit).ManhattanDistance() > closestExitProximity)
                .ToList();

            exits.Add(exit);
            data[exit.y, exit.x] = (int)BlockTileTypes.Exit;

            if (exitCandidates.Count == 0)
            {
                i = nExits;
            }
        }
        
        return exits;
    }

    public DungeonBlock Generate()
    {
        var data = new int[blockShape.y, blockShape.x];

        var exits = AddExits(data);

        return new DungeonBlock(data);
    }
}
