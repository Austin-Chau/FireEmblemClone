using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UnitsTracker
{
    private Unit[] Units;
    int position = -1; //which unit in the Units array it is the turn of

    /// <summary>
    /// Resets to the initial state.
    /// </summary>
    public void Reset()
    {
        position = -1;
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
    public bool MoveNext()
    {
        position++;
        return (position >= 0 && position < Units.Length);
    }
    public bool MoveBack()
    {
        position--;
        return (position >= 0 && position < Units.Length);
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
        return true;
    }

    /// <summary>
    /// Returns the current unit.
    /// </summary>
    public Unit Current
    {
        get
        {
            return Units[position];
        }
    }

}