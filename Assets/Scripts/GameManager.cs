using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ParseCommandCallback;

public class GameManager : MonoBehaviour
{
    #region Public Variables
    public static GameManager instance;
    public BoardManager BoardScript;

    public GameObject UnitPrefab;
    public GameObject CursorPrefab;
    public GameObject GUIManagerPrefab;

    public CameraManager CameraManager;
    public Board Board;
    public Cursor Cursor { get; private set; }
    public GUIManager GUIManager { get; private set; }

    public List<Commander> Commanders { get; private set; }

    public Commander CurrentCommander { get; private set; }

    public bool[,] BoardRockData;

    public GameStates CurrentGameState
    {
        get
        {
            return currentGameState;
        }
        private set
        {
            currentGameState = value;
        }
    }

    #endregion

    #region Private Variables
    private Dictionary<Commander, List<Unit>> unitRosters = new Dictionary<Commander, List<Unit>>();
    private List<Unit> remainingActableUnits;
    private bool inputLocked;
    private int commanderIndex = 0;
    private bool doingSetup = true;
    private GameStates currentGameState;

    private AdjacentDirection persistantInputDirection = AdjacentDirection.None;
    private const int menuTimerMax = 30;
    private const int menuTimerDelay = 5;
    private int menuTimer = menuTimerMax;

    private Unit SelectedUnit
    {
        get
        {
            return selectedUnit;
        }
        set
        {
            GUIManager.SelectedUnit = value;
            selectedUnit = value;
        }
    }
    private Unit selectedUnit;

    /// <summary>
    /// The unit that is currently creating its movementpath. Used while the gamemanager is in the state "UnitPathCreation."
    /// </summary>
    private Unit MovingUnit;

    private Func<AdjacentDirection, bool> checkIfCursorMovementLegal;

    private WinConditions currentWinCondition = WinConditions.Rout;
    private Dictionary<Commander, Commander> opponents = new Dictionary<Commander, Commander>();

