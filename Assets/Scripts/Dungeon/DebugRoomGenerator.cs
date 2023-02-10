using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DebugRoomGenerator : MonoBehaviour
{
    [SerializeField] RoomBlockGenerator generator;
    [SerializeField, Range(0, 1)] float fillScale = 0.95f;

    DungeonBlock block;


    private void Update()
    {
        if (Input.anyKeyDown)
        {
            block = generator.Generate();
        }

    }

    static Color ValueToColor(int value)
    {
        switch (value)
        {
            case 1:
                return Color.cyan;
            case 2:
                return Color.magenta;
            default:
                return Color.gray;
        }
    }

    private void OnDrawGizmos()
    {
        if (block == null) return;

        var positions = block.FilledDungeonPositions().ToArray();
        Vector3 origin = transform.position;
        Vector3 shape = new Vector3(1, 1, 0) * fillScale;
        Vector3 scaleOffset = (new Vector3(1, 1, 0) - shape) * 0.5f;

        for (int i = 0; i< positions.Length; i++)
        {
            var position = positions[i];

            Vector3 pos = new Vector3(origin.x + position.Key.x, origin.y + position.Key.y, origin.z) + scaleOffset;

            Gizmos.color = ValueToColor(position.Value);

            Gizmos.DrawCube(pos, shape);
        }
    }
}
