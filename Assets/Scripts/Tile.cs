using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

//Just an enum used to differentiate the two types of tile we have. 
//This will have to be removed when we get more types of tiles and complex ones maybe
public enum TileType
{
    Wall,
    Ground
}

public class Tile
{
    /// <summary>
    /// Construction of this object creates a GameObject at designated position with settings
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="adjTiles"></param>
    /// <param name="_type"></param>
    public Tile(Vector2 pos, TileType _type, Transform parent)
    {
        GridPosition = new Vector2Int((int)pos.x,(int)pos.y);
        type = _type;
        CreateTile();
        tileObject.transform.position = pos;
        Position = pos;
        tileObject.transform.parent = parent;
        tileScript = tileObject.GetComponent<TileScript>();
        MovementWeights = new Dictionary<MovementTypes, int>();
        if (_type == TileType.Ground)
        {
            GameManager.instance.BoardRockData[GridPosition.x, GridPosition.y] = false;
            MovementWeights[MovementTypes.None] = 1;
            MovementWeights[MovementTypes.Ground] = 1;
            MovementWeights[MovementTypes.Flying] = 1;
        }
        else
        {
            GameManager.instance.BoardRockData[GridPosition.x, GridPosition.y] = true;
            MovementWeights[MovementTypes.None] = 1;
            MovementWeights[MovementTypes.Ground] = 100;
            MovementWeights[MovementTypes.Flying] = 1;
        }
    }

    #region Public Variables
    /// <summary>
    /// Checks if there's a unit in this space already
    /// </summary>
    public bool Occupied
    {
        get
        {
            return occupied;
        }
        private set
        {
            occupied = value;
            tileScript.Occupied = value;
        }
    }
    /// <summary>
    /// Assign a unit that is sitting on this tile. Automatically sets the Occupied flag.
    /// </summary>
    public Unit CurrentUnit
    {
        get
        {
            return currentUnit;
        }
        set
        {
            currentUnit = value;
            if (value == null)
            {
                Occupied = false;
            }
            else
            {
                Occupied = true;
            }
        }
    }
    public Dictionary<MovementTypes, int> MovementWeights;
    public Vector3 Position { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    public TileType type { get; private set; }

    #endregion

    #region Private Variables


    GameObject tileObject;

    //Amount of movement it takes to get to the tile
    //MAX_INT if impassable.

    private TileScript tileScript;
    private bool occupied;
    private Unit currentUnit;

    //Four adjacent tiles based on cardinal directions for pathfinding
    private Dictionary<AdjacentDirection, Tile> AdjacentTiles = new Dictionary<AdjacentDirection, Tile>();

    #region Constants

    private const string TileGameObjectResource =
        "Prefabs/Tile";
    private const string GroundSpriteResource =
        "Tiles/Grounds";
    private const string WallSpriteResource =
        "Tiles/Walls";

    #endregion

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets Adjacent Tile
    /// </summary>
    /// <param name="dir">Direction of Adjacent Tile</param>
    /// <returns>Adjacent Tile, or null if it does not exist (meaning tile is a border tile.)</returns>
    public Tile GetAdjacentTile(AdjacentDirection dir)
    {
        if (AdjacentTiles.ContainsKey(dir))
            return AdjacentTiles[dir];
        else
            return null;
    }

    public List<Tile> GetAdjacentTiles()
    {
        List<Tile> tiles = new List<Tile>();

        foreach (KeyValuePair<AdjacentDirection,Tile> pair in AdjacentTiles)
        {
            if (pair.Value != null)
            {
                tiles.Add(pair.Value);
            }
        }

        return tiles;
    }

    public Dictionary<AdjacentDirection,Tile> GetAdjacentTilesDictionary()
    {
        return AdjacentTiles;
    }

    public void AddAdjacentTile(AdjacentDirection dir, Tile tile)
    {
        if (AdjacentTiles.ContainsKey(dir))
        {
            Debug.LogError("Adjacent tile is being added twice.");
        }
        else
        {
            AdjacentTiles[dir] = tile;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates the gameObject tile and associates this object
    /// with the GameObject tile so it can mess with it.
    /// </summary>
    private void CreateTile()
    {
        tileObject = UnityEngine.Object.Instantiate(
            Resources.Load<GameObject>(TileGameObjectResource));

        AttachSpriteToObject();
        

    }

    /// <summary>
    /// Picks a random tile from Resources based on the type of tile
    /// </summary>
    private void AttachSpriteToObject()
    {
        Sprite[] sprites;
        //Currently Loads all the sprites just to pick one, not efficient
        switch (type)
        {
            case TileType.Ground:
                sprites = Resources.LoadAll<Sprite>
                    (GroundSpriteResource);
                tileObject.name = "Ground";
                break;
            case TileType.Wall:
                sprites = Resources.LoadAll<Sprite>
                    (WallSpriteResource);
                tileObject.name = "Wall";
                break;
            default:
                Debug.LogError("Type not defined for Tile.");
                sprites = null;
                break;
        }

        tileObject.GetComponent<SpriteRenderer>().sprite =
            sprites[UnityEngine.Random.Range(0, sprites.Length - 1)];
    }

    #endregion
}
