using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParseCommandCallback;

public class Commander
{
    public Team Team { get; private set; }
    public GameManager GameManager;
    public List<Unit> Units = new List<Unit>();
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
            GameManager.instance.GUI.SelectedUnit = value;
            selectedUnit = value;
        }
    }
    private Unit selectedUnit;

    /// <summary>
    /// Sets the team and behavior of this controller. 
    /// </summary>
    /// <param name="_team">The team</param>
    public Commander(Team _team, ActionManager _actionManager)
    {
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
                /*
                Debug.Log(SelectedUnit != null);
                Debug.Log(SelectedUnit != null && !SelectedUnit.IsPerformingAction());
                Debug.Log(SelectedUnit != null && SelectedUnit.commander == this);
                Debug.Log(SelectedUnit != null && SelectedUnit.actionSpaces.ContainsKey(_cursorContext.currentTile));
                */
                if (SelectedUnit != null && //we have a SelectedUnit
                    !SelectedUnit.IsPerformingAction() && //it is not in the middle of an action
                    SelectedUnit.Commander == this && //it is our unit
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
                    ParseCommand(new ParseCommandPayload(CommandNames.Revert, SelectedUnit, null, new Action[] { _cursorContext.releaseCursorCallback }, new object[0]));
                    SelectedUnit = null;
                }
                break;

            default:
                break;
        }
    }

    private void SwitchSelectedUnit(CursorContext _context)
    {
        Debug.Log("switching selected units from "+ SelectedUnit);
        if (SelectedUnit == _context.currentTile.CurrentUnit)
        {
            Debug.Log("Unit on the tile is the same as SelectedUnit, nothing is done.");
            return;
        }
        _context.lockCursorCallback();
        Debug.Log("Unit on the tile is different than SelectedUnit, time to switch selection.");
        if (SelectedUnit != null)
            ParseCommand(new ParseCommandPayload(CommandNames.Cancel, SelectedUnit, null, new Action[] { _context.releaseCursorCallback }, new object[0]));

        SelectedUnit = _context.currentTile.CurrentUnit;
        if (SelectedUnit == null)
        {
            Debug.Log("> no unit on the tile, we are done deselecting.");
            _context.releaseCursorCallback();
            return;
        }
        else if (SelectedUnit.Commander != this)
        {
            Debug.Log(">the unit on the tile is not ours, we assume this means the player wants to see the move spaces.");
            ParseCommand(new ParseCommandPayload(CommandNames.GenerateMoveSpaces, SelectedUnit, _context.currentTile, new Action[] { _context.releaseCursorCallback }, new object[0]));
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
            ParseCommand(new ParseCommandPayload(_unit.actionSpaces[_context.currentTile].command, SelectedUnit, _context.currentTile, new Action[] { _context.releaseCursorCallback }, new object[0]));
        }
    }
    #endregion

    #region Turn Flow
    public void StartTurn()
    {
        MyTurn = true;
        SelectedUnit = null;
    }

    public void EndTurn()
    {
        MyTurn = false;
        SelectedUnit = null;
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
                actionFinishedCallback = (_unit) => { SucceededAction(_unit); DeselectUnit();  _payload.PerformCallbacks(); };
                _payload.actingUnit.PerformAction(CommandsToActions[CommandNames.Move], _payload.targetTile, actionFinishedCallback);
                return;
            case CommandNames.InitializeMove:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit); _payload.PerformCallbacks(); };
                _payload.actingUnit.GenerateActSpaces(ActionNames.Move);
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.GenerateMoveSpaces:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit); _payload.PerformCallbacks(); };
                _payload.actingUnit.GenerateActSpaces(ActionNames.Move);
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Attack:
                return;
            case CommandNames.InitializeAttack:
                return;
            case CommandNames.EndTurn:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit); DeselectUnit();  _payload.PerformCallbacks(); };
                _payload.actingUnit.EndActions();
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Cancel:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit); _payload.PerformCallbacks(); };
                _payload.actingUnit.EraseSpaces();
                actionFinishedCallback(_payload.actingUnit);
                return;
            case CommandNames.Revert:
                actionFinishedCallback = (_unit) => { SucceededAction(_unit); _payload.PerformCallbacks(); };
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
    public void SucceededAction(Unit _unit)
    {
        if (_unit.QueryEndOfTurn())
        {
            GameManager.RetireUnit(_unit);
        }
    }

    public void DeselectUnit()
    {
        SelectedUnit = null;
    }

    #endregion

};
