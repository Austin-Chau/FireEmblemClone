using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitTilesContainer
{
    public Dictionary<Vector2Int, GameObject> tilePositions; //all positions should be positive

    public List<Vector2Int> leftRotationSweep;
    public List<Vector2Int> rightRotationSweep;

    Tile sourceTile;
    public int rotation;
    public int Rotation
    {
        get
        {
            return rotation;
        }
        set
        {
            rotation = value;
            UpdateSprites();

        }

    }
    const string spritePrefab = "Prefabs/UnitSegmentSprite";

    public UnitTilesContainer(int width, int height, Tile _sourceTile, Transform _spritesParent)
    {
        //width/height should be odd for now, width < height

        leftRotationSweep = new List<Vector2Int>();
        rightRotationSweep = new List<Vector2Int>();

        int horizontalRadius = (width - 1) / 2;
        int verticalRadius = (height - 1) / 2;

        //Compute the tile positions that this unit will hit when rotating (different for rotating 90/-90 degrees
        //Assumes the unit is a rectangle.
        double sqrDistance = Math.Pow(horizontalRadius + .5, 2) + Math.Pow(verticalRadius + .5, 2);
        //Debug.Log(sqrDistance);
        Debug.Log("Calculating sweeps:");
        for (int i = horizontalRadius; i >= -verticalRadius-1; i--)
        {
            double jmax;
            //Slope of the circle matters: if i > 0, the circle slopes up to the left, so we need to compensate and check one higher on j
            jmax = Math.Sqrt(sqrDistance - Math.Pow(i + .5, 2));
            //Debug.Log(jmax);
            //jmax = i >= 0 ? Math.Ceiling(jmax) : Math.Ceiling(jmax-.5);
            jmax = Math.Floor(jmax);
            for (int j = 0; j <= jmax; j++)
            {
                leftRotationSweep.Add(new Vector2Int(i, j));
                leftRotationSweep.Add(new Vector2Int(-i, -j));

                rightRotationSweep.Add(new Vector2Int(j, -i));
                rightRotationSweep.Add(new Vector2Int(-j, i));
                Debug.Log("Point: "+i + "," + j);
            }
        }

        sourceTile = _sourceTile;
        rotation = 0;
        tilePositions = new Dictionary<Vector2Int, GameObject>();
        GameObject prefab = Resources.Load<GameObject>(spritePrefab);
        for (int i = -horizontalRadius; i <= horizontalRadius; i++)
        {
            for (int j = -verticalRadius; j <= verticalRadius; j++)
            {
                GameObject newSprite = UnityEngine.Object.Instantiate(prefab, _spritesParent, false);
                newSprite.transform.localPosition = new Vector3(i, j, 0);

                tilePositions[new Vector2Int(i, j)] = newSprite;
            }
        }
    }

    public static Vector2Int RotateVector2Int(Vector2Int _initial, int _rotation)
    {
        if (_rotation == 0)
        {
            return _initial;
        }

        int x = 0;
        int y = 0;

        switch (_rotation)
        {
            case 1:
                x = _initial.y;
                y = -_initial.x;
                break;
            case 2:
                x = -_initial.x;
                y = -_initial.y;
                break;
            case 3:
                x = -_initial.y;
                y = _initial.x;
                break;
        }

        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Returns true if the unit is out of bounds at _origin with rotation _rotation.
    /// </summary>
    /// <param name="_origin"></param>
    /// <param name="_rotation"></param>
    /// <returns></returns>
    public bool CheckOutOfBounds(Vector2Int _origin, int _rotation)
    {
        bool returnValue = false;
        foreach( KeyValuePair<Vector2Int,GameObject> pair in tilePositions)
        {
            Vector2Int vector = RotateVector2Int(pair.Key, _rotation);
            if ((vector.x+_origin.x < 0 || vector.x+_origin.x >= BoardManager.columns)
                                || (vector.y + _origin.y < 0 || vector.y + _origin.y >= BoardManager.rows)
                                || GameManager.instance.BoardRockData[vector.x + _origin.x, vector.y + _origin.y])
            {
                returnValue = true;
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Returns true if the unit cannot rotate at _origin position.
    /// </summary>
    /// <param name="_tilePositions"></param>
    /// <param name="_origin"></param>
    /// <param name="_rotationDelta">Integer between -2 and 2</param>
    /// <returns></returns>
    public bool CheckIfCannotRotate(Vector2Int _origin, int _rotation, int _rotationDelta)
    {
        switch (_rotationDelta)
        {
            case 0:
                return false;
            case 1:
                foreach(Vector2Int vector in rightRotationSweep)
                {
                    Vector2Int rotatedVector = RotateVector2Int(vector, _rotation);
                    Debug.Log("Checking " + rotatedVector + " for rotation sweep from " + _origin);
                    if (rotatedVector.x + _origin.x >= 0
                        && rotatedVector.x + _origin.x < BoardManager.columns
                        && rotatedVector.y + _origin.y >= 0
                        && rotatedVector.y + _origin.y < BoardManager.rows
                        && GameManager.instance.BoardRockData[rotatedVector.x+_origin.x,rotatedVector.y+_origin.y])
                    {
                        Debug.Log("Sweep failed");
                        return true;
                    }
                }
                Debug.Log("Sweep succeeded");
                return false;
            case -1:
                foreach (Vector2Int vector in leftRotationSweep)
                {
                    Vector2Int rotatedVector = RotateVector2Int(vector, _rotation);
                    Debug.Log("Checking " + rotatedVector + " for rotation sweep from " + _origin);
                    if (rotatedVector.x + _origin.x >= 0
                        && rotatedVector.x + _origin.x < BoardManager.columns
                        && rotatedVector.y + _origin.y >= 0
                        && rotatedVector.y + _origin.y < BoardManager.rows
                        && GameManager.instance.BoardRockData[rotatedVector.x + _origin.x, rotatedVector.y + _origin.y])
                    {
                        //Debug.Log("Sweep failed");
                        return true;
                    }
                }
                return false;
            case 3:
            case -3:
            case 2:
            case -2:
                foreach (Vector2Int vector in rightRotationSweep)
                {
                    Vector2Int rotatedVector = RotateVector2Int(vector, _rotation);
                    Debug.Log("Checking " + rotatedVector + " for rotation sweep from " + _origin);
                    if (rotatedVector.x + _origin.x >= 0
                        && rotatedVector.x + _origin.x < BoardManager.columns
                        && rotatedVector.y + _origin.y >= 0
                        && rotatedVector.y + _origin.y < BoardManager.rows
                        && GameManager.instance.BoardRockData[rotatedVector.x + _origin.x, rotatedVector.y + _origin.y])
                    {
                        Debug.Log("Sweep failed");
                        return true;
                    }
                }
                foreach (Vector2Int vector in leftRotationSweep)
                {
                    Vector2Int rotatedVector = RotateVector2Int(vector, _rotation);
                    Debug.Log("Checking " + rotatedVector + " for rotation sweep from " + _origin);
                    if (rotatedVector.x + _origin.x >= 0
                        && rotatedVector.x + _origin.x < BoardManager.columns
                        && rotatedVector.y + _origin.y >= 0
                        && rotatedVector.y + _origin.y < BoardManager.rows
                        && GameManager.instance.BoardRockData[rotatedVector.x + _origin.x, rotatedVector.y + _origin.y])
                    {
                        //Debug.Log("Sweep failed");
                        return true;
                    }
                }
                return false;
        }
        return false;
    }

    public bool CheckIfCanMakeStep(Vector2Int _origin, Vector2Int _destination, int _initialRotation, int _deltaRotation)
    {
        //First check if we can make the rotation
        if (_deltaRotation != 0)
        {
        }
        return false;
    }

    /// <summary>
    /// Given a position and rotation and movement type, computes the combined weight of all the tiles in this container's stored shape.
    /// </summary>
    /// <param name="tilePositions"></param>
    /// <param name="_origin"></param>
    /// <param name="_rotation"></param>
    /// <param name="_movementType"></param>
    /// <returns></returns>
    public int GetWeight(Vector2Int _origin, int _rotation, MovementTypes _movementType)
    {
        int maximumWeight = 0;
        foreach (KeyValuePair<Vector2Int,GameObject> pair in tilePositions)
        {
            int tempWeight = 0;
            Vector2Int vector = RotateVector2Int(pair.Key, _rotation);
            int x = vector.x + _origin.x;
            int y = vector.y + _origin.y;

            if (x >= 0 && x < BoardManager.columns && y >= 0 && y < BoardManager.rows)
            {
                tempWeight = GameManager.instance.Board.Tiles[vector.x + _origin.x, vector.y + _origin.y].MovementWeights[_movementType];
            }

            if (tempWeight > maximumWeight)
            {
                maximumWeight = tempWeight;
            }
        }

        return maximumWeight;
    }

    public void UpdateSprites()
    {
        foreach(KeyValuePair<Vector2Int,GameObject> pair in tilePositions)
        {
            Vector2Int rotatedVector = RotateVector2Int(pair.Key, rotation);

            pair.Value.transform.localPosition = new Vector3(rotatedVector.x, rotatedVector.y, 0);
        }
    }
}
