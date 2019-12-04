using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    /// <summary>
    /// Generates the series of steps for the unit's path from point A to B.
    /// </summary>
    /// <returns>The steps. Each step should be a vector with integer coordinates.</returns>
    /// <param name="InitialPosition">Initial position of the unit.</param>
    /// <param name="FinalPosition">Final position of the unit.</param>
    /// <param name="PossibleTiles">A list of all possible nodes that can be travelled</param>
    public static Stack<Tile> GenerateSteps(Tile InitialTile, Tile FinalTile, Dictionary<Tile, int> tileDistances)
    {
        Stack<Tile> steps = new Stack<Tile>();
        Tile currentTile = FinalTile;
        Tile lowestTile;
        int lowestValue;
        List<Tile> tilesToCheck = new List<Tile>();

        steps.Push(currentTile);

        while (steps.Peek() != InitialTile) {
            lowestValue = int.MaxValue;
            lowestTile = null;

            tilesToCheck = currentTile.GetAdjacentTiles();
            foreach(Tile tile in tilesToCheck)
            {
                if(tileDistances[tile] < lowestValue)
                {
                    lowestValue = tileDistances[tile];
                    lowestTile = tile;
                }
            }
            steps.Push(lowestTile);
            currentTile = lowestTile;
        }
        
        return steps;
    }

    /// <summary>
    /// Generates all the tiles a unit can move too. 
    /// </summary>
    /// <returns>The list of positions.</returns>
    /// <param name="UnitPosition">Current position in integer coordinates of the unit.</param>
    /// <param name="moveRadius">Move radius of the unit.</param>
    public static Dictionary<Tile, int> GenerateMoveTree(Tile unitTile, int moveRadius)
    {
        Dictionary<Tile, int> tileDistances = new Dictionary<Tile, int>();
        int moveCounter = 0;

        tileDistances[unitTile] = 0;

        FloodFill(moveCounter, moveRadius, unitTile, tileDistances);
        
        return tileDistances;
    }

    /// <summary>
    /// FloodFill Algorithm for finding distances from a point of origin
    /// </summary>
    /// <param name="count">Amount of movement units currently moved</param>
    /// <param name="moveRadius">Limit that unit can move</param>
    /// <param name="currentTile">Tile currently being checked</param>
    /// <param name="tileDistances">A dictionary of tiles with their distances</param>
    private static void FloodFill(int count, int moveRadius, Tile currentTile, Dictionary<Tile, int> tileDistances)
    {
        int distanceFromOrigin = currentTile.MovementWeight + count;
        //If going to the tile would go over the moveRadius, don't add to the list
        if (distanceFromOrigin > moveRadius) return;
        if (tileDistances.ContainsKey(currentTile)) return;

        //Otherwise, add to the list and check the adjacent tiles
        tileDistances[currentTile] = distanceFromOrigin;

        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Down), tileDistances);
        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Left), tileDistances);
        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Up), tileDistances);
        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Right), tileDistances);
    }
}
