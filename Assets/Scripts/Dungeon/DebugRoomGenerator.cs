using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DebugRoomGenerator : MonoBehaviour
{
    [SerializeField] RoomBlockGenerator generator;
    [SerializeField, Range(0, 1)] float fillScale = 0.95f;
    [SerializeField, Range(0, 1)] float rotationTickDuration = 1;

    DungeonBlock block;

    float lastRotation = -1;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            block = generator.Generate();
        } else if (block != null && (Time.timeSinceLevelLoad - lastRotation) > rotationTickDuration)
        {
            var scroll = -Input.mouseScrollDelta.x;
            float threshold = 0.1f;

            if (scroll < -threshold)
            {
                block.Orientation = block.Orientation.RotateCW();
                lastRotation = Time.timeSinceLevelLoad;
            } else if (scroll > threshold)
            {
                block.Orientation = block.Orientation.RotateCCW();                
                lastRotation = Time.timeSinceLevelLoad;
            }
        }

    }

    static Color ValueToColor(BlockTileTypes value)
    {
        switch (value)
        {
            case BlockTileTypes.Room:
                return Color.cyan;
            case BlockTileTypes.Door:
                return Color.magenta;
            default:
                return Color.gray;
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
        Gizmos.DrawCube(origin, new Vector3(blockShape.x, blockShape.y));

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
