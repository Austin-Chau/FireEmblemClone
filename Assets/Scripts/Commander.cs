﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParseCommandCallback;

public class Commander
{
    public Team Team { get; private set; }

    public List<Unit> Units = new List<Unit>();
    private List<Unit> actableUnits = new List<Unit>(); //a list to keep track of which units still need to move
    public UnitsTracker UnitsTracker { get; private set; }
    public ActionManager ActionManager { get; private set; }
    public bool MyTurn { get; private set; }

    private Unit SelectedUnit
    {
        get
        {
            return selectedUnit;
        }
        set
        {
            Debug.Log(value);
            GameManager.instance.GUI.SelectedUnit = value;
            selectedUnit = value;
        }
    }
    private Unit selectedUnit;

    /// <summary>
    /// Returns a tuple containing the unit, and the action that is currently expected to be performed.
    /// </summary>
    public Unit CurrentUnit
    {
        get
        {
            return UnitsTracker.Current;
        }
    }

    /// <summary>
    /// Sets the team and behavior of this controller. 
    /// </summary>
    /// <param name="_team">The team</param>
    public Commander(Team _team, ActionManager _actionManager)
    {
        UnitsTracker = new UnitsTracker();
        Team = _team;
        ActionManager = _actionManager;
    }

    #region Cursor Parsing

    /// <summary>
    /// Handles the behavior of the cursor selecting a tile.
    /// </summary>
    /// <param name="_cursorContext"></param>
    /// <returns>Returns whether or not the cursor should be locked.</returns>
    public void ParseCursorOutput(CursorContext _cursorContext)
    {
        List<Tile> restrictedSpaces = new List<Tile>();
        switch (_cursorContext.inputButton)
        {
            case ControlsEnum.Confirm:
                Debug.Log("Confirm has been pressed:");
                Debug.Log(SelectedUnit != null);
                Debug.Log(SelectedUnit != null && !SelectedUnit.IsPerformingAction());
                Debug.Log(SelectedUnit != null && SelectedUnit.commander == this);
                Debug.Log(SelectedUnit != null && SelectedUnit.actionSpaces.ContainsKey(_cursorContext.currentTile));
                if (SelectedUnit != null && //we have a SelectedUnit
                    !SelectedUnit.IsPerformingAction() && //it is not in the middle of an action
                    SelectedUnit.commander == this && //it is our unit
                    SelectedUnit.actionSpaces.ContainsKey(_cursorContext.currentTile)) //the tile we are selecting is an actionspace
                {
                    Debug.Log(">parsing tile as an actionspace.");
                    _cursorContext.lockCursorCallback();
                    ParseTile(SelectedUnit, _cursorContext);
                }
                else
                {
                    Debug.Log(">parsing tile as a unit selection.");
                    SwitchSelectedUnit(_cursorContext);
                }
                break;

            case ControlsEnum.Reverse:
                Debug.Log("Reverse has been pressed.");
                if (SelectedUnit != null && !SelectedUnit.IsPerformingAction())
                {
                    ParseCommand(new ParseCommandPayload().Initialize(CommandNames.Revert, SelectedUnit, null, new Action[] { _cursorContext.releaseCursorCallback }));
                    SelectedUnit = null;
                }
                break;

            default:
                break;
        }
    }

    private void SwitchSelectedUnit(CursorContext _context)
    {
        if (SelectedUnit == _context.currentTile.CurrentUnit)
        {
            Debug.Log("Unit on the tile is the same as SelectedUnit, nothing is done.");
            return;
        }
        _context.lockCursorCallback();
        Debug.Log("Unit on the tile is different than SelectedUnit, time to switch selection.");
        if (SelectedUnit != null)
            ParseCommand(new ParseCommandPayload().Initialize(CommandNames.Cancel, SelectedUnit, null, new Action[] { _context.releaseCursorCallback }));

        SelectedUnit = _context.currentTile.CurrentUnit;
        if (SelectedUnit == null)
        {
            Debug.Log("> no unit on the tile, we are done deselecting.");
            _context.releaseCursorCallback();
            return;
        }
        else if (SelectedUnit.commander != this)
        {
            Debug.Log(">the unit on the tile is not ours, we assume this means the player wants to see the move spaces.");
            ParseCommand(new ParseCommandPayload().Initialize(CommandNames.GenerateMoveSpaces, SelectedUnit, _context.currentTile, new Action[] { _context.releaseCursorCallback }));
        }
        else if (!SelectedUnit.Spent)
        {
            Debug.Log(">this is our unit, and it is not spent, so we shall start using it. Actionmanager shall now pick a command and run it through ParseCommand.");
            List<ActionNames> possibleActions = SelectedUnit.GetAllPossibleActions();
            ParseCommandCallbackContainer parseCommandCallbackContainer = new ParseCommandCallbackContainer(ParseCommand, _context.releaseCursorCallback);
            ActionManager.DecideOnACommand(SelectedUnit, _context.currentTile, possibleActions, parseCommandCallbackContainer);
        }
    }

