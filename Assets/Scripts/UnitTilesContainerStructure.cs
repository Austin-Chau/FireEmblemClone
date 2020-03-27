using System;
using UnityEngine;
public struct UnitTilesContainerStructure
{
    public Vector2Int pivotPosition;
    public int rotation;
    public int movementWeight;

    public UnitTilesContainerStructure(Vector2Int _pivotPosition, int _rotation, int _movementWeight)
    {
        pivotPosition = _pivotPosition;
        rotation = _rotation;
        movementWeight = _movementWeight;
    }
}