    private Dictionary<Commander, Dictionary<Unit, bool>> routRoster = new Dictionary<Commander, Dictionary<Unit, bool>>(); //true if dead, false otherwise
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
    }

    private void Start()
    {
        InitGame();
    }

    /// <summary>
    /// Initializes the game, currently only creates the board.
    /// </summary>
    void InitGame()
    {
        doingSetup = true;
        BoardRockData = new bool[12, 12];

        //reset old game state
        foreach (KeyValuePair<Commander, List<Unit>> pair in unitRosters)
        {
            foreach (Unit unit in pair.Value)
            {
                unit.DeleteGameObjects();
            }
        }

        opponents = new Dictionary<Commander, Commander>();
        unitRosters = new Dictionary<Commander, List<Unit>>();
        routRoster = new Dictionary<Commander, Dictionary<Unit, bool>>();
        if (Board != null)
        {
            Board.DeleteGameObjects();
        }
        Board = null;
        Board.Instance = null;

        Commanders = new List<Commander>();
        CurrentCommander = null;
        commanderIndex = 0;

        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("test");
        //end reset of old game state

        Board = BoardScript.SetupScene();
        Commander tempCommander = new Commander(Team.Player1, new PlayerBehavior());
        tempCommander.GameManager = this;

        unitRosters[tempCommander] = new List<Unit>();
        routRoster[tempCommander] = new Dictionary<Unit, bool>();
        Commanders.Add(tempCommander);
        SpawnAndAddUnit(Board.Tiles[1, 1], tempCommander);
        SpawnAndAddUnit(Board.Tiles[3, 3], tempCommander);

        CurrentCommander = tempCommander;

        tempCommander = new Commander(Team.Player2, new PlayerBehavior());
        tempCommander.GameManager = this;
        unitRosters[tempCommander] = new List<Unit>();
        routRoster[tempCommander] = new Dictionary<Unit, bool>();
        Commanders.Add(tempCommander);
        SpawnAndAddUnit(Board.Tiles[6, 6], tempCommander);

        opponents[tempCommander] = CurrentCommander;
        opponents[CurrentCommander] = tempCommander;

        Cursor.JumpToTile(unitRosters[CurrentCommander][0].currentTile);
        GUIManager.UpdateCurrentTeam(CurrentCommander);
        remainingActableUnits = new List<Unit>();
        foreach (Unit unit in unitRosters[CurrentCommander])
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
        CameraManager.MoveToCursor();
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
    /// Registers that a unit has died in the gamestate (routroster) for detection by the win condition.
    /// </summary>
    /// <param name="_unit"></param>
    public void ReportUnitDeath(Unit _unit)
    {
        routRoster[_unit.Commander][_unit] = true;
        if (currentWinCondition != WinConditions.Rout)
        {
            return;
        }

        bool cumulative = true;

        foreach (KeyValuePair<Unit, bool> pair in routRoster[_unit.Commander])
        {
            cumulative &= pair.Value;
        }

        if (cumulative)
        {
            EndGame(opponents[_unit.Commander]);
        }
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
        routRoster[_commander][_unit] = false;

        if (CurrentCommander == _commander)
        {
            remainingActableUnits.Add(_unit);
        }

        UnitStats stats = new UnitStats(20, 20, 2, 3);

        _unit.InitializeUnit(_spawnTile, _commander, stats);
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
        routRoster[_commander][_unit] = false;

        if (CurrentCommander == _commander)
        {
            remainingActableUnits.Add(_unit);
        }


        UnitStats stats = new UnitStats(5, 20, 2, 3);

        _unit.InitializeUnit(_unit.currentTile, _commander, stats);
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
        routRoster[_commander].Remove(_unit);
        return unitRosters[_commander].Remove(_unit);
    }

    public Vector3 CursorPosition()
    {
        return Cursor.transform.position;
    }

    private void Update()
    {
        //First, prime all the variables for input interpretation
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
        else if (Input.GetButtonDown("rotate"))
        {
            pressedInput = ControlsEnum.Rotate;
        }

        if (Mathf.Abs(x) > Mathf.Epsilon)
            direction = x > 0 ? AdjacentDirection.Right : AdjacentDirection.Left;
        else if (Mathf.Abs(y) > Mathf.Epsilon)
            direction = y > 0 ? AdjacentDirection.Up : AdjacentDirection.Down;


        switch (currentGameState) //The grand hierarchy of what inputs do what and with what priority
        {
            case GameStates.GUIMenuing: //in a GUI menu, only the GUIManager should be doing things
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
                return;

            case GameStates.UnitPathCreation: //A unit on the field is creating their movement path, only that should be interacted with
                if (!Cursor.Moving && direction != AdjacentDirection.None)
                {
                    if (MovingUnit.StepPathCreationTranslate(direction))
                    {
                        Cursor.Move(direction);
                        cursorHasMoved(direction);
                    }
                    return;
                }
                switch (pressedInput)
                {
                    case ControlsEnum.Confirm:
                        MovingUnit.PerformAction(ActionNames.Move,null,(Unit _unit)=> { EndGameState(); });
                        break;
                    case ControlsEnum.Rotate:
                        bool clockwise;
                        if (Input.GetAxisRaw("rotate") >= 0)
                        {
                            clockwise = true;
                        }
                        else
                        {
                            clockwise = false;
                        }
                        if (MovingUnit.StepPathCreationRotation(clockwise))
                        {
                            //meep
                        }
                        break;
                }
                return;

            case GameStates.None: //nothing interrupting is going on
                //No game state is currently active, so cursor movement should be allowed. after which we then listen to other button presses
                if (!Cursor.Moving && direction != AdjacentDirection.None)
                {
                    Cursor.Move(direction);
                    cursorHasMoved(direction);
                    return;
                }
                switch (pressedInput)
                {
                    //PORTED FROM COMMANDER
                    case ControlsEnum.Confirm:
                        Debug.Log("Confirm has been pressed:");
                        if (SelectedUnit != null && //we have a SelectedUnit
                            !SelectedUnit.IsPerformingAction() && //it is not in the middle of an action
                            SelectedUnit.Commander == CurrentCommander && //it is our unit
                            SelectedUnit.actionSpaces.ContainsKey(Cursor.CurrentTile)) //the tile we are selecting is an actionspace
                        {
                            Debug.Log(">parsing tile as an actionspace.");
                            ParseTile(SelectedUnit);
                        }
                        else
                        {
                            Debug.Log(">parsing tile as a unit selection.");
                            SwitchSelectedUnit();
                        }
                        break;
                    case ControlsEnum.Reverse:
                        Debug.Log("Reverse has been pressed.");
                        if (SelectedUnit != null && !SelectedUnit.IsPerformingAction())
                        {
                            ParseCommandPayload payload = new ParseCommandPayload(
                                CommandNames.Cancel, //command is cancel
                                SelectedUnit, //the previously selected unit
                                null, //no tile needed
                                new Action[] { () => { SetInputLocked(false); } }); //unlock the cursor
                            ParseCommand(payload);
                            DeselectUnit();
                        }
                        break;
                    //^^ PORTED FROM COMMANDER
                    case ControlsEnum.OpenMainMenu:
                        ChangeGameState(GameStates.GUIMenuing);
                        GUIManager.StartMainMenu();
                        break;
                }
                break;
        }

    }

    //PORTED FROM COMMANDER
    /// <summary>
    /// Switches what the SelectedUnit is, to what unit is on the tile the cursor is currently on
    /// </summary>
    private void SwitchSelectedUnit()
    {
        Debug.Log("switching selected units from " + SelectedUnit);
        if (SelectedUnit == Cursor.CurrentTile.CurrentUnit)
        {
            Debug.Log("Unit on the tile is the same as SelectedUnit.");
            //Debug.Log("Unit on the tile is the same as SelectedUnit, nothing is done.");
            //return;
        }
        Debug.Log("Switching selection.");
        if (SelectedUnit != null)
        {
            ParseCommandPayload payload = new ParseCommandPayload(
                CommandNames.Cancel, //command is cancel
                SelectedUnit, //the previously selected unit
                null, //no tile needed
                new Action[] {}); //unlock the cursor
            ParseCommand(payload);
        }

        SelectedUnit = Cursor.CurrentTile.CurrentUnit;
        if (SelectedUnit == null)
        {
            Debug.Log("> no unit on the tile, we are done deselecting.");
            return;
        }
        else if (SelectedUnit.Commander != CurrentCommander)
        {
            Debug.Log(">the unit on the tile is not ours, we assume this means the player wants to see the move spaces.");
            ParseCommand(
                new ParseCommandPayload(
                    CommandNames.GenerateMoveSpaces,
                    SelectedUnit,
                    Cursor.CurrentTile,
                    new Action[] {})
                );
        }
        else if (!SelectedUnit.Spent)
        {
            Debug.Log(">this is our unit, and it is not spent, so we shall start using it. Actionmanager shall now pick a command and run it through ParseCommand. If this is a player commander, a menu shall pop up instead.");
            List<ActionNames> possibleActions = SelectedUnit.GetAllPossibleActions();
            CurrentCommander.ActionManager.DecideOnACommand(SelectedUnit, Cursor.CurrentTile, possibleActions, ParseCommand, EndGameState);
        }
    }

    /// <summary>
    /// Interprets what to do for any given command payload
    /// </summary>
    /// <param name="_payload"></param>
    public void ParseCommand(ParseCommandPayload _payload)
    {
        Action<Unit> actionFinishedCallback;
        switch (_payload.commandName)
        {
            case CommandNames.Move:
                actionFinishedCallback = (_unit) => { DeselectUnit(); _payload.PerformCallbacks(); CheckIfUnitFinishedTurn(_unit); };
                _payload.actingUnit.PerformAction(CommandsToActions[CommandNames.Move], _payload.targetTile, actionFinishedCallback);
                return;
            case CommandNames.InitializeMove:
                actionFinishedCallback = (_unit) => { _payload.PerformCallbacks(); };
                Action<Unit> unitActionFinishedCallback = (_unit) => { DeselectUnit(); _payload.PerformCallbacks(); CheckIfUnitFinishedTurn(_unit); };
                //_payload.actingUnit.GenerateActSpaces(ActionNames.Move);
                _payload.actingUnit.InitializePathCreation(actionFinishedCallback);
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.GenerateMoveSpaces:
                actionFinishedCallback = (_unit) => { _payload.PerformCallbacks(); };
                _payload.actingUnit.GenerateActSpaces(ActionNames.Move);
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Attack:
                actionFinishedCallback = (_unit) => { DeselectUnit(); _payload.PerformCallbacks(); CheckIfUnitFinishedTurn(_unit); };
                _payload.actingUnit.PerformAction(CommandsToActions[CommandNames.Attack], _payload.targetTile, actionFinishedCallback);
                return;
            case CommandNames.InitializeAttack:
                actionFinishedCallback = (_unit) => { _payload.PerformCallbacks(); };
                _payload.actingUnit.GenerateActSpaces(ActionNames.Attack);
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.EndTurn:
                actionFinishedCallback = (_unit) => { DeselectUnit(); _payload.PerformCallbacks(); CheckIfUnitFinishedTurn(_unit); };
                _payload.actingUnit.EndActions();
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Cancel:
                actionFinishedCallback = (_unit) => { DeselectUnit(); _payload.PerformCallbacks(); };
                _payload.actingUnit.EraseSpaces();
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Revert:
                actionFinishedCallback = (_unit) => { DeselectUnit(); _payload.PerformCallbacks(); };
                _payload.actingUnit.EraseSpaces();
                actionFinishedCallback(_payload.actingUnit);
                return;
            default:
                return;
        }
    }

    /// <summary>
    /// A dictionary to help facilitate the interpretation of commands into actions for the unit.
    /// </summary>
    private Dictionary<CommandNames, ActionNames> CommandsToActions = new Dictionary<CommandNames, ActionNames>
    {
        {CommandNames.Move, ActionNames.Move },
        {CommandNames.Attack, ActionNames.Attack }
    };

    /// <summary>
    /// Deselects the currently selected unit
    /// </summary>
    public void DeselectUnit()
    {
        //Debug.Log("The currently selected unit has been deslected by GameManager.");
        SelectedUnit = null;
    }

    /// <summary>
    /// (when interacting with a unit falls through), causes the cursor to interact directly with a tile.
    /// </summary>
    /// <param name="_unit"></param>
    /// <param name="_context"></param>
    public void ParseTile(Unit _unit)
    {
        if (!_unit.actionSpaces.ContainsKey(Cursor.CurrentTile) || _unit.actionSpaces[Cursor.CurrentTile] == null || _unit.actionSpaces[Cursor.CurrentTile].Invalid)
        {
            SwitchSelectedUnit();
        }
        else
        {
            Debug.Log("Performing an action space.");
            SetInputLocked(true);

            ParseCommand(new ParseCommandPayload(
                _unit.actionSpaces[Cursor.CurrentTile].command,
                SelectedUnit,
                Cursor.CurrentTile,
                new Action[] {}
                ));
        }
    }

    //TODO: possibly remove the inputlocked functionality, and just have the states be robust enough
    /// <summary>
    /// Locks the entirety of input
    /// </summary>
    /// <param name="_value"></param>
    private void SetInputLocked(bool _value)
    {
        inputLocked = _value;
    }

    /// <summary>
    /// Called after a unit finishes their action.
    /// </summary>
    public void CheckIfUnitFinishedTurn(Unit _unit)
    {
        if (_unit.QueryEndOfTurn())
        {
            RetireUnit(_unit);
        }
    }

    //^^ PORTED FROM COMMANDER

    /// <summary>
    /// Ends the entire game and declares the given victor
    /// </summary>
    /// <param name="victor"></param>
    private void EndGame(Commander victor)
    {
        GUIManager.VictoryBanner(victor, InitGame);
    }

    /// <summary>
    /// Changes the gamestate
    /// </summary>
    /// <param name="_gameState"></param>
    public void ChangeGameState(GameStates _gameState)
    {
        currentGameState = _gameState;
    }
    /// <summary>
    /// Ends whatever gamestate is currently going on
    /// </summary>
    public void EndGameState()
    {
        Debug.Log("Ending GameState");
        switch (currentGameState)
        {
            case GameStates.UnitPathCreation:
                MovingUnit = null;
                break;
        }
        currentGameState = GameStates.None;

    }

    /// <summary>
    /// Starts a unit on creating a movement path
    /// </summary>
    /// <param name="_unit"></param>
    public void StartUnitMovement(Unit _unit)
    {
        ChangeGameState(GameStates.UnitPathCreation);
        MovingUnit = _unit;
    }

    /// <summary>
    /// Called after the cursor has successfully moved
    /// </summary>
    /// <param name="_direction"></param>
    public void cursorHasMoved(AdjacentDirection _direction) //called after a successful cursor movement
    {
        Tile _destinationTile = Cursor.CurrentTile.GetAdjacentTile(_direction);
        GUIManager.UpdateSelectedTile(_destinationTile);

        switch (currentGameState)
        {
            case GameStates.UnitPathCreation:
                break;
            case GameStates.None:
                GUIManager.UpdateHoveredUnit(_destinationTile.CurrentUnit);
                break;
        }
    }
}
