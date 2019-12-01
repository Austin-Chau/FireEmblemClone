using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board 
{
    //Singleton instance
    public static Board Instance;

    public Tile[,] Tiles { get; private set; }
    public Vector2[,] Positions { get; private set; }

    int columns;
    int rows;
    GameObject boardObject;

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
            boardObject = GameObject.Instantiate(new GameObject());
            boardObject.name = "Board";
            Tiles = new Tile[_rows, _columns];
            Positions = new Vector2[_rows, _columns];
            GenerateTileMap();
        }
    }

    /// <summary>
    /// Creates random tile map.
    /// </summary>
    void GenerateTileMap()
    {
        int enumLength = System.Enum.GetNames(typeof(TileType)).Length;
        TileType tileType;
        for(int i = 0; i < rows; i++)
        {
            for(int j = 0; j < columns; j++)
            {
                Positions[i, j] = new Vector2(i, j);
                tileType = Random.value > .2f ? TileType.Ground : TileType.Wall;
                Tiles[i, j] = new Tile(Positions[i, j],
                    tileType,
                    boardObject.transform);
            }
        }
    }


}
