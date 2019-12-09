using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller
{
    public Team Team { get; private set; }

    protected List<Unit> Units;
    protected UnitsEnum unitsEnum = new UnitsEnum();
    protected ControllerBehavior behavior; //

    /// <summary>
    /// Sets the team.
    /// </summary>
    /// <param name="_team">The team</param>
    public Controller(Team _team, ControllerBehavior _behavior)
    {
        Team = _team;
        behavior = _behavior;
    }

    /// <summary>
    /// Adds a unit to this controllers list of units while also initializing it.
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
        return true;
    }

    /// <summary>
    /// Removes a unit from this controllers list of units.
    /// </summary>
    /// <param name="_unit">Reference to the unit to be removed.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool RemoveUnit(Unit _unit)
    {
        return Units.Remove(_unit);
    }

    public void PerformTurn()
    {
        unitsEnum.Initialize(Units.ToArray());

        //put business for the start of the turn

        if (behavior != null)
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
        if (unitsEnum.MoveNext())
        {
            behavior.ParseAction(unitsEnum.Current);
        }
    }

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
public class UnitsEnum : IEnumerator
{
    private Unit[] Units;
    int position = -1; //which unit in the Units array it is the turn of
    int subPosition = -1; //the unit's phase
    int subLength = Action.GetNames(typeof(Action)).Length; //the number of actions

    /// <summary>
    /// Resets to the initial state.
    /// </summary>
    public void Reset()
    {
        position = -1;
        subPosition = -1;
    }

    public void Initialize(Unit[] _units)
    {
        Units = _units;
        Reset();
    }

    /// <summary>
    /// Steps an action phase, and a unit if subPosition exceeds the number of actions
    /// </summary>
    /// <returns>Returns true if position is in bounds of Units, false otherwise.</returns>
    public bool MoveNext()
    {
        subPosition++;
        if (subPosition > subLength)
        {
            subPosition = 0;
            position++;
        }
        return (position < Units.Length);
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
            return new Tuple<Unit, Action>(Units[position],(Action)subPosition);
        }
    }

    object IEnumerator.Current
    {
        get
        {
            return Current;
        }
    }

}