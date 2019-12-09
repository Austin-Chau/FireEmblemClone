using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _GameManager : MonoBehaviour
{
    //a place to store the singular game manager, can be checked against so a competing one is not instanced
    public static _GameManager instance = null;

    public _BoardManager boardScript;

    public Board board;
    public Cursor Cursor;

    //Various bools used by the manager to make sure states are transitioned properly
    public bool playersTurn = true;
    public bool moving = false;
    public Vector2 playerPosition = new Vector2(0, 0);
    public Vector3 cursorPosition = new Vector3(0,0,0);

    public bool panCamera = true;

    //How long between each NPC movement
    public float turnDelay = .1f;

    private bool doingSetup = true;
    private List<Enemy> enemies;

    private Player playerController;
    private List<Enemy> enemyController;

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

        enemies = new List<Enemy>();
        boardScript = GetComponent<_BoardManager>();
        InitGame();
    }

    //Actually setting up the level. Can be called multiple times, after clearing the level.
    void InitGame()
    {
        doingSetup = true;
        board = boardScript.SetupScene();
        doingSetup = false;
    }

    public void GameOver()
    {
        //for when the player dies or otherwise the game ends
    }

    void Update()
    {
        //game manager twiddles its thumbs while enemies are not supposed to move
        if (playersTurn || doingSetup || moving)
        {
            return;
        }
        StartCoroutine(MoveEnemies());
    }

    //enemies move one by one
    IEnumerator MoveEnemies()
    {
        moving = true;
        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }
        else
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Move();
                while (enemies[i].moving)
                {
                    yield return null;
                }
                enemies[i].acted = false;
                enemies[i].moved = false;
                yield return new WaitForSeconds(turnDelay);
            }
        }
        moving = false;
        playersTurn = true;
    }
}
