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
    /// Generates a floodfill of tiles.
    /// </summary>
    /// <returns>The list of positions.</returns>
    /// <param name="sourceTile">Current position in integer coordinates of the unit.</param>
    /// <param name="radius">Move radius of the unit.</param>
    public static Dictionary<Tile, int> GenerateTileTree(Tile sourceTile, int radius, MovementTypes movementType, bool includeFringes)
    {
        Dictionary<Tile, int> tileDistances = new Dictionary<Tile, int>();
        int moveCounter = 0;

        tileDistances[sourceTile] = 0;

        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Down), tileDistances, movementType, includeFringes);
        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Left), tileDistances, movementType, includeFringes);
        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Up), tileDistances, movementType, includeFringes);
        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Right), tileDistances, movementType, includeFringes);
        
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
    /// FloodFill Algorithm for finding distances from a point of origin (uses recursion)
    /// </summary>
    /// <param name="count">Current accumulated distance</param>
    /// <param name="radius">The max distance we want to flood fill</param>
    /// <param name="currentTile">Tile currently being checked</param>
    /// <param name="tileDistances">A dictionary of tiles with their distances</param>
    /// <param name="movementType">What type of traversal is being performed</param>
    /// <param name="includeFringes">Should the last tiles that are >= radius be included?</param>
    private static void FloodFill(int count, int radius, Tile currentTile, Dictionary<Tile, int> tileDistances, MovementTypes movementType, bool includeFringes)
    {
        if (currentTile == null)
        {
            return;
        }

        int distanceFromOrigin = count;

        distanceFromOrigin += currentTile.MovementWeights[movementType];

        //If going to the tile would go over the moveRadius, don't add to the list. If include fringes is true, actually add this tile.
        if (distanceFromOrigin > radius && !includeFringes)
        {
            //Debug.Log("farther than the origin");
            return;
        }

        if (tileDistances.ContainsKey(currentTile))
        {
            //Debug.Log("already have that tile");
            //Replace if the distance is lower?
            return;
        }

        //Otherwise, add to the list and check the adjacent tiles
        tileDistances[currentTile] = distanceFromOrigin;

        //We stop for sure here.
        if (distanceFromOrigin >= radius)
        {
            return;
        }

        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Down), tileDistances, movementType, includeFringes);
        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Left), tileDistances, movementType, includeFringes);
        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Up), tileDistances, movementType, includeFringes);
        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Right), tileDistances, movementType, includeFringes);
    }
}
