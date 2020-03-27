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
    public abstract void DecideOnACommand(Unit _unit, Tile _targetTile, List<ActionNames> _actions, Action<ParseCommandPayload> _parseCommand, Action _endGameState);
}

/*
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

}*/

public class PlayerBehavior : ActionManager
{ //The behaviors for what a player's unit should do when the controller interacts with it
    public PlayerBehavior()
    {
        autoTurn = false;
    }

    public override void DecideOnACommand(Unit _unit, Tile _targetTile, List<ActionNames> _actions, Action<ParseCommandPayload> _parseCommand, Action _endGameState)
    {
        Action[] callbacks = new Action[] { }; //callbacks for after the gamemanager parses the command
        Action selectedAction; // The action that is performed when the menu option is selected
        Tuple<string, Action> tuple; //tuple to store the menu entry

        //Now for every possible action that was passed
        List<Tuple<string, Action>> listOfEntries = new List<Tuple<string, Action>>();
        foreach (ActionNames action in _actions)
        {
            //actions = new Action[] { _endGameState };
            selectedAction =
                () => {
                    _endGameState();
                    ParseCommandPayload tempPayload = new ParseCommandPayload(ActionsToCommands[action], _unit, _targetTile, callbacks);
                    _parseCommand(tempPayload);
                };
            tuple = new Tuple<string, Action>(ActionsToCommandMenuString[action], selectedAction);
            listOfEntries.Add(tuple);
        }

        //Now for the wait option, which appears no matter what
        //actions = new Action[] { _endGameState };
        selectedAction =
            () => {
                _endGameState();
                ParseCommandPayload tempPayload = new ParseCommandPayload(CommandNames.EndTurn, _unit, _targetTile, callbacks);
                _parseCommand(tempPayload);
            };
        tuple = new Tuple<string, Action>("Wait", selectedAction);

        listOfEntries.Add(tuple);

        //Now for what happens when the player backs out of the menu
        //actions = new Action[] { _endGameState };
        Action reverseCallback =
            () => {
                _endGameState();
                ParseCommandPayload tempPayload = new ParseCommandPayload(CommandNames.Revert, _unit, _targetTile, callbacks);
                _parseCommand(tempPayload);
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