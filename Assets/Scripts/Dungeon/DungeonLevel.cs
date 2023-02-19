using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class DungeonLevel
{
    [SerializeField]
    private List<DungeonBlock> blocks = new List<DungeonBlock>();

    [SerializeField]
    private List<BlockConnector> connectors = new List<BlockConnector>();
        
    private DungeonBlock tentativeBlock;

    public DungeonBlock TentativeBlock { get { return tentativeBlock; } }

    public bool HasTentative
    {
        get { return TentativeBlock != null; }
    }

    private DungeonOvelap GetOverlap(Vector2Int coords)
    {

        var con = connectors.Find(con => con.Coordinates == coords);

        if (con == null)
        {
            return DungeonOvelap.Nothing;
        }
        return con.GetOverlap(coords);
    }
    
    public bool MayPlaceTentativeBlock
    {
        get
        {

            if (TentativeBlock == null)
            {
                Debug.LogWarning("There's no tentative block to place");
                return false;
            }

            var tentativeConnectors = TentativeBlock
                .DungeonPositions(BlockConnector.IsConnector)
                .Select(kvp => kvp.Key)
                .ToArray();


            if (blocks.Count() == 0)
            {
                return true;
            }

            var tentativeWithOverlapStatus = tentativeConnectors
                .Select(coords => new KeyValuePair<Vector2Int, DungeonOvelap>(
                    coords, 
                    GetOverlap(coords)
                ))
                .ToArray();

            if (
                tentativeWithOverlapStatus.Count(o => o.Value == DungeonOvelap.Connection) == 0
                || tentativeWithOverlapStatus.Any(o => o.Value == DungeonOvelap.Collision)                 
            )
            {
                return false;
            }
                   
            return !blocks.Any(block => {
                if (block.CollidesWith(TentativeBlock, out var positions)) {
                    // If some collision is of bad type
                    return positions.Any(coord => 
                        // The colliding block's coordinates may only be of connector type
                        BlockConnector.IsNotConnector(block.GetValue(coord))
                        // The tentative block's coordinates may also only be of connector type
                        || !tentativeConnectors.Any(con => con == coord));
                };
                return false;
            });
        }
    }

    public void PlaceBlock()
    {
        if (!MayPlaceTentativeBlock) return;

        blocks.Add(TentativeBlock);

        // List all connections need handling
        var connections = TentativeBlock.DungeonPositions(BlockConnector.IsConnector).Select(kvp => kvp.Key).ToArray();

        // Complete connectors where needed
        var fillers = connectors.Where(con => connections.Any(coord => coord == con.Coordinates)).ToArray();
        for (int i = 0; i<fillers.Length; i++)
        {
            fillers[i].Connect(TentativeBlock);
        }

        // Create new open connectors where needed
        connectors.AddRange(
            connections
                .Where(coord => !fillers.Any(conn => conn.Coordinates == coord))
                .Select(coord => new BlockConnector(TentativeBlock, coord))
        );
        tentativeBlock = null;
    }

    public void ClearTentativeBlock()
    {
        tentativeBlock = null;
    }

    public void SetTentativeBlock(DungeonBlock block)
    {
        tentativeBlock = block;
    }

    public IEnumerable<DungeonBlock> Blocks { get { return blocks;  } }
}
