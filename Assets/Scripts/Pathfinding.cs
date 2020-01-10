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
    /// <param name="InitialTile">Initial position of the unit.</param>
    /// <param name="FinalTile">Final position of the unit.</param>
    /// <param name="tileDistances">A list of all possible nodes that can be travelled</param>
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
                if(tileDistances.ContainsKey(tile) && tileDistances[tile] < lowestValue)
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
    /// <param name="unitTile">Current position in integer coordinates of the unit.</param>
    /// <param name="moveRadius">Move radius of the unit.</param>
    /// <param name="team">Team that the unit generating movetree is on</param>
    public static Dictionary<Tile, int> GenerateMoveTree(Tile unitTile, int moveRadius, Team team)
    {
        Dictionary<Tile, int> tileDistances = new Dictionary<Tile, int>();
        int moveCounter = 0;

        tileDistances[unitTile] = 0;

        FloodFill(moveCounter, moveRadius,
                unitTile.GetAdjacentTile(AdjacentDirection.Down), tileDistances, team);
        FloodFill(moveCounter, moveRadius,
                unitTile.GetAdjacentTile(AdjacentDirection.Left), tileDistances, team);
        FloodFill(moveCounter, moveRadius,
                unitTile.GetAdjacentTile(AdjacentDirection.Up), tileDistances, team);
        FloodFill(moveCounter, moveRadius,
                unitTile.GetAdjacentTile(AdjacentDirection.Right), tileDistances, team);
        
        return tileDistances;
    }

    public static AdjacentDirection GetAdjacentTilesDirection(Tile startTile, Tile endTile)
    {
        foreach (KeyValuePair<AdjacentDirection, Tile> pair in startTile.GetAdjacentTilesDictionary())
        {
            if (pair.Value == endTile)
            {
                return pair.Key;
            }
        }
        return AdjacentDirection.None;
    }

    /// <summary>
    /// Obtain the direction that a tile is using x y coordinates.
    /// Prioritizes X direction over Y.
    /// </summary>
    /// <param name="source">Start Tile</param>
    /// <param name="dest">End Tile</param>
    /// <returns>Returns a Vector2Int with a 1 or -1 in the direction of the target. Vector.Zero if tile is the same.</returns>
    public static Vector2Int GetTileDirectionVector(Tile source, Tile dest)
    {
        if (source == dest) return Vector2Int.zero;

        int xDiff;
        int yDiff;

        xDiff = (int)(dest.Position.x - source.Position.x);
        yDiff = (int)(dest.Position.y - source.Position.y);

        if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff)) return new Vector2Int(xDiff / Mathf.Abs(xDiff), 0);
        else return new Vector2Int(0, yDiff / Mathf.Abs(yDiff));


    }

    /// <summary>
    /// FloodFill Algorithm for finding distances from a point of origin
    /// </summary>
    /// <param name="count">Amount of movement units currently moved</param>
    /// <param name="moveRadius">Limit that unit can move</param>
    /// <param name="currentTile">Tile currently being checked</param>
    /// <param name="tileDistances">A dictionary of tiles with their distances</param>
    /// <param name="team">Team that unit is from, should be passed from starting function</param>
    private static void FloodFill(int count, int moveRadius, Tile currentTile, Dictionary<Tile, int> tileDistances, Team team)
    {
        if (currentTile == null)
        {
            return;
        }

        int distanceFromOrigin = currentTile.MovementWeight + count;

        //Debug.Log(distanceFromOrigin);
        //If going to the tile would go over the moveRadius, don't add to the list
        if (distanceFromOrigin > moveRadius)
        {
            //Debug.Log("Tile" + currentTile.GridPosition + " is farther than the origin");
            return;
        }

        //If not on same team, this counts as an obstacle and can not be passed through.
        if (currentTile.Occupied && currentTile.CurrentUnit.Team != team)
            return;

        if (tileDistances.ContainsKey(currentTile))
        {
            //Debug.Log("Tile " + currentTile.GridPosition + " already in dictionary");
            if (tileDistances[currentTile] > count)
                tileDistances[currentTile] = count;
            else
                return;
        }

        //Otherwise, add to the list and check the adjacent tiles
        tileDistances[currentTile] = distanceFromOrigin;
        
        //Debug.Log(currentTile.GridPosition + " added");

        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Down), tileDistances, team);
        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Left), tileDistances, team);
        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Up), tileDistances, team);
        FloodFill(distanceFromOrigin, moveRadius,
                currentTile.GetAdjacentTile(AdjacentDirection.Right), tileDistances, team);
    }
}
