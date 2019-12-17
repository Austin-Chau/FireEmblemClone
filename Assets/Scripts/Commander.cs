using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander
{
    public Team Team { get; private set; }

    public List<Unit> Units = new List<Unit>();
    private List<Unit> actableUnits = new List<Unit>(); //a list to keep track of which units still need to move
    public UnitsTracker UnitsTracker { get; private set; }
    public ActionManager ActionManager { get; private set; }
    public bool MyTurn { get; private set; }

    private Unit selectedUnit
    {
        get
        {
            return selectedunit;
        }
        set
        {
            GameManager.instance.GUI.UpdateSelectedUnit(value);
            selectedunit = value;
        }
    }
    private Unit selectedunit;

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
        CursorContext context = _cursorContext;
        List<Tile> restrictedSpaces = new List<Tile>();
        switch (context.inputButton)
        {
            case ControlsEnum.Confirm:
                Debug.Log("confirm pressed");
                if (selectedUnit == null)
                {
                    Debug.Log("No unit currently selected, selecting a new one.");
                    SwitchSelectedUnit(context);
                }
                else if (selectedUnit != null && !selectedUnit.IsPerformingAction())
                {
                    if (selectedUnit.commander == this)
                    {
                        Debug.Log("A unit is currently selected on our team, so we are attempting to parse the tile as an actionspace.");
                        context.lockCursorCallback();
                        ActionManager.ParseTile(selectedUnit, context, SelectedUnitCallback);
                    }
                    else
                    {
                        Debug.Log("The unit selected is not on our team.");
                        if (selectedUnit != context.currentTile.CurrentUnit)
                        {
                            Debug.Log("Unit (possibly null) on this tile is different, time to select a new unit.");
                            SwitchSelectedUnit(context);
                        }
                        else
                        {
                            Debug.Log("The same unit was selected again. Nothing should happen.");
                        }
                    }
                }
                break;

            case ControlsEnum.Reverse:
                Debug.Log("reverse pressed");
                if (selectedUnit != null && !selectedUnit.IsPerformingAction())
                {
                    selectedUnit.RevertMaybeTeleport(selectedUnit.commander == this);
                    selectedUnit = null;
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// A callback for ActionManager that occurs after any actions.
    /// </summary>
    /// <param name="_context"></param>
    /// <returns></returns>
    public void SelectedUnitCallback(bool _unitActionWasPerformed, CursorContext _context)
    {
        Debug.Log("selectedunitfallback has been called");
        if (_unitActionWasPerformed)
        {
            if (selectedUnit.QueryEndOfTurn())
            {
                RetireUnit(selectedUnit);
            }
            selectedUnit = null;
        }
        else
        {
            if (selectedUnit != _context.currentTile.CurrentUnit)
            {
                Debug.Log("Unit on this tile is different, time to select a new unit.");
                SwitchSelectedUnit(_context);
            }
            else
            {
                Debug.Log("The same unit was selected again. Nothing should happen.");
            }
        }
    }

    private void SwitchSelectedUnit(CursorContext _context)
    {
        ParseActionsCallbackContainer parseActionsCallbackContainer = new ParseActionsCallbackContainer(_context.releaseCursorCallback, DeselectUnit);
        Debug.Log("No action space selected/no unit is currently selected. Time to either switch selected units, or deselect completely:");
        if (selectedUnit != null)
            selectedUnit.EraseSpaces();

        selectedUnit = _context.currentTile.CurrentUnit;
        if (selectedUnit == null)
        {
            Debug.Log("> no unit on the tile");
        }
        else if (selectedUnit.commander != this)
        {
            Debug.Log(">the unit on the tile is not ours.");
            selectedUnit.GenerateActSpaces(ActionNames.Move);
        }
        else if (!_context.currentTile.CurrentUnit.Spent)
        {
            Debug.Log(">this is our unit, so we shall start using it.");
            List<ActionNames> possibleActions = selectedUnit.GetAllPossibleActions();
            _context.lockCursorCallback();
            ActionManager.ParseActions(selectedUnit, possibleActions, parseActionsCallbackContainer);
        }
    }

    public void DeselectUnit()
    {
        selectedUnit = null;
    }

    #endregion

    #region Turn Flow
    public void StartTurn()
    {
        MyTurn = true;
        actableUnits = new List<Unit>();
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

    #region Unit Controls
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

};

public struct ParseActionsCallbackContainer
{
    public Action unlockCursorCallback;
    public Action deselectUnit;

    public ParseActionsCallbackContainer(Action _unlockCursorCallback, Action _deselectUnit)
    {
        unlockCursorCallback = _unlockCursorCallback;
        deselectUnit = _deselectUnit;
    }
    public void PerformActions()
    {
        unlockCursorCallback();
        deselectUnit();
    }
}