    /// <summary>
    /// Given the context of a tile, converts this into a command for the unit.
    /// </summary>
    /// <param name="_unit"></param>
    /// <param name="_context"></param>
    public void ParseTile(Unit _unit, CursorContext _context)
    {
        if (!_unit.actionSpaces.ContainsKey(_context.currentTile) || _unit.actionSpaces[_context.currentTile] == null)
        {
            SwitchSelectedUnit(_context);
        }
        else
        {
            Debug.Log("Performing an action space.");
            ParseCommand(new ParseCommandPayload().Initialize(_unit.actionSpaces[_context.currentTile].command, SelectedUnit, _context.currentTile, new Action[] { _context.releaseCursorCallback }));
        }
    }
    #endregion

    #region Turn Flow
    public void StartTurn()
    {
        MyTurn = true;
        actableUnits = new List<Unit>();
        SelectedUnit = null;
        foreach (Unit unit in Units)
        {
            unit.ResetStates();
            actableUnits.Add(unit);
        }
        UnitsTracker.Initialize(Units.ToArray());
    }

    /// <summary>
    /// Called by the child unit to indicate that it has finished its current action and is ready to perform the next.
    /// Checks if the unit's turn should end, and if the controller's turn should end.
    /// </summary>
    public void StepTurn() 
    {

    }

    private void EndTurn()
    {
        Debug.Log(Team + " is ending its turn");
        MyTurn = false;
        GameManager.instance.PassTurn();
    }
    #endregion

    #region Roster Controls
    /// <summary>
    /// Adds a unit to this controllers list of units while setting its spawn tile.
    /// </summary>
    /// <param name="_spawnTile">The tile to spawn at</param>
    /// <param name="_unit">A reference to an instantiated unit monobehavior</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool SpawnAndAddUnit(Tile _spawnTile, Unit _unit)
    {
        if (Units.Contains(_unit))
        {
            return false;
        }
        Units.Add(_unit);
        actableUnits.Add(_unit);
        _unit.InitializeUnit(_spawnTile, Team, this);
        UnitsTracker.UpdateUnits(Units.ToArray());
        return true;
    }

    /// <summary>
    /// Adds a unit to this controllers list of units while not changing its spawn tile.
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool AddUnit(Unit _unit)
    {
        if (Units.Contains(_unit))
        {
            return false;
        }
        Units.Add(_unit);
        actableUnits.Add(_unit);
        _unit.InitializeUnit(_unit.currentTile, Team, this);
        UnitsTracker.UpdateUnits(Units.ToArray());
        return true;
    }

    /// <summary>
    /// Removes a unit from the active list of units and checks if it is now empty, signalling the end of a turn. Returns true in that case.
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>True if the turn is ending, false otherwise.</returns>
    public void RetireUnit(Unit _unit)
    {
        actableUnits.Remove(_unit);
        if (actableUnits.Count < 1)
        {
            Debug.Log("Retired a unit, and now the controller is out of units");
            EndTurn();
            return;
        }
        Debug.Log("Retired a unit, but the controller has more");
    }

    /// <summary>
    /// Removes a unit from this controllers list of units. An indefinite change of which team it is on.
    /// </summary>
    /// <param name="_unit">Reference to the unit to be removed.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool RemoveUnit(Unit _unit)
    {
        actableUnits.Remove(_unit);
        return Units.Remove(_unit);
    }
    #endregion

    #region Command Parsing
    /// <summary>
    /// Converts commands to what the unit should actually do.
    /// </summary>
    public void ParseCommand(ParseCommandPayload _payload)
    {
        Action<Unit> actionFinishedCallback;
        switch (_payload.commandName)
        {
            case CommandNames.Move:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit, true); _payload.PerformCallbacks(); };
                _payload.actingUnit.PerformAction(CommandsToActions[CommandNames.Move], _payload.targetTile, actionFinishedCallback);
                return;
            case CommandNames.InitializeMove:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit, false); _payload.PerformCallbacks(); };
                _payload.actingUnit.GenerateActSpaces(ActionNames.Move);
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.GenerateMoveSpaces:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit, false); _payload.PerformCallbacks(); };
                _payload.actingUnit.GenerateActSpaces(ActionNames.Move);
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Attack:
                return;
            case CommandNames.InitializeAttack:
                return;
            case CommandNames.EndTurn:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit, false); _payload.PerformCallbacks(); };
                _payload.actingUnit.EndActions();
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Cancel:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit, false); _payload.PerformCallbacks(); };
                _payload.actingUnit.EraseSpaces();
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Revert:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit, false); _payload.PerformCallbacks(); };
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
    /// Called after a unit finishes their action.
    /// </summary>
    public void SucceededAction(Unit _unit, bool _deselectAfterAction)
    {
        Debug.Log("successful action");
        if (_unit.QueryEndOfTurn())
        {
            RetireUnit(_unit);
        }
        if (_deselectAfterAction && SelectedUnit == _unit)
        {
            SelectedUnit = null;
        }
    }

    #endregion

};