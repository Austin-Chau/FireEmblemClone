using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class ControllerBehavior
{
    /// <summary>
    /// Decides if this controller should automatically move to the next unit
    /// </summary>
    public bool autoTurn;

    /// <summary>
    /// Given a tuple of a unit and an action, passes to the proper behavior set on the object
    /// </summary>
    /// <param name="tuple"></param>
    public void ParseAction(Tuple<Unit, Action> tuple)
    {
        switch (tuple.Item2)
        {
            case Action.Move:
                Move(tuple.Item1);
                break;
            case Action.Attack:
                Attack(tuple.Item1);
                break;
            default:
                break;
        }
    }

    protected abstract void Move(Unit _unit);
    protected abstract void Attack(Unit _unit);
}

public class BasicEnemy : ControllerBehavior
{ //This AI is rudimentary, it just moves to a random tile in the tree
    public BasicEnemy()
    {
        autoTurn = true;
    }
    protected override void Move(Unit _unit)
    {
        Debug.Log("Trying to move an enemy.");
        Tile tile;
        tile = Unit.GetRandomTileFromMoveTree(_unit.GetMoveTree());
        _unit.MetaMove(tile);
    }

    protected override void Attack(Unit _unit)
    {
        _unit.Attack((Tile)null);
    }
}

public class PlayerBehavior : ControllerBehavior
{ //The behaviors for what a player's unit should do when the controller interacts with it
    public PlayerBehavior()
    {
        autoTurn = false;
    }

    protected override void Move(Unit _unit)
    {
        _unit.MetaMove(_GameManager.instance.Cursor.currentTile);
    }

    protected override void Attack(Unit _unit)
    {
        _unit.EraseSpaces();
    }
}