using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BlockType { Room = 1, Hall = 2 };

[RequireComponent(typeof(DungeonDesignerController))]
public class DebugGenerator : MonoBehaviour
{
    [SerializeField] RoomBlockGenerator roomGenerator;
    [SerializeField] HallBlockGenerator hallGenerator;

    [SerializeField] BlockType blockType = BlockType.Hall;

    [SerializeField, Range(0, 1)] float fillScale = 0.95f;

    DungeonBlock block;
  
    private IBlockGenerator generator {
        get {  
        
            switch (blockType)
            {
                case BlockType.Room:
                    return roomGenerator;
                case BlockType.Hall:
                    return hallGenerator;
                default:
                    throw new System.ArgumentException($"Unknown block type {blockType}");
            }
        }
    }

    private void OnEnable()
    {
        GetComponent<DungeonDesignerController>().OnAction += HandleDesignEvents;
    }

    private void OnDisable()
    {
        GetComponent<DungeonDesignerController>().OnAction -= HandleDesignEvents;
    }

    private void HandleDesignEvents(DungeonDesignEventType eventType)
    {
        switch (eventType)
        {
            case DungeonDesignEventType.SpawnTile:
                block = generator.Generate();
                return;
            case DungeonDesignEventType.RotateCW:
                if (block != null)
                {
                    block.Orientation = block.Orientation.RotateCW();
                }
                return;
            case DungeonDesignEventType.RoteateCCW:
                if (block != null)
                {
                    block.Orientation = block.Orientation.RotateCCW();
                }
                return;

            case DungeonDesignEventType.MoveNorth:
                if (block != null)
                {
                    block.Anchor += Vector2Int.up;
                }
                return;
            case DungeonDesignEventType.MoveSouth:
                if (block != null)
                {
                    block.Anchor += Vector2Int.down;
                }
                return;
            case DungeonDesignEventType.MoveWest:
                if (block != null)
                {
                    block.Anchor += Vector2Int.left;
                }
                return;
            case DungeonDesignEventType.MoveEast:
                if (block != null)
                {
                    block.Anchor += Vector2Int.right;
                }
                return;
        }
    }

    [SerializeField]
    Color32 colorHall;
    [SerializeField]
    Color32 colorRoom;
    [SerializeField]
    Color32 colorDoor;
    [SerializeField]
    Color32 colorHallExit;
    [SerializeField]
    Color32 tileBackground;

    Color ValueToColor(BlockTileTypes value)
    {
        switch (value)
        {
            case BlockTileTypes.Hall:
                return colorHall;
            case BlockTileTypes.Room:
                return colorRoom;
            case BlockTileTypes.Exit:
                return colorHallExit;
            case BlockTileTypes.Door:
                return colorDoor;
            case BlockTileTypes.FalseHall:
                return Color.yellow;
            default:
                return tileBackground;
        }
    }

    private void OnDrawGizmos()
    {
        if (block == null) return;

        // Constants
        Vector3 origin = transform.position;
        Vector3 shape = new Vector3(1, 1, 0) * fillScale;
        Vector3 scaleOffset = (new Vector3(1, 1, 0) - shape) * 0.5f;

        // Outline shape of block
        Gizmos.color = ValueToColor(BlockTileTypes.Nothing);        
        var blockShape = block.DungeonShape;
        Gizmos.DrawCube(new Vector3(origin.x + block.Anchor.x, origin.y + block.Anchor.y, origin.z), new Vector3(blockShape.x, blockShape.y));

        // Draw features
        var positions = block.FilledDungeonPositions().ToArray();
        for (int i = 0; i< positions.Length; i++)
        {
            var position = positions[i];

            Vector3 pos = new Vector3(origin.x + position.Key.x, origin.y + position.Key.y, origin.z) + scaleOffset;

            Gizmos.color = ValueToColor((BlockTileTypes) position.Value);
            Gizmos.DrawCube(pos, shape);
        }
    }
}
