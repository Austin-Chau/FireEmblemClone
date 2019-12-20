using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Variables
    public static GameManager instance;
    public BoardManager BoardScript;

    public GameObject UnitPrefab;
    public GameObject CursorPrefab;
    public GameObject GUIPrefab;

    public Board Board;
    public Cursor Cursor { get; private set; }
    public GUI GUI { get; private set; }
    public Vector3 cursorPosition = new Vector3(0, 0, 0);
    public List<Commander> Commanders { get; private set; }

    public Commander CurrentCommander { get; private set; }

    #endregion

    #region Private Variables
    private int commanderIndex = 0;
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
        BoardScript = GetComponent<BoardManager>();
        Commanders = new List<Commander>();
        Cursor = Instantiate(CursorPrefab).GetComponent<Cursor>();
        GUI = Instantiate(GUIPrefab).GetComponent<GUI>();
        InitGame();
    }

    /// <summary>
    /// Initializes the game, currently only creates the board.
    /// </summary>
    void InitGame()
    {
        doingSetup = true;

        Board = BoardScript.SetupScene();
        Commander tempCommander = new Commander(Team.Player1, new PlayerBehavior());
        Commanders.Add(tempCommander);
        tempCommander.SpawnAndAddUnit(Board.Tiles[0, 0], Instantiate(UnitPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Unit>());
        tempCommander.SpawnAndAddUnit(Board.Tiles[1, 3], Instantiate(UnitPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Unit>());
        CurrentCommander = tempCommander;

        tempCommander = new Commander(Team.Player2, new PlayerBehavior());
        Commanders.Add(tempCommander);
        tempCommander.SpawnAndAddUnit(Board.Tiles[5,5], Instantiate(UnitPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Unit>());

        Cursor.CurrentCommander = CurrentCommander;
        Cursor.JumpToTile(CurrentCommander.Units[0].currentTile);
        GUI.UpdateCurrentTeam(CurrentCommander);
        CurrentCommander.StartTurn();

        doingSetup = false;
    }

    public void PassTurn()
    {
        Debug.Log("passing turn -------------");
        Debug.Log(commanderIndex);
        commanderIndex++;
        if (commanderIndex >= Commanders.Count)
        {
            commanderIndex = 0;
        }
        Debug.Log(commanderIndex);
        CurrentCommander = Commanders[commanderIndex];
        Debug.Log(CurrentCommander.Team);
        Cursor.UnlockCursor();
        Cursor.CurrentCommander = CurrentCommander;
        Cursor.JumpToTile(CurrentCommander.Units[0].currentTile);
        GUI.UpdateCurrentTeam(CurrentCommander);
        CurrentCommander.StartTurn();
    }

}
