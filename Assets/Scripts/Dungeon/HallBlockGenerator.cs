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

    [SerializeField, Range(0, 1)]
    float digStraightProbability = 0.7f;

    [SerializeField, Range(0, 1)]
    float digElseTurnTowardsProbability = 0.9f;

    [SerializeField, Range(0, 1)]
    float digInitialDirectionProbability = 0.8f;

    Vector2Int SelectNextDigDirection(
        Vector2Int position, 
        Vector2Int target,
        Vector2Int direction, 
        List<Vector2Int> candidates,         
        out Vector2Int next
    )
    {
        if (direction == Vector2Int.zero)
        {
            next = candidates
                .OrderBy(c => (target - c).ManhattanDistance())
                .Where((_, idx) => idx == candidates.Count - 1 || Random.value < digInitialDirectionProbability)
                .First(); ;
            return (next - position);
        }

        var straight = candidates.Where(c => (c - position) == direction).ToArray();
        if (straight.Length > 0 && Random.value < digStraightProbability)
        {
            next = straight[0];
            return direction;
        }

        var turns = candidates.OrderBy(c => (target - c).ManhattanDistance()).ToArray();
        if (turns.Length > 1 && Random.value > digElseTurnTowardsProbability)
        {
            next = turns[1];
            return (next - position);
        }

        next = turns[0];
        return (next - position);
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
        var direction = Vector2Int.zero;

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

                diggable = diggable.Where(c => (accidentalTarget - c).ChebyshevDistance() > 1).ToList();
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

            direction = SelectNextDigDirection(position, target, direction, candidates, out Vector2Int next);
            if (data[position.y, position.x] != (int)BlockTileTypes.Exit)
            {
                // Remove next position since we'll dig it out now
                diggable = diggable.Where(c => c != next).ToList();
            }            
            if (position == source)
            {
                diggable = diggable.Where(c => (c - source).ManhattanDistance() > 1).ToList();
            }

            if (next == target)
            {
                // If we are attempting to dig out our target next (we should never need to)
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
            
            bool targetIsExit = GetDigTarget(digSource, exits, halls, data, out Vector2Int digTarget);
            if (targetIsExit)
            {
                exits = exits.Where(e => e != digTarget).ToList();
            }

            Debug.Log($"Connect {digSource} with {digTarget} (exit:{targetIsExit})");

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
            diggable = diggable
                .Where(c => (c - digSource).ChebyshevDistance() > 1 && (!targetIsExit || !success || (c - digTarget).ChebyshevDistance() > 1))
                .ToList();

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

    private struct Neighbourhood
    {
        public Vector2Int Position;
        public int Neighbours;
        public bool NextToExit;

        public Neighbourhood(Vector2Int position, int neighbours, bool nextToExit)
        {
            Position = position;
            Neighbours = neighbours;
            NextToExit = nextToExit;
        }

        public bool IsSame(Neighbourhood other) => other.Position == Position;

        public int Discount(Neighbourhood other)
        {
            if ((other.Position - Position).ChebyshevDistance() == 1)
            {
                Neighbours--;
            }
            return Neighbours;
        }
    }
    
    int CountSeparateHalls(IEnumerable<Vector2Int> halls)
    {
        int count = 0;

        List<Vector2Int> unTouchedHalls = new List<Vector2Int>();
        List<Vector2Int> sources = new List<Vector2Int>();

        unTouchedHalls.AddRange(halls);

        if (unTouchedHalls.Count == 0) return 0;

        sources.Add(unTouchedHalls[0]);
        unTouchedHalls.Remove(sources[0]);

        while(unTouchedHalls.Count > 0)
        {
            while (sources.Count > 0)
            {
                var source = sources[0];
                sources.RemoveAt(0);

                var neighbours = unTouchedHalls.Where(h => (h - source).ManhattanDistance() == 1).ToList();

                unTouchedHalls.RemoveAll(h => neighbours.Contains(h));
                sources.AddRange(neighbours);
            }

            if (unTouchedHalls.Count == 0) break;

            sources.Add(unTouchedHalls[0]);
            unTouchedHalls.Remove(sources[0]);

            count++;
        }

        return count;
    }

    [SerializeField, Range(3, 5)]
    int erosionThreshold = 3;

    void ErodeHalls(int[,] data)
    {
        var halls = data.GetAll(v => v == (int)BlockTileTypes.Hall).ToList();
        int nSeparateHalls = CountSeparateHalls(halls);

        var candidates = halls
            .Select(current =>
        {
            int neighbours = halls.Count(c => (c - current).ChebyshevDistance() == 1);
            bool nextToExit = halls.Any(c => data[c.y, c.x] == (int)BlockTileTypes.Exit);
            return new Neighbourhood(current, neighbours, nextToExit);
        })
            .Where(n => n.Neighbours > erosionThreshold && !n.NextToExit)
            .OrderBy(n => n.Neighbours)
            .ToList();

        while (candidates.Count > 0)
        {
            var maxNeighbours = candidates.Max(c => c.Neighbours);
            var options = candidates.Where(c => c.Neighbours == maxNeighbours).ToList();
            var candidate = options[Random.Range(0, options.Count)];

            if (nSeparateHalls == CountSeparateHalls(halls.Where(h => h != candidate.Position))) {
                // data[candidate.Position.y, candidate.Position.x] = (int)BlockTileTypes.FalseHall;
                data[candidate.Position.y, candidate.Position.x] = (int)BlockTileTypes.Nothing;
                halls.Remove(candidate.Position);
            }

            candidates = candidates
                .Where(c => !c.IsSame(candidate))
                .Where(c => c.Discount(candidate) > erosionThreshold)
                .ToList();
        }
    }

    public DungeonBlock Generate()
    {
        var data = new int[blockShape.y, blockShape.x];

        var exits = AddExits(data);
        DigHalls(data, exits);
        ErodeHalls(data);

        return new DungeonBlock(data);
    }
}
