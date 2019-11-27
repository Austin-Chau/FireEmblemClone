using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _GameManager : MonoBehaviour
{
    //a place to store the singular game manager, can be checked against so a competing one is not instanced
    public static _GameManager instance = null;

    public _BoardManager boardScript;

    //Various bools used by the manager to make sure states are transitioned properly
    public bool playersTurn = true;
    public bool objectsMoving = false;
    public bool turnSwitching = false;

    //Controls stuff
    public bool panCamera = true;

    //How long between each enemy movement
    public float turnDelay = .1f;

    private bool doingSetup = true;
    //private List<_Enemy> enemies;


    //Preinitialization stuff
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        //enemies = new List<Enemy>();
        boardScript = GetComponent<_BoardManager>();
        InitGame();
    }

    //Actually setting up the level. Can be called multiple times.
    void InitGame()
    {
        doingSetup = true;
        //enemies.Clear();
        boardScript.SetupScene();
        doingSetup = false;
    }

    public void GameOver()
    {
        //for when the player dies or otherwise the game ends
    }

    void Update()
    {
        //game manager twiddles its thumbs while enemies are not supposed to move
        if (playersTurn || doingSetup || objectsMoving)
        {
            return;
        }
        StartCoroutine(MoveEnemies());
    }

    //pass an instance of each enemy object (done by themselves)
    /*
    public void RegisterEnemy(Enemy script)
    {
        enemies.Add(script);
    }
    */
    IEnumerator MoveEnemies()
    {
        //objectsMoving = true;
        yield return new WaitForSeconds(turnDelay);
        /*
        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        */
        playersTurn = true;
        //objectsMoving = false;
    }
}
