using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class DungeonLevel
{
    [SerializeField]
    private List<DungeonBlock> blocks = new List<DungeonBlock>();
    
    private DungeonBlock tentativeBlock;

    public DungeonBlock TentativeBlock { get { return tentativeBlock; } }

    public bool HasTentative
    {
        get { return TentativeBlock != null; }
    }
    
    // Todo add block to block connectivity and occupancy of connections

    public bool MayPlaceTentativeBlock
    {
        get
        {
            // This is not the real rules!

            if (TentativeBlock == null) return false;

            if (blocks.Count() == 0) return true;

            return blocks.Any(block => {
                if (block.CollidesWith(TentativeBlock, out var positions)) {
                    return positions.Count() > 0;
                };
                return false;
            });
        }
    }

    public void PlaceBlock()
    {
        if (TentativeBlock == null) return;

        blocks.Add(TentativeBlock);
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
