using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    /// <summary>
    /// Generates the series of steps for the unit's path from point A to B.
    /// </summary>
    /// <returns>The steps. Each step should be a vector with integer coordinates.</returns>
    /// <param name="InitialPosition">Initial position of the unit.</param>
    /// <param name="FinalPosition">Final position of the unit.</param>
    public static Stack<Vector2> GenerateSteps(Vector2 InitialPosition, Vector2 FinalPosition)
    {
        Stack<Vector2> steps = new Stack<Vector2>();

        //Currently, just go horizontal in one direction then vertical to get to the desired spot
        bool right = InitialPosition.x < FinalPosition.x;
        bool up = InitialPosition.y < FinalPosition.y;
        for (int i = 0; i < Mathf.Abs(FinalPosition.x - InitialPosition.x); i++)
        {
            steps.Push(right ? new Vector2(1, 0) : new Vector2(-1, 0));
        }
        for (int i = 0; i < Mathf.Abs(FinalPosition.y - InitialPosition.y); i++)
        {
            steps.Push(up ? new Vector2(0, 1) : new Vector2(0, -1));
        }
        return steps;
    }

    /// <summary>
    /// Generates all the tiles a unit can move too. 
    /// </summary>
    /// <returns>The list of positions.</returns>
    /// <param name="UnitPosition">Current position in integer coordinates of the unit.</param>
    /// <param name="moveRadius">Move radius of the unit.</param>
    public static List<Vector2> GenerateMoveTree(Vector2 UnitPosition, int moveRadius)
    {
        List<Vector2> movePositions = new List<Vector2>();
        for (int i = -moveRadius; i <= moveRadius; i++)
        {
            for (int j = -(moveRadius - Mathf.Abs(i)); j <= moveRadius - Mathf.Abs(i); j++)
            {
                movePositions.Add(new Vector2(i, j)+UnitPosition);
            }
        }
        return movePositions;
    }
}
