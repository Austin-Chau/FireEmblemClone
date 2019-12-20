using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ParseCommandCallback;


public abstract class ActionManager
{
    /// <summary>
    /// Decides if this controller should automatically move to the next unit
    /// </summary>
    public bool autoTurn;

    /// <summary>
    /// Given a unit and a list of actions that unit can take, does whatever behavior we wish.
    /// AI will pick one, player will talk with the GUI to pick one.
    /// </summary>
    public abstract void DecideOnACommand(Unit _unit, Tile _targetTile, List<ActionNames> _actions, ParseCommandCallbackContainer _callbackContainer);
}

public class BasicEnemy : ActionManager
{ //This AI is rudimentary, it just moves to a random tile in the tree
    public BasicEnemy()
    {
        autoTurn = true;
    }

    public override void DecideOnACommand(Unit _unit, Tile _targetTile, List<ActionNames> _actions, ParseCommandCallbackContainer _callbackContainer)
    {
        return;
    }

}

public class PlayerBehavior : ActionManager
{ //The behaviors for what a player's unit should do when the controller interacts with it
    public PlayerBehavior()
    {
        autoTurn = false;
    }

    public override void DecideOnACommand(Unit _unit, Tile _targetTile, List<ActionNames> _actions, ParseCommandCallbackContainer _callbackContainer)
    {
        /*
         * Goal: send this list of actions to the GUI.
         */
        foreach (ActionNames action in _actions)
        {
            Debug.Log(action);
        }
        if (_actions.Contains(ActionNames.Move))
        {
            _callbackContainer.payload.Initialize(CommandNames.InitializeMove, _unit, _targetTile, new Action[] {_callbackContainer.releaseCursorCallback });
            Debug.Log(_unit);
            Debug.Log(_callbackContainer.payload.actingUnit);
            _callbackContainer.PerformCallback();
        }
        else
        {
            _callbackContainer.payload.Initialize(CommandNames.EndTurn, _unit, _targetTile, new Action[] { _callbackContainer.releaseCursorCallback });
            _callbackContainer.PerformCallback();
        }
        return;
    }

}