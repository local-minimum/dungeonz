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

    bool GetDigTarget(Vector2Int source, List<Vector2Int> exits, List<Vector2Int> halls, int[,] data, out Vector2Int target)
    {
        if (exits.Count > 0)
        {
            target = exits[Random.Range(0, exits.Count)];
            return true;
        } else if (halls.Count > 0)
        {
            target = halls[Random.Range(0, halls.Count)];
        } else
        {
            var options = data.GetAll(v => v == 0).Where(c => (c - source).ManhattanDistance() > closestExitProximity).ToList();
            if (options.Count == 0) throw new System.ArgumentException("There are no halls nor any exits");

            target = options[Random.Range(0, options.Count)];
        }
        
        return false;
    }

    List<Vector2Int> Connect(
        int[,] data,
        Vector2Int source,
        Vector2Int target,
        List<Vector2Int> otherExits,
        List<Vector2Int> diggable,
        out List<Vector2Int> halls,
        out bool success
    )
    {
        halls = new List<Vector2Int>();
        var position = source;
        while (true)
        {
            if ((position - target).IsUnit())
            {
                // We are next to target, we've dug out what we need to dig!
                success = true;
                return diggable;
            } else if (otherExits.Any(c => (position - c).IsUnit()))
            {
                // We are next to another exit, lets stop
                var accidentalTarget = otherExits.First(c => (position - c).IsUnit());

                Debug.Log($"Wanted to connect {source} with {target} but stumbled upon {accidentalTarget} from {position}");

                diggable = diggable.Where(c => !(accidentalTarget - c).IsUnit()).ToList();
                success = false;
                return diggable;
            }

            var candidates = diggable.Where(c => (c - position).IsUnit()).ToList();

            if (candidates.Count == 0)
            {
                // We have nowhere to go
                Debug.Log($"Failed to connect {source} with {target}, would need to backtrack");
                success = false;
                return diggable;
            }

            var next = candidates[Random.Range(0, candidates.Count)];
            if (data[position.y, position.x] == (int)BlockTileTypes.Exit)
            {
                // Current position is an exit, remove all remaining diggables around it
                // needed to clear options around the source
                diggable = diggable.Where(c => !(c - position).IsUnit()).ToList();
            }
            else
            {
                // Remove next position since we'll dig it out now
                diggable = diggable.Where(c => c != next).ToList();
            }            

            if (next == target)
            {
                // If we are attempting to dig out our target next (we should never need to)

                if (data[target.y, target.x] == (int)BlockTileTypes.Exit)
                {
                    // This shouldn't happen but lets clear out options around the target if
                    // it was an exit
                    diggable = diggable.Where(c => !(c - position).IsUnit()).ToList();
                }
                success = true;
                return diggable;
            } else
            {
                // Actually dig
                data[next.y, next.x] = (int)BlockTileTypes.Hall;
                halls.Add(next);
            }

            if (diggable.Count == 0)
            {
                // We have no more positions in block to dig at all!
                Debug.LogWarning($"Failed to connect {source} with {target}, everything is dugout!");
                success = false;
                return diggable;
            }

            position = next;
        }
    }

    void DigHalls(int[,] data, List<Vector2Int> exits)
    {
        var allExits = new List<Vector2Int>();
        allExits.AddRange(exits);

        List<Vector2Int> halls = new List<Vector2Int>();

        var diggable = data
            .GetAll(v => v == 0)
            .Where(c => !exits.Contains(c))
            .ToList();


        while (exits.Count > 0)
        {
            var digSource = exits[Random.Range(0, exits.Count)];
            exits = exits.Where(e => e != digSource).ToList();

            Vector2Int digTarget;
            if (GetDigTarget(digSource, exits, halls, data, out digTarget))
            {
                exits = exits.Where(e => e != digTarget).ToList();
            }

            List<Vector2Int> newHalls;
            diggable = Connect(
                data,
                digSource, 
                digTarget, 
                allExits.Where(e => e != digSource && e != digTarget).ToList(),
                diggable,
                out newHalls, 
                out bool success
            );

            halls.AddRange(newHalls);

            if (diggable.Count == 0)
            {
                Debug.Log($"Abandonging digging with {exits.Count} exits left to connect");
                break;
            }

            if (!success && data[digTarget.y, digTarget.x] == (int)BlockTileTypes.Exit)
            {
                exits.Add(digTarget);
            }
        }
        
    }

    public DungeonBlock Generate()
    {
        var data = new int[blockShape.y, blockShape.x];

        var exits = AddExits(data);
        DigHalls(data, exits);

        return new DungeonBlock(data);
    }
}
