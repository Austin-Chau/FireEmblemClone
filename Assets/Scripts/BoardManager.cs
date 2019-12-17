using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //how wide and tall the level is (does not include the outer walls on the outside)
    public int columns = 12;
    public int rows = 12;

    Board board;

    private Tile[,] Tiles;

    /// <summary>
    /// Returns a random position from the current board size.
    /// </summary>
    /// <returns>The position.</returns>
    Vector2 RandomPosition()
    {
        int randomY = Random.Range(0, board.columns);
        int randomX = Random.Range(0, board.rows);
        return new Vector2(randomX, randomY);
    }

    //Populates the active field with from the desired array within the given range
    List<GameObject> LayoutObjectsAtRandom(GameObject[] tileArray, int min, int max)
    {
        List < GameObject > createdObjects = new List<GameObject>();
        int count = Random.Range(min, max + 1);
        for (int i = 0; i < count; i++) 
        {
            Tile tile = GetRandomUnoccupiedTile();
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            GameObject obj = Instantiate(tileChoice, tile.Position, Quaternion.identity);
            tile.CurrentUnit = obj.GetComponent<Unit>();
            createdObjects.Add(obj);

        }
        return createdObjects;
    }

    public Tile GetRandomUnoccupiedTile()
    {
        Vector3 randomPosition = RandomPosition();
        while (board.IsTileOccupied((int)randomPosition.x, (int)randomPosition.y))
        {
            randomPosition = RandomPosition();
        }
        Tile tileChoice = Tiles[(int)randomPosition.x, (int)randomPosition.y];
        return tileChoice;
    }

    public Board SetupScene()
    {
        board = new Board(rows, columns);
        Tiles = board.Tiles;
        return board;
    }
}
