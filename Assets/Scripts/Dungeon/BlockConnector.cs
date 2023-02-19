using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BlockConnector
{    
    private DungeonBlock first;

    [SerializeField]
    private Vector2Int coordinates;
    
    private DungeonBlock second;

    public Vector2Int Coordinates
    {
        get { return coordinates; }
    }

    public DungeonBlock[] Connects
    {
        get
        {
            if (IsOpen)
            {
                return new DungeonBlock[] { first };
            }
            return new DungeonBlock[] { first, second };
        }
    }

    public BlockConnector(DungeonBlock first, Vector2Int coordinates)
    {
        this.first = first;
        this.coordinates = coordinates;
        second = null;

        Debug.Log(this);
    }

    public override string ToString()
    {
        return $"<Connector at {coordinates} is open: {IsOpen}, connects {first} with {second}>";
    }

    public bool IsOpen
    {
        get { return second == null; }
    }

    public DungeonOvelap GetOverlap(Vector2Int coordinates)
    {
        if (this.coordinates != coordinates) return DungeonOvelap.Nothing;
        
        return IsOpen ? DungeonOvelap.Connection : DungeonOvelap.Collision;
    }

    public static bool IsConnector(int value)
    {
        return value == (int)BlockTileTypes.Exit || value == (int)BlockTileTypes.Door;
    }

    public static bool IsNotConnector(int value)
    {
        return value != (int)BlockTileTypes.Nothing && !IsConnector(value);
    }

    public bool Connect(DungeonBlock second)
    {
        if (second.DungeonPositions(IsConnector).Any(kvp => kvp.Key == coordinates))
        {
            this.second = second;
            return true;
        }
        return false;
    }
}
