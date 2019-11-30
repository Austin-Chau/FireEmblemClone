using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _BoardManager : MonoBehaviour
{
    //how wide and tall the level is (does not include the outer walls on the outside)
    public int columns = 12;
    public int rows = 12;
    //An edge to not spawn interactable tiles
    public int edgeWidth = 1;

    //Arrays of prefabs, to be instanced upon board creation
    public GameObject[] floorTiles;
    public GameObject[] outerWallTiles;
    public GameObject[] enemyTiles;

    //Places to store all the tiles to avoid clutter in the hierarchy
    private Transform boardHolder;
    private Transform tileHolder;

    //A place to store all possible positions to place tiles
    private List<Vector3> gridPositions = new List<Vector3>();

    private int enemyCount = 1;

    //Initialize all the possible positions (Besides the outer rim)
    void InitializeList()
    {
        gridPositions.Clear();

        for (int x = edgeWidth; x < columns - edgeWidth; x++)
        {
            for (int y = edgeWidth; y < rows - edgeWidth; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    //Place the outer all and floor, all non-interactable.
    void BoardSetup ()
    {
        for (int x = -1; x < columns + 1; x++)
        {
            for (int y = -1; y < rows + 1; y++)
            {
                GameObject toInstantiate;
                if (x == -1 || x == columns || y == -1 || y == rows)
                {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                }
                else
                {
                    toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                }

                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity, boardHolder) as GameObject;
            }
        }
    }

    //Gets a random position out of all the gridpositions we have instanced
    //removes it from gridPositions if desired
    Vector3 RandomPosition(bool remove)
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        if (remove)
        {
            gridPositions.RemoveAt(randomIndex);
        }
        return randomPosition;
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
        boardHolder = new GameObject("Board").transform;
        tileHolder = new GameObject("BoardTiles").transform;

        BoardSetup();
        InitializeList();
        LayoutObjectsAtRandom(enemyTiles, enemyCount, enemyCount);
    }
}
