using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _GameManager : MonoBehaviour
{
    #region Public Variables
    public static _GameManager instance;
    public _BoardManager BoardScript;
    public GameObject Unit;
    public Board Board;
    public Cursor Cursor;
    public Vector3 cursorPosition = new Vector3(0, 0, 0);
    public List<Controller> Controllers { get; private set; }
    public Controller PlayerController;
    public Controller EnemyController;

    public Controller CurrentController { get; private set; }

    #endregion

    #region Private Variables
    private int controllerIndex = 0;
    private bool doingSetup = true;
    #endregion

    #region Constants
    const int enemyCount = 2;
    #endregion

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
        BoardScript = GetComponent<_BoardManager>();
        Controllers = new List<Controller>();
        InitGame();
    }

    /// <summary>
    /// Initializes the game, currently only creates the board.
    /// </summary>
    void InitGame()
    {
        doingSetup = true;

        Board = BoardScript.SetupScene(EnemyController);
        PlayerController = new Controller(Team.Player, new PlayerBehavior());
        PlayerController.SpawnAndAddUnit(Board.Tiles[0, 0], Instantiate(Unit,new Vector3(0,0,0),Quaternion.identity).GetComponent<Unit>());
        Controllers.Add(PlayerController);

        EnemyController = new Controller(Team.Enemy, new BasicEnemy());
        for (int i = 0; i < enemyCount; i++)
        {
            Tile randomTile = BoardScript.GetRandomUnoccupiedTile();
            GameObject unit = Instantiate(Unit, randomTile.Position, Quaternion.identity);
            EnemyController.SpawnAndAddUnit(randomTile, unit.GetComponent<Unit>());
        }
        Controllers.Add(EnemyController);

        CurrentController = PlayerController;
        CurrentController.PerformTurn();

        doingSetup = false;
    }

    public void PassTurn()
    {
        Debug.Log("passing turn -------------");
        Debug.Log(controllerIndex);
        controllerIndex++;
        if (controllerIndex >= Controllers.Count)
        {
            controllerIndex = 0;
        }
        Debug.Log(controllerIndex);
        CurrentController = Controllers[controllerIndex];
        Debug.Log(CurrentController.Team);
        CurrentController.PerformTurn();
    }

}
