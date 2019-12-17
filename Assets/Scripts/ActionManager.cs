using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class ActionManager
{
    /// <summary>
    /// Decides if this controller should automatically move to the next unit
    /// </summary>
    public bool autoTurn;

    /// <summary>
    /// Given a unit and a tile, returns whether or not an action space was parsed.
    /// Cases:
    /// (0: no actionspace on the tile)
    /// (1: unit is out of possible actions and is now spent)
    /// (2: unit can still perform actions).
    /// Also performs the action of that action space.
    /// </summary>
    public void ParseTile(Unit _unit, CursorContext _context, Action<bool, CursorContext> _commanderCallback)
    {
        if (!_unit.actionSpaces.ContainsKey(_context.currentTile) || _unit.actionSpaces[_context.currentTile] == null)
        {
            Debug.Log("Attempted to perform an action and failed. Releasing the cursor and parsing the space as a selection of a new unit.");
            _context.releaseCursorCallback();
            _commanderCallback(false, _context);
        }
        else
        {
            Debug.Log("Performing an action space.");
            _unit.PerformAction(_context, _commanderCallback);
        }

    }

    /// <summary>
    /// Given a unit and a list of actions that unit can take, does whatever behavior we wish.
    /// AI will pick one, player will talk with the GUI to pick one.
    /// </summary>
    public abstract void ParseActions(Unit _unit, List<ActionNames> _actions, ParseActionsCallbackContainer _callbackContainer);
}

public class BasicEnemy : ActionManager
{ //This AI is rudimentary, it just moves to a random tile in the tree
    public BasicEnemy()
    {
        autoTurn = true;
    }

    public override void ParseActions(Unit _unit, List<ActionNames> _actions, ParseActionsCallbackContainer _callbackContainer)
    {
        _callbackContainer.PerformActions();
        return;
    }

}

public class PlayerBehavior : ActionManager
{ //The behaviors for what a player's unit should do when the controller interacts with it
    public PlayerBehavior()
    {
        autoTurn = false;
    }

    public override void ParseActions(Unit _unit, List<ActionNames> _actions, ParseActionsCallbackContainer _callbackContainer)
    {
        //Instead of GUI rn, just automatically starts movement or ends the unit's turn if it can't move
        foreach (ActionNames action in _actions)
        {
            Debug.Log(action);
        }
        if (_actions.Contains(ActionNames.Move))
        {
            _unit.GenerateActSpaces(ActionNames.Move);
        }
        else
        {
            _unit.EndActions();
            _callbackContainer.PerformActions();
        }

        _callbackContainer.unlockCursorCallback();
        return;
    }

}