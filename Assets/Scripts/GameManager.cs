using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Variables
    public static GameManager instance;
    public BoardManager BoardScript;

    public GameObject UnitPrefab;
    public GameObject CursorPrefab;
    public GameObject GUIManagerPrefab;

    public Camera Camera;
    public Board Board;
    public Cursor Cursor { get; private set; }
    public GUIManager GUIManager { get; private set; }

    public List<Commander> Commanders { get; private set; }

    public Commander CurrentCommander { get; private set; }

    #endregion

    #region Private Variables
    private Dictionary<Commander, List<Unit>> unitRosters = new Dictionary<Commander, List<Unit>>();
    private List<Unit> remainingActableUnits;
    private bool inputLocked;
    private int commanderIndex = 0;
    private bool doingSetup = true;

    private AdjacentDirection persistantInputDirection = AdjacentDirection.None;
    private const int menuTimerMax = 30;
    private const int menuTimerDelay = 5;
    private int menuTimer = menuTimerMax;
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
        GUIManager = Instantiate(GUIManagerPrefab).GetComponent<GUIManager>();
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
        tempCommander.GameManager = this;
        unitRosters[tempCommander] = new List<Unit>();
        Commanders.Add(tempCommander);
        SpawnAndAddUnit(Board.Tiles[0, 0], tempCommander);
        SpawnAndAddUnit(Board.Tiles[0, 1], tempCommander);

        CurrentCommander = tempCommander;

        tempCommander = new Commander(Team.Player2, new PlayerBehavior());
        tempCommander.GameManager = this;
        unitRosters[tempCommander] = new List<Unit>();
        Commanders.Add(tempCommander);
        SpawnAndAddUnit(Board.Tiles[1, 1], tempCommander);

        Cursor.JumpToTile(unitRosters[CurrentCommander][0].currentTile);
        GUIManager.UpdateCurrentTeam(CurrentCommander);
        remainingActableUnits = new List<Unit>();
        foreach(Unit unit in unitRosters[CurrentCommander])
        {
            remainingActableUnits.Add(unit);
            unit.ResetStates();
        }
        CurrentCommander.StartTurn();

        doingSetup = false;
    }

    public void PassTurn()
    {
        Debug.Log("passing turn -------------");
        inputLocked = true;
        CurrentCommander.EndTurn();
        commanderIndex++;
        if (commanderIndex >= Commanders.Count)
        {
            commanderIndex = 0;
        }
        Debug.Log("Now going to be commander " + commanderIndex + "'s turn");
        CurrentCommander = Commanders[commanderIndex];
        Cursor.JumpToTile(unitRosters[CurrentCommander][0].currentTile);
        Camera.MoveToCursor();
        GUIManager.UpdateCurrentTeam(CurrentCommander);
        remainingActableUnits = new List<Unit>();
        foreach (Unit unit in unitRosters[CurrentCommander])
        {
            remainingActableUnits.Add(unit);
            unit.ResetStates();
        }
        CurrentCommander.StartTurn();
        Action callback = () => { inputLocked = false; };
        GUIManager.TurnBanner(CurrentCommander, callback);
    }

    /// <summary>
    /// Removes a unit from the active list of units and checks if it is now empty, signalling the end of a turn. Returns true in that case.
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>True if the turn is ending, false otherwise.</returns>
    public void RetireUnit(Unit _unit)
    {
        if (!remainingActableUnits.Remove(_unit))
            return;

        if (remainingActableUnits.Count < 1)
        {
            //Debug.Log("Retired a unit, and now the commander is out of units.");
            PassTurn();
            return;
        }
        //Debug.Log("Retired a unit, but the commander has more.");
    }

    /// <summary>
    /// Adds a unit to this commanders list of units while setting its spawn tile.
    /// </summary>
    /// <param name="_spawnTile">The tile to spawn at</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool SpawnAndAddUnit(Tile _spawnTile, Commander _commander)
    {
        Unit _unit = Instantiate(UnitPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Unit>();
        if (unitRosters[_commander].Contains(_unit))
        {
            return false;
        }
        unitRosters[_commander].Add(_unit);

        if (CurrentCommander == _commander)
        {
            remainingActableUnits.Add(_unit);
        }

        _unit.InitializeUnit(_spawnTile, _commander);
        return true;
    }

    /// <summary>
    /// Adds a unit to this commanders list of units while not changing its current tile.
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool AddUnit(Unit _unit, Commander _commander)
    { 
        if (unitRosters[_commander].Contains(_unit))
        {
            return false;
        }
        unitRosters[_commander].Add(_unit);

        if (CurrentCommander == _commander)
        {
            remainingActableUnits.Add(_unit);
        }

        _unit.InitializeUnit(_unit.currentTile, _commander);
        return true;
    }

    /// <summary>
    /// Removes a unit from this commander's list of units. 
    /// </summary>
    /// <param name="_unit">Reference to the unit to be removed.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool RemoveUnit(Unit _unit, Commander _commander)
    {
        if (CurrentCommander == _commander && remainingActableUnits.Remove(_unit) && remainingActableUnits.Count < 1)
        {
            Debug.Log("Removed a unit from the current commander's roster, and now the commander is out of units.");
            PassTurn();
            return true;
        }
        _unit.Commander = null;
        return unitRosters[_commander].Remove(_unit);
    }

    public Vector3 CursorPosition()
    {
        return Cursor.transform.position;
    }

    private void Update()
    {
        if (inputLocked)
            return;

        float x = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        float y = Input.GetAxisRaw("Vertical") * Time.deltaTime;
        AdjacentDirection direction = AdjacentDirection.None;

        ControlsEnum pressedInput = ControlsEnum.Null;
        if (Input.GetButtonDown("confirm"))
        {
            pressedInput = ControlsEnum.Confirm;
        }
        else if (Input.GetButtonDown("reverse"))
        {
            pressedInput = ControlsEnum.Reverse;
        }
        else if (Input.GetButtonDown("openMainMenu"))
        {
            pressedInput = ControlsEnum.OpenMainMenu;
        }

        if (Mathf.Abs(x) > Mathf.Epsilon)
            direction = x > 0 ? AdjacentDirection.Right : AdjacentDirection.Left;
        else if (Mathf.Abs(y) > Mathf.Epsilon)
            direction = y > 0 ? AdjacentDirection.Up : AdjacentDirection.Down;

        if (GUIManager.InANavigatableMenu())
        {
            switch (pressedInput)
            {
                case ControlsEnum.Confirm:
                    GUIManager.ActivateCursor();
                    break;
                case ControlsEnum.Reverse:
                    GUIManager.ReverseMenuContainer();
                    break;
                default:
                    GUIManager.MoveCursor(direction);
                    break;
            }
        }
        else if (!Cursor.Moving)
        {
            if (direction != AdjacentDirection.None)
            {
                Cursor.Move(direction);
            }
            else if (pressedInput != ControlsEnum.Null)
            {
                void UnlockInput() { inputLocked = false; }
                void LockInput() { inputLocked = true; }
                CursorContext context = new CursorContext(Cursor.CurrentTile, CurrentCommander, Cursor.CurrentTile.CurrentUnit, pressedInput, UnlockInput, LockInput);
                CurrentCommander.ParseCursorOutput(context);
            }
        }

        switch (pressedInput)
        {
            case ControlsEnum.OpenMainMenu:
                GUIManager.StartMainMenu();
                break;
        }
    }
}
