using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board 
{
    //Singleton instance
    public static Board Instance;

    #region Public Variables
    public Tile[,] Tiles { get; private set; }
    public Vector2[,] Positions { get; private set; }
    public int columns { get; private set; }
    public int rows { get; private set; }
    //public List<GameObject> SpawnedEnemyUnits;
    #endregion

    #region Constants

    private const string TileGameObjectResource =
        "Prefabs/Tile";
    private const string GroundSpriteResource =
        "Tiles/Grounds";
    private const string WallSpriteResource =
        "Tiles/Walls";

    #endregion

    GameObject boardObject;
    Transform WallsParent;
    Transform FloorsParent;

    public Board(int _rows, int _columns)
    {
        if (Instance != null)
        {
            Debug.LogError("Board attempted to be instantiated when one already exists.");
        }
        else
        {
            Instance = this;
            rows = _rows;
            columns = _columns;

            GameObject BoardObject = new GameObject("Board");
            GameObject WallsObject = new GameObject("Walls");
            GameObject FloorsObject = new GameObject("Floors");

            boardObject = Object.Instantiate(BoardObject);
            WallsParent = Object.Instantiate(WallsObject, boardObject.transform).transform;
            FloorsParent = Object.Instantiate(FloorsObject, boardObject.transform).transform;

            Object.Destroy(BoardObject);
            Object.Destroy(WallsObject);
            Object.Destroy(FloorsObject);

            Tiles = new Tile[_rows, _columns];
            Positions = new Vector2[_rows, _columns];
            GenerateTileMap();
        }
    }
    
    /// <summary>
    /// Checks if value is filled by another unit. 
    /// Currently checks if tile is Wall as well, but will have to change if walls are passable
    /// </summary>
    /// <param name="x">x in grid position</param>
    /// <param name="y">y in grid position</param>
    /// <returns>True if pass, false otherwise</returns>
    public bool IsTileOccupied(int x, int y)
    {
        //Debug.Log(x + ", " + y);
        //Debug.Log(!Tiles[x, y].Occupied);
        //Debug.Log(Tiles[x, y].type == TileType.Ground);
        if (Tiles[x, y].Occupied || Tiles[x, y].type == TileType.Wall)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Creates random tile map. Will need to be replaced when maps are actually designed
    /// </summary>
    void GenerateTileMap()
    {
        int enumLength = System.Enum.GetNames(typeof(TileType)).Length;
        TileType tileType;
        for(int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Positions[i, j] = new Vector2(i, j);

                if (i < 3 || j < 3)
                {
                    tileType = TileType.Ground;
                }
                else
                {
                    tileType = Random.value > .2f ? TileType.Ground : TileType.Wall;
                }


                Tiles[i, j] = new Tile(Positions[i, j],
                    tileType,
                    tileType == TileType.Ground ? FloorsParent : WallsParent);
                if (i > 0)
                {
                    Tiles[i - 1, j].AddAdjacentTile(AdjacentDirection.Right, Tiles[i, j]);
                    Tiles[i, j].AddAdjacentTile(AdjacentDirection.Left, Tiles[i - 1, j]);
                }
                if (j > 0)
                {
                    Tiles[i, j].AddAdjacentTile(AdjacentDirection.Down, Tiles[i, j - 1]);
                    Tiles[i, j - 1].AddAdjacentTile(AdjacentDirection.Up, Tiles[i, j]);
                }
                Tiles[i, j].AddAdjacentTile(AdjacentDirection.None, Tiles[i, j]);
            }
        }
    }

    /// <summary>
    /// Fetches the tiles in the border of a diamond a certain distance away. 0 = 1 tile, 1 = 4 tile, 2 = 8 tile, etc.
    /// </summary>
    /// <param name="_distance"></param>
    /// <returns>The list of tiles.</returns>
    public List<Tile> GenerateDiamond(int _distance, Tile _centerTile)
    {
        List<Tile> list = new List<Tile>();
        if (_distance == 0)
        {
            //Degenerate case
            list.Add(_centerTile);
            return list;
        }
        for (int i = 0; i < _distance; i++)
        {
            int centerX = _centerTile.GridPosition.x;
            int centerY = _centerTile.GridPosition.y;

            //left
            if (centerX - _distance + i >= 0 && centerY + i < Tiles.GetLength(1))
            {
                Tile tile = Tiles[centerX - _distance + i, centerY + i];
                //Debug.Log("left" + tile.GridPosition);
                list.Add(tile);
            }
            //top
            if (centerX + i < Tiles.GetLength(0) && centerY + _distance - i < Tiles.GetLength(1))
            {
                Tile tile = Tiles[centerX + i, centerY + _distance - i];
                //Debug.Log("top" + tile.GridPosition);
                list.Add(tile);
            }
            //right
            if (centerY - i >= 0 && centerX + _distance - i < Tiles.GetLength(0))
            {
                Tile tile = Tiles[centerX + _distance - i, centerY - i];
                //Debug.Log("right" + tile.GridPosition);
                list.Add(tile);
            }
            //bottom
            if (centerX - i >= 0 && centerY - _distance + i >= 0)
            {
                Tile tile = Tiles[centerX - i, centerY - _distance + i];
                //Debug.Log("bottom" + tile.GridPosition);
                list.Add(tile);
            }
        }
        return list;
    }

    /// <summary>
    /// Deletes all gameobjects and related baggage
    /// </summary>
    public void DeleteGameObjects()
    {
        GameObject.Destroy(boardObject);
    }
}
