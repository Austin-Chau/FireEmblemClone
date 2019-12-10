using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller
{
    public Team Team { get; private set; }

    private List<Unit> Units = new List<Unit>();
    private List<Unit> actableUnits = new List<Unit>(); //a list to keep track of which units still need to move
    public UnitsEnum unitsEnum { get; private set; }
    public ControllerBehavior behavior { get; private set; }
    public bool MyTurn { get; private set; }

    public Tuple<Unit,Action> Current
    {
        get
        {
            return unitsEnum.Current;
        }
    }

    /// <summary>
    /// Sets the team and behavior of this controller. 
    /// </summary>
    /// <param name="_team">The team</param>
    public Controller(Team _team, ControllerBehavior _behavior)
    {
        unitsEnum = new UnitsEnum();
        Team = _team;
        behavior = _behavior;
    }

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
        _unit.InitializeUnit(_spawnTile, Team, this);
        unitsEnum.UpdateUnits(Units.ToArray());
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
        unitsEnum.UpdateUnits(Units.ToArray());
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

    public void PerformTurn()
    {
        MyTurn = true;
        actableUnits = new List<Unit>();
        foreach (Unit unit in Units)
        {
            unit.ResetStates();
            actableUnits.Add(unit);
        }
        unitsEnum.Initialize(Units.ToArray());

        //put business for the start of the turn
        if (behavior != null && behavior.autoTurn)
        {
            //if this controller has a behavior, start a cascade of the units acting
            StepTurn();
        }
    }

    /// <summary>
    /// Called by the child unit to indicate that it has finished its currnent action and is ready to perform the next
    /// </summary>
    public void StepTurn()
    {
        Debug.Log(Team+ " performs step turn");
        if (unitsEnum.MoveNext(behavior.autoTurn))
        {
            Debug.Log("parsing action");
            Debug.Log(unitsEnum.Current);
            behavior.ParseAction(unitsEnum.Current);
        }
        else
        {
            EndTurn();
            //end the turn
        }
    }

    private void EndTurn()
    {
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
        unitsEnum.MoveToPhase(Action.Move);
    }

    /// <summary>
    /// Removes a unit from the active list of units and checks if it is now empty, signalling the end of a turn. Returns true in that case.
    /// </summary>
    /// <param name="_unit"></param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool RetireUnit(Unit _unit)
    {
        bool successful = actableUnits.Remove(_unit);
        if (actableUnits.Count < 1)
        {
            EndTurn();
            return true;
        }
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
public class UnitsEnum
{
    private Unit[] Units;
    int position = -1; //which unit in the Units array it is the turn of
    int subPosition = -1; //the unit's phase
    int subLength = Enum.GetNames(typeof(Action)).Length; //the number of actions

    /// <summary>
    /// Resets to the initial state.
    /// </summary>
    public void Reset()
    {
        position = 0;
        subPosition = -1;
    }

    public void Initialize(Unit[] _units)
    {
        Units = _units;
        Reset();
    }

    public void UpdateUnits(Unit[] _units)
    {
        Units = _units;
    }

    /// <summary>
    /// Steps an action phase, and optionally a unit if subPosition exceeds the number of actions.
    /// </summary>
    /// <param name="shiftUnit">Whether or not the next unit should automatically be moved to.</param>
    /// <returns>Returns true if position is in bounds of Units, false otherwise.</returns>
    public bool MoveNext(bool shiftUnit)
    {
        Debug.Log(subPosition);
        Debug.Log(position);
        subPosition++;
        if (subPosition >= subLength)
        {
            subPosition = 0;
            if (shiftUnit)
            {
                position++;
            }
        }
        Debug.Log(position >= 0);
        Debug.Log(position < Units.Length);
        return (position >= 0 && position < Units.Length);
    }
    public bool MoveBack(bool shiftUnit)
    {
        subPosition--;
        if (subPosition < 0)
        {
            subPosition = 0;
            if (shiftUnit)
            {
                position--;
            }
        }
        return (position >= 0 && position < Units.Length);
    }

    /// <summary>
    /// Move to the next unit, skips all action phases not yet done, resets subPosition.
    /// </summary>
    /// <returns>Returns true if position is in bounds of Units, false otherwise.</returns>
    public bool MoveToNextUnit()
    {
        subPosition = 0;
        position++;
        return (position < Units.Length);
    }

    /// <summary>
    /// Switches position to a specific unit
    /// </summary>
    /// <param name="_unit">A reference to the unit to move to</param>
    /// <returns>True if successfuly moved to the unit, false otherwise (if the unit is not in Units)</returns>
    public bool MoveToUnit(Unit _unit)
    {
        int pos = Array.IndexOf(Units, _unit);
        if (pos < 0)
        {
            return false;
        }
        position = pos;
        subPosition = 0;
        return true;
    }

    /// <summary>
    /// Switches subPosition to a specific Action
    /// </summary>
    /// <param name="_action">The action to move to</param>
    public void MoveToPhase(Action _action)
    {
        subPosition = (int)_action;
    }

    /// <summary>
    /// Returns a tuple containing a unit that should act, and then the action it should perform.
    /// </summary>
    public Tuple<Unit,Action> Current
    {
        get
        {
            if (subPosition < 0)
            {
                return new Tuple<Unit, Action>(Units[position], (Action)0);
            }
            return new Tuple<Unit, Action>(Units[position],(Action)subPosition);
        }
    }

}
