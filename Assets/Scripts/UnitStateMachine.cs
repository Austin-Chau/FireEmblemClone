using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MenuStates
{
    Starting,
    MovedButNotActed,
    ActedButNotMoved,
    Spent
}

public interface IUnitState
{
    void Enter();               //behavior when entering the state
    void Update();              //when in the middle of the state
    void Exit();                //when leaving the state
    void CursorSelect();        //when the player selects this unit
    //these three might not be necessarily be needed on the state:
    void CursorHover();         //when the player hovers over this unit
    void CursorHoverUpdate();   //when the player is hovering over this unit
    void CursorHoverExit();     //when the player stops hovering
}

public class UnitStateMachine
{
    public IUnitState currentState;

    public void ChangeState(IUnitState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    /// <summary>
    /// Updating method for a unit, based on its current state.
    /// </summary>
    /// <param name="cursorHovering">whether or not the cursor is hovering on this unit</param>
    public void Update(bool cursorHovering)
    {
        if (currentState != null)
        {
            currentState.Update();
            if (cursorHovering)
            {
                currentState.CursorHoverUpdate();
            }
        }
    }
}

/// <summary>
/// The state of a unit when it has done nothing at all yet.
/// </summary>
public class Starting : IUnitState
{
    Unit parent;
    public Starting(Unit parent) { this.parent = parent; }

    public void Enter()
    {

    }
    public void Update()
    {

    }
    public void Exit()
    {

    }
    public void CursorHover()
    {

    }
    public void CursorHoverUpdate()
    {

    }
    public void CursorHoverExit()
    {

    }
    /// <summary>
    /// Pops up a menu of options
    /// </summary>
    public void CursorSelect()
    {
        
    }
}

public class MovedButNotActed : IUnitState
{
    Unit parent;
    public MovedButNotActed(Unit parent) { this.parent = parent; }
    public void Enter()
    {

    }
    public void Update()
    {

    }
    public void Exit()
    {

    }
    public void CursorHover()
    {

    }
    public void CursorHoverUpdate()
    {

    }
    public void CursorHoverExit()
    {

    }
    public void CursorSelect()
    {

    }
}

public class ActedButNotMoved : IUnitState
{
    Unit parent;
    public ActedButNotMoved(Unit parent) { this.parent = parent; }
    public void Enter()
    {

    }
    public void Update()
    {

    }
    public void Exit()
    {

    }
    public void CursorHover()
    {

    }
    public void CursorHoverUpdate()
    {

    }
    public void CursorHoverExit()
    {

    }
    public void CursorSelect()
    {

    }
}

/// <summary>
/// The state of a unit after it has exhausted all actions it can perform on its own.
/// </summary>
public class Spent : IUnitState
{
    Unit parent;
    public Spent(Unit parent) { this.parent = parent; }
    public void Enter()
    {

    }
    public void Update()
    {

    }
    public void Exit()
    {

    }
    public void CursorHover()
    {

    }
    public void CursorHoverUpdate()
    {

    }
    public void CursorHoverExit()
    {

    }
    public void CursorSelect()
    {

    }
}
