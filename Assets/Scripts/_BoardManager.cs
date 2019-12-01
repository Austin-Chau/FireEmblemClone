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
    
    //Gets a random position out of all the gridpositions we have instanced
    //removes it from gridPositions if desired
    Vector2 RandomPosition(bool remove)
    {
        int randomY = Random.Range(0, board.columns);
        int randomX = Random.Range(0, board.rows);
        if (!board.IsTileOccupied(randomX, randomY))
            return new Vector2(randomX, randomY);
        else return Vector2.zero;
    }

    //Populates the active field with from the desired array within the given range
    void LayoutObjectsAtRandom(GameObject[] tileArray, int min, int max)
    {
        int count = Random.Range(min, max + 1);
        for (int i = 0; i < count; i++) 
        {
            Vector3 randomPosition = RandomPosition(true);
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    public void SetupScene()
    {
        board = new Board(rows, columns);
        
        LayoutObjectsAtRandom(enemyTiles, enemyCount, enemyCount);
    }
}
