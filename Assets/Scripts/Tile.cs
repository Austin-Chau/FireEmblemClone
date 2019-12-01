using System.Collections;
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
        type = _type;
        CreateTile();
        tileObject.transform.position = pos;
        tileObject.transform.parent = parent;
        if (_type == TileType.Ground) MovementWeight = 1;
        else MovementWeight = int.MaxValue;
    }

    #region Private Variables

    TileType type;

    GameObject tileObject;

    //Four adjacent tiles based on cardinal directions for pathfinding
    Dictionary<AdjacentDirection, Tile> AdjacentTiles = new Dictionary<AdjacentDirection, Tile>();

    //Amount of movement it takes to get to the tile
    //MAX_INT if impassable.
    int MovementWeight;

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
        tileObject = Object.Instantiate(
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
            sprites[Random.Range(0, sprites.Length - 1)];
    }

    #endregion
}
