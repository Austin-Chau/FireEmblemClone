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
        Action[] callbacks;
        Action endTurnCallback;
        Tuple<string, Action> tuple;

        List<Tuple<string, Action>> listOfEntries = new List<Tuple<string, Action>>();
        foreach (ActionNames action in _actions)
        {
            Debug.Log(action);
            callbacks = new Action[] { _callbackContainer.releaseInputCallback };
            endTurnCallback =
                () => {
                    ParseCommandPayload tempPayload = new ParseCommandPayload(ActionsToCommands[action], _unit, _targetTile, callbacks, new object[0]);
                    _callbackContainer.PerformCallback(tempPayload);
                };
            tuple = new Tuple<string, Action>(ActionsToCommandMenuString[action], endTurnCallback);
            listOfEntries.Add(tuple);
        }

        callbacks = new Action[] { _callbackContainer.releaseInputCallback };
        endTurnCallback =
            () => {
                ParseCommandPayload tempPayload = new ParseCommandPayload(CommandNames.EndTurn, _unit, _targetTile, callbacks, new object[0]);
                _callbackContainer.PerformCallback(tempPayload);
            };
        tuple = new Tuple<string, Action>("Wait", endTurnCallback);
        listOfEntries.Add(tuple);

        Action reverseCallback =
            () => {
                ParseCommandPayload tempPayload = new ParseCommandPayload(CommandNames.Revert, _unit, _targetTile, callbacks, new object[0]);
                _callbackContainer.PerformCallback(tempPayload);
            };
        GameManager.instance.GUIManager.StartCommandMenu(listOfEntries, reverseCallback);
        return;
    }

    private Dictionary<ActionNames, string> ActionsToCommandMenuString = new Dictionary<ActionNames, string>()
    {
        { ActionNames.Move, "Move" },
        { ActionNames.Attack, "Attack (currently broken)"}
    };

    private Dictionary<ActionNames, CommandNames> ActionsToCommands = new Dictionary<ActionNames, CommandNames>()
    {
        { ActionNames.Move, CommandNames.InitializeMove },
        { ActionNames.Attack, CommandNames.InitializeAttack }
    };

}