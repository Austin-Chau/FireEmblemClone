using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    private readonly static AdjacentDirection[] adjacentDirections = new AdjacentDirection[4]
        {AdjacentDirection.Up, AdjacentDirection.Right, AdjacentDirection.Down, AdjacentDirection.Left };

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
    /*
    /// <summary>
    /// Generates a floodfill of tiles.
    /// </summary>
    /// <returns>The list of positions.</returns>
    /// <param name="sourceTile">Current position in integer coordinates of the unit.</param>
    /// <param name="radius">Move radius of the unit.</param>
    /// <param name="movementType">What type of movement.</param>
    /// <param name="includeFringes">Should the final tile >= the radius be included?</param>
    /// <param name="ignoreEnemies">Should this floodfill ignore enemies.</param>
    /// <param name="team">What team this floodfill is in the name of.</param>
    public static Dictionary<Tile, int> GenerateTileTree(Tile sourceTile, int radius, MovementTypes movementType, bool includeFringes, bool ignoreEnemies, Team team)
    {
        Dictionary<Tile, int> tileDistances = new Dictionary<Tile, int>();
        int moveCounter = 0;

        tileDistances[sourceTile] = 0;

        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Down), tileDistances, movementType, includeFringes, ignoreEnemies, team);
        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Left), tileDistances, movementType, includeFringes, ignoreEnemies, team);
        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Up), tileDistances, movementType, includeFringes, ignoreEnemies, team);
        FloodFill(moveCounter, radius,
                sourceTile.GetAdjacentTile(AdjacentDirection.Right), tileDistances, movementType, includeFringes, ignoreEnemies, team);
        
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
    private static void FloodFill(int count, int radius, Tile currentTile, Dictionary<Tile, int> tileDistances, MovementTypes movementType, bool includeFringes, bool ignoreEnemies, Team team)
    {
        if (currentTile == null)
        {
            return;
        }

        int distanceFromOrigin = count;

        distanceFromOrigin += currentTile.MovementWeights[movementType];

        //If going to the tile would go over the moveRadius, don't add to the list and stop the recursion.
        //If the tile includes an enemy unit and we do not wish to ignore them, don't add it to the list and stop the recursion.
        //If include fringes is true, actually add this tile.
        if (distanceFromOrigin > radius || (!ignoreEnemies && currentTile.Occupied && currentTile.CurrentUnit.Team != team) && !includeFringes)
        {
            //Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Tile" + currentTile.GridPosition + " is farther than the origin");
            return;
        }

        if (tileDistances.ContainsKey(currentTile))
        {
            //Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Tile " + currentTile.GridPosition + " already in dictionary");
            if (tileDistances[currentTile] > count)
                tileDistances[currentTile] = count;
            else
                return;
        }

        //Otherwise, add to the list and check the adjacent tiles
        tileDistances[currentTile] = distanceFromOrigin;
        
        //Debug.Log(currentTile.GridPosition + " added");

        //We stop for sure here.
        if (distanceFromOrigin >= radius)
        {
            return;
        }

        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Down), tileDistances, movementType, includeFringes, ignoreEnemies, team);
        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Left), tileDistances, movementType, includeFringes, ignoreEnemies, team);
        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Up), tileDistances, movementType, includeFringes, ignoreEnemies, team);
        FloodFill(distanceFromOrigin, radius,
                currentTile.GetAdjacentTile(AdjacentDirection.Right), tileDistances, movementType, includeFringes, ignoreEnemies, team);
    }
    */

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
    /// Initiates the floodfill algorithm. Copied summary from the other method:
    /// The perspective of this floodfill is that moving to currentTile is granted:
    /// now we are judging which of the adjacent tiles we can move to.
    /// Constantly updates _nodesList and _nodesDictionary (which maps tiles to nodes),
    /// which we can perform A* or some other algorithm on.
    /// </summary>
    public static Dictionary<Tile,TileNode> NewGenerateTileTree(
        Tile _startingTile,
        int _maxDistance,
        MovementTypes _movementType,
        UnitTilesContainer unitTilesContainer,
        int unitCurrentRotation)
    {
        TileNode currentNode = new TileNode(_startingTile.GridPosition);
        Dictionary<Tile, TileNode> nodesDictionary = new Dictionary<Tile, TileNode>();
        List<TileNode> nodesList = new List<TileNode>();

        nodesDictionary[_startingTile] = currentNode;
        nodesList.Add(currentNode);

        NewFloodFill(
            currentNode,
            nodesDictionary,
            _startingTile,
            nodesList,
            unitTilesContainer,
            unitCurrentRotation,
            _movementType,
            0,
            _maxDistance,
            4);
        /*
        foreach(KeyValuePair<Tile, TileNode> pair in nodesDictionary)
        {
            Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "----------");
            Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Reachable tile pos: "+ pair.Key.GridPosition);
            foreach (KeyValuePair<TileNode, Tuple<int,int>> pair2 in pair.Value.neighborsWithRotationAndWeightToReachThem)
            {
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "From this tile, can move to: "+pair2.Key.position+" with rotation "+pair2.Value.Item1+", resulting weight is "+pair2.Value.Item2);
            }
            if (pair.Value.neighborsWithRotationAndWeightToReachThem.Keys.Count == 0)
            {
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Cannot move to any tiles from here.");
            }
        }
        Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "----------");
        */
        return nodesDictionary;
    }

    /// <summary>
    /// The perspective of this floodfill is that moving to currentTile is granted:
    /// now we are judging which of the adjacent tiles we can move to.
    /// Constantly updates _nodesList and _nodesDictionary (which maps tiles to nodes),
    /// which we can perform A* or some other algorithm on.
    /// </summary>
    private static void NewFloodFill(
        TileNode _currentNode,
        Dictionary<Tile,TileNode> _nodesDictionary,
        Tile _currentTile,
        List<TileNode> _nodesList,
        UnitTilesContainer _unitTilesContainer,
        int _currentRotation,
        MovementTypes _movementType,
        int _currentDistance,
        int _maxDistance,
        int _excludedDirection)
    {
        //Attempt each cardinal direction
        DateTime currentTime = DateTime.Now;
        Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "----------");
        Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Checking tile " + _currentTile.GridPosition);
        for (int i = 0; i < 4; i++)
        {
            Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Excluded direction/i: "+_excludedDirection + " "+i);
            if (i == _excludedDirection)
            {
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "skipping " + i);
                continue;
            }
            //the index in adjacentDirections that we should ignore in the next floodfill (if we go right, ignore left etc)
            int newExcludedDirection = i > 1 ? i - 2 : i + 2;

            AdjacentDirection direction = adjacentDirections[i];
            Tile adjacentTile = _currentTile.GetAdjacentTile(direction);
            int newRotation;
            int deltaRotation = 0;
            int movementWeight;

            if (adjacentTile == null)
            {
                continue;
            }
            //Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "checking oob at: " + adjacentTile.GridPosition + " with rotation: " + _currentRotation);
            //check out of bounds, just shifting in the direction. if OOB, try a 90degree and -90degree rotation, then moving.
            if (_unitTilesContainer.CheckOutOfBounds(adjacentTile.GridPosition, _currentRotation))
            {
                //this if block is jankily written
                //go 90 degrees
                //Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "no rotation failed");
                deltaRotation = 1;
                if (_unitTilesContainer.CheckIfCannotRotate(_currentTile.GridPosition, _currentRotation, deltaRotation)
                    || _unitTilesContainer.CheckOutOfBounds(adjacentTile.GridPosition, _currentRotation+ deltaRotation))
                {
                    Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "failed to go 90");
                    //go -90 degrees
                    //Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "deltaRotation 1 failed");
                    deltaRotation = -1;
                    if (_unitTilesContainer.CheckIfCannotRotate(_currentTile.GridPosition, _currentRotation, deltaRotation)
                        || _unitTilesContainer.CheckOutOfBounds(adjacentTile.GridPosition, _currentRotation + deltaRotation))
                    {
                        //Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "deltaRotation -1 failed");
                        Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "failed to go -90");
                        //neither rotation worked, skip to the next direction
                        continue;
                    }
                }
            }

            Debug.Log("" + currentTime.Ticks % 10000000 + " " + "deltaRotation: " + deltaRotation);

            newRotation = _currentRotation + deltaRotation;
            newRotation = ModuloCorrect.CustomMath.mod(newRotation, 4);

            //now grab the weight of the new position if we were to move+rotate to it
            movementWeight = _unitTilesContainer.GetWeight(adjacentTile.GridPosition, newRotation, _movementType);
            int newDistance = _currentDistance + movementWeight;
            
            if (newDistance > _maxDistance)
            {
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Distance has become too far -> return");
                continue; //too far, give up
            }

            //we are not too far, time to add the node connection 
            TileNode adjacentNode;
            if (_nodesDictionary.ContainsKey(adjacentTile))
            {
                //the adjacent node has already been connected to (but from a different node)
                adjacentNode = _nodesDictionary[adjacentTile];

                if (_currentNode.neighborTransitions.ContainsKey(adjacentNode))
                {
                    Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Tried to add an edge that already existed -> return");
                    continue; //we are looping back on ourselves, stop
                }
            }
            else
            {
                //a new node must be made
                adjacentNode = new TileNode(adjacentTile.GridPosition);
                _nodesList.Add(adjacentNode);
                _nodesDictionary[adjacentTile] = adjacentNode;
            }
            _currentNode.neighborTransitions[adjacentNode] = new TileNodeTransition(deltaRotation, _currentRotation, newRotation, newDistance);
            adjacentNode.neighborTransitions[_currentNode] = new TileNodeTransition(-deltaRotation, newRotation, _currentRotation, _currentDistance);
            /*
                TileNode _currentNode,
                Dictionary<Tile,TileNode> _nodesDictionary,
                Tile _currentTile,
                List<TileNode> _nodesList,
                List<Vector2Int> _unitTilePositions,
                int _currentRotation,
                MovementTypes _movementType,
                int _currentDistance,
                int _maxDistance,
                AdjacentDirection _sourceDirection
                */
            Debug.Log("" + currentTime.Ticks % 10000000 + " " + "Going to " + adjacentTile.GridPosition + " with excludedDirection " + newExcludedDirection);
            NewFloodFill(
                adjacentNode,
                _nodesDictionary,
                adjacentTile,
                _nodesList,
                _unitTilesContainer,
                newRotation,
                _movementType,
                newDistance,
                _maxDistance,
                newExcludedDirection);
        }

    }

    public static Stack<PathfindingStep> NewGenerateSteps(Tile _initialTile, Tile _finalTile, Dictionary<Tile, TileNode> _nodesDictionary)
    {
        DateTime currentTime = DateTime.Now;

        //Stack<TileNode> nodes = new Stack<TileNode>();
        Stack<PathfindingStep> steps = new Stack<PathfindingStep>();

        TileNode currentNode = _nodesDictionary[_finalTile];
        TileNode destinationNode = _nodesDictionary[_initialTile];

        if (currentNode == destinationNode)
        {
            return steps;
        }

        Tile destinationTile;
        PathfindingStep step;

        TileNode baseNode = currentNode;
        Debug.Log("" + currentTime.Ticks % 10000000 + " " + "baseNode position: " + baseNode.position);
        //Do the first step manually, since there may only be one step, in which case the full algorithm does not work
        TileNode lowestNode = getLowestNode(currentNode);

        int deltaRotation = lowestNode.neighborTransitions[currentNode].deltaRotation;

        Tile baseTile = GameManager.instance.Board.Tiles[baseNode.position.x, baseNode.position.y];
        Tile lowestTile = GameManager.instance.Board.Tiles[lowestNode.position.x, lowestNode.position.y];
        Tile currentTile = GameManager.instance.Board.Tiles[currentNode.position.x, currentNode.position.y];

        AdjacentDirection newDirection = GetAdjacentTilesDirection(lowestTile, currentTile);
        int newRotation = lowestNode.neighborTransitions[currentNode].finalRotation;

        AdjacentDirection baseDirection = newDirection;
        int baseRotation = newRotation;

        int oldDeltaRotation = deltaRotation;

        while (currentNode != destinationNode)
        {
            //Get the "closest" (lowest weight) node to our destination (where the unit currently is)
            lowestNode = getLowestNode(currentNode);
            Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Lowest node position: " + lowestNode.position);

            //Now, grab the rotation information. first node refers to the first node the unit will start on
            //during a movement, secondNode the node they are moving to (so the one lower on the stack, if it goes at all on the stack)

            deltaRotation = lowestNode.neighborTransitions[currentNode].deltaRotation;

            lowestTile = GameManager.instance.Board.Tiles[lowestNode.position.x, lowestNode.position.y];
            currentTile = GameManager.instance.Board.Tiles[currentNode.position.x, currentNode.position.y];

            newDirection = GetAdjacentTilesDirection(lowestTile, currentTile);
            newRotation = lowestNode.neighborTransitions[currentNode].finalRotation;

            if (newDirection != baseDirection || newRotation != baseRotation)
            {
                /*
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Change in movement detected, destination is " + baseNode.position);
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "Deltarotation is " + deltaRotation);
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "currentTile is " + currentTile.GridPosition);
                Debug.Log( "" + currentTime.Ticks % 10000000 + " " + "lowestTile is " + lowestTile.GridPosition);
                */
                step = new PathfindingStep(currentTile, baseTile, newDirection, newRotation, oldDeltaRotation, baseRotation, PathCreationStepTypes.None);

                baseDirection = newDirection;
                baseRotation = newRotation;

                baseNode = currentNode;
                baseTile = GameManager.instance.Board.Tiles[baseNode.position.x, baseNode.position.y];
                Debug.Log("" + currentTime.Ticks % 10000000 + " " + "new baseNode is " + baseNode.position);
                steps.Push(step);
            }
            oldDeltaRotation = deltaRotation;

            currentNode = lowestNode;
        }

        step = new PathfindingStep(lowestTile, baseTile, newDirection, newRotation, deltaRotation, baseRotation, PathCreationStepTypes.None);
        steps.Push(step);

        //Debug.Log("steps count: "+steps.Count);
        return steps;
    }

    private static TileNode getLowestNode(TileNode _currentNode)
    {
        int lowestValue = int.MaxValue;
        TileNode lowestNode = null;

        Dictionary<TileNode, TileNodeTransition>.KeyCollection nodesToCheck = _currentNode.neighborTransitions.Keys;

        foreach (TileNode node in nodesToCheck)
        {
            int weight = _currentNode.neighborTransitions[node].cumulativeTileWeight;
            if (weight < lowestValue)
            {
                lowestValue = weight;
                lowestNode = node;
            }
        }

        return lowestNode;
    }


    public class TileNode
    {
        public Dictionary<TileNode, TileNodeTransition> neighborTransitions;
        public Vector2Int position;

        public TileNode(Vector2Int _position)
        {
            neighborTransitions = new Dictionary<TileNode, TileNodeTransition>();
            position = _position;
        }
    }

    public class TileNodeTransition
    {
        public int initialRotation;
        public int finalRotation;
        public int cumulativeTileWeight;
        public int deltaRotation;

        public TileNodeTransition(int _deltaRotation, int _initialRotation, int _finalRotation, int _cumulativeTileWeight)
        {
            initialRotation = _initialRotation;
            finalRotation = _finalRotation;
            cumulativeTileWeight = _cumulativeTileWeight;
            deltaRotation = _deltaRotation;
        }
    }

    public class PathfindingStep
    {
        public Tile initialTile;
        public Tile finalTile;
        public int initialRotation;
        public int deltaRotation;
        public int finalRotation;
        public AdjacentDirection direction;

        private PathCreationStepTypes movementType;
        private GameObject arrowSprite;
        private const string TranslationArrowPrefab = "Prefabs/TranslationArrow";

        public PathfindingStep(Tile _initialTile, Tile _finalTile, AdjacentDirection _direction, int _initialRotation, int _deltaRotation, int _finalRotation, PathCreationStepTypes _type)
        {
            initialTile = _initialTile;
            finalTile = _finalTile;
            initialRotation = _initialRotation;
            deltaRotation = _deltaRotation;
            finalRotation = _finalRotation;
            direction = _direction;

            movementType = _type;
            //tileObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(TileGameObjectResource));
            if (movementType == PathCreationStepTypes.Translation)
            {
                arrowSprite = UnityEngine.Object.Instantiate(
                    Resources.Load<GameObject>(TranslationArrowPrefab),initialTile.Position,Quaternion.identity);
            }
        }

        public void Clear()
        {
            if (arrowSprite == null)
            {
                return;
            }

            GameObject.Destroy(arrowSprite);
        }
    }

    /*
    private class TileEdge
    {
        public Dictionary<TileNode, Tuple<TileNode,int>> neighborWithRotation;

        public TileEdge(TileNode node1, int rotationToReachNode2, TileNode node2, int rotationToReachNode1)
        {
            neighborWithRotation[node1] = new Tuple<TileNode, int>(node2, rotationToReachNode2);
            neighborWithRotation[node2] = new Tuple<TileNode, int>(node1, rotationToReachNode1);
        }
    }
    */
}
