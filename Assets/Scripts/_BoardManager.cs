using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _BoardManager : MonoBehaviour
{
    //how wide and tall the level is (does not include the outer walls on the outside)
    public int columns = 12;
    public int rows = 12;

    public GameObject[] enemyTiles;
    Board board;

    private int enemyCount = 1;
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
    void LayoutObjectsAtRandom(GameObject[] tileArray, int min, int max)
    {
        int count = Random.Range(min, max + 1);
        for (int i = 0; i < count; i++) 
        {
            Vector3 randomPosition = RandomPosition();
            if (board.IsTileOccupied((int)randomPosition.x, (int)randomPosition.y))
            {
                randomPosition = Vector3.zero;
            }
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            Tiles[(int)randomPosition.x, (int)randomPosition.y].Occupied = true;
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    public Board SetupScene()
    {
        board = new Board(rows, columns);
        Tiles = board.Tiles;

        LayoutObjectsAtRandom(enemyTiles, enemyCount, enemyCount);
        return board;
    }
}
