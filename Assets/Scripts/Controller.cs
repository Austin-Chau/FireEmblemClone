using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander
{
    public Team Team { get; private set; }

    private List<Unit> Units = new List<Unit>();
    private List<Unit> actableUnits = new List<Unit>(); //a list to keep track of which units still need to move
    public UnitsTracker UnitsTracker { get; private set; }
    public ActionManager ActionManager { get; private set; }
    public bool MyTurn { get; private set; }

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
    /// Handles the behavior of what units should do when the cursor selects them.
    /// </summary>
    /// <param name="_cursorContext"></param>
    /// <returns>Returns what the currently selected unit should be set to.</returns>
    public CommanderParseCursorOutput ParseCursorOutput(CursorContext _cursorContext)
    {
        CursorContext context = _cursorContext;
        List<Tile> restrictedSpaces = new List<Tile>();
        switch (context.inputButton)
        {
            case ControlsEnum.Confirm:
                if (selectedUnit == null && context.currentTile.CurrentUnit == null)
                {

                }
                else if (selectedUnit == null && context.currentTile.CurrentUnit != null)
                {
                    selectedUnit = context.currentTile.CurrentUnit;
                    if (selectedUnit.commander != this)
                    {
                        //render action spaces, purely visual
                        return new CommanderParseCursorOutput(false, restrictedSpaces);
                    }
                    else if (!context.currentTile.CurrentUnit.Spent)
                    {
                        //this is our unit, so we shall start using it
                        List<Action> possibleActions = selectedUnit.GetAllActionFlags();
                        return ActionManager.ParseActions(selectedUnit, possibleActions);
                    }
                }
                else if (selectedUnit != null && !selectedUnit.IsPerformingAction())
                {
                    if (ActionManager.ParseTile(selectedUnit, context.currentTile))
                    {

                    }
                    else
                    { 
                        selectedUnit.controller.RevertMaybeTeleport(selectedUnit, selectedUnit.team == Team.Player);
                        if (currentTile.CurrentUnit != null && !currentTile.CurrentUnit.Spent)
                        {
                            Debug.Log("Selected unit is not player or no action space on tile, and the current tile has a selectable unit");

                            selectedUnit = currentTile.CurrentUnit;
                            if (PlayerController.unitsEnum.MoveToUnit(selectedUnit)) //player controls this unit -> all systems go
                            {
                                PlayerController.unitsEnum.MoveToPhase(Action.Move); //set the controller to be in movement mode for the current unit
                            }
                            actionSpaces = selectedUnit.GenerateMoveSpaces(selectedUnit.team != Team.Player);
                        }
                        else
                        {
                            Debug.Log("No action space, no unit to select, no god");
                            selectedUnit = null;
                        }
                    }
                }
                break;

            case ControlsEnum.Reverse:
                if (selectedUnit != null && !selectedUnit.IsPerformingAction())
                {
                    selectedUnit.commander.RevertMaybeTeleport(selectedUnit, selectedUnit.commander == this);
                    selectedUnit = null;
                    return new CommanderParseCursorOutput(false, restrictedSpaces);
                }
                break;

            default:
                break;
        }

            //TEMPORARY:
            if (PlayerController.Current.Item1 == selectedUnit && PlayerController.Current.Item2 == Action.Attack)
            { //just skip the attack phase of the player controlled unit
                Debug.Log("Temporary automatic performance of attack upon pressing Z");
                selectedUnit.Attack(currentTile);
                selectedUnit = null;
                locked = false;
            }
    }
    #endregion

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
    /// Removes a unit from this controllers list of units.
    /// </summary>
    /// <param name="_unit">Reference to the unit to be removed.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool RemoveUnit(Unit _unit)
    {
        actableUnits.Remove(_unit);
        return Units.Remove(_unit);
    }

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

        //put business for the start of the turn
        if (ActionManager != null && ActionManager.autoTurn)
        {
            //if this controller has a behavior that automatically performs the turn, start a cascade of the units behaving
            StepTurn();
        }
    }

    /// <summary>
    /// Called by the child unit to indicate that it has finished its current action and is ready to perform the next.
    /// Checks if the unit's turn should end, and if the controller's turn should end.
    /// </summary>
    public void StepTurn() // NEEDS REDOING (units now are commanded by their actionmanager on what to do, not based on what the unitstracker says
    {
        if (!MyTurn)
        {
            return;
        }
        if (UnitsTracker.Current.QueryEndOfTurn())
        {
            Debug.Log("Unit has ended its movements");
            UnitsTracker.Current.EndActions();
            if (RetireUnit(UnitsTracker.Current))
            {
                EndTurn();
                return;
            }
        }
        Debug.Log(Team + " is stepping its turn");
        if (UnitsTracker.MoveNext())
        {
            Debug.Log(UnitsTracker.Current);
            ActionManager.ParseAction(UnitsTracker.Current);
        }
        else
        {
            EndTurn();
            return;
            //end the turn
        }
    }

    private void EndTurn()
    {
        Debug.Log(Team + " is ending its turn");
        MyTurn = false;
        _GameManager.instance.PassTurn();
    }

    #region Unit Controls
    /// <summary>
    /// Tells a specific unit to teleport back to its past tile and resets the state.
    /// </summary>
    /// <param name="_unit">The unit.</param>
    /// <param name="_teleport">Whether or not it should teleport.</param>
    public void RevertMaybeTeleport(Unit _unit, bool _teleport)
    {
        if (_teleport)
        {
            _unit.Teleport(_unit.pastTile);
        }
        _unit.ResetStates();
    }

    /// <summary>
    /// Removes a unit from the active list of units and checks if it is now empty, signalling the end of a turn. Returns true in that case.
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>True if the turn is ending, false otherwise.</returns>
    public bool RetireUnit(Unit _unit)
    {
        actableUnits.Remove(_unit);
        if (actableUnits.Count < 1)
        {
            Debug.Log("Retired a unit, and now the controller is out of units");
            return true;
        }
        Debug.Log("Retired a unit, but the controller has more");
        return false;
    }
    #endregion

};

/// <summary>
/// A list of actions that units can take. Also doubles as the order units should automatically perform their turn in.
/// </summary>
public enum Action
{
    Move,
    Attack
}
public enum Team
{
    Player,
    Enemy
}
