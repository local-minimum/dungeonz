using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DungeonDesignEventType
{
    SpawnTile,
    DiscardTile,
    SpawnPlaceTile,
    PlaceTile,
    MoveNorth,
    MoveWest,
    MoveEast,
    MoveSouth,
    RotateCW,
    RoteateCCW,
}

public class DungeonDesignerController : MonoBehaviour
{
    public UnityAction<DungeonDesignEventType> OnAction;

    [SerializeField, Range(0, 2)]
    float moveTickSpeed = 0.5f;
    [SerializeField, Range(0, 1)]
    float noMoveThreshold = 0.1f;

    float lastMove;

    [SerializeField, Range(0, 2)]
    float rotateTickSpeed = 0.5f;

    [SerializeField]
    bool useSpawnPlace = false;

    float lastRotate;

    private void Start()
    {
        lastMove = -moveTickSpeed;
    }

    bool HandleMovementAxisInput(string axis, DungeonDesignEventType plusEvent, DungeonDesignEventType minusEvent)
    {
        var value = Input.GetAxis(axis);        
        if (value < -noMoveThreshold)
        {            
            OnAction(minusEvent);
            return true;
        }
        else if (value > noMoveThreshold)
        {            
            OnAction(plusEvent);
            return true;
        }

        return false;
    }

    bool HandleMovement()
    {
        var horizontalMovement = HandleMovementAxisInput("Horizontal", DungeonDesignEventType.MoveEast, DungeonDesignEventType.MoveWest);
        var verticalMovement = HandleMovementAxisInput("Vertical", DungeonDesignEventType.MoveNorth, DungeonDesignEventType.MoveSouth);
        return horizontalMovement || verticalMovement;
    }

    bool HandleRotate()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            OnAction(DungeonDesignEventType.RoteateCCW);
            return true;
        } 
        if (Input.GetKey(KeyCode.E)) {
            OnAction(DungeonDesignEventType.RotateCW);
            return true;
        }

        return false;
    }

    void Update()
    {
        if (Time.timeSinceLevelLoad - lastMove > moveTickSpeed)
        {
            if (HandleMovement())
            {
                lastMove = Time.timeSinceLevelLoad;
            }
        }

        if (Time.timeSinceLevelLoad - lastRotate > rotateTickSpeed)
        {
            if (HandleRotate())
            {
                lastRotate = Time.timeSinceLevelLoad;
            }
        }

        if (!useSpawnPlace && Input.GetKeyDown(KeyCode.R)) {
            OnAction(DungeonDesignEventType.SpawnTile);
        } else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (useSpawnPlace)
            {
                OnAction(DungeonDesignEventType.SpawnPlaceTile);
            } else
            {
                OnAction(DungeonDesignEventType.PlaceTile);
            }
        } else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnAction(DungeonDesignEventType.DiscardTile);
        }
    }
}
