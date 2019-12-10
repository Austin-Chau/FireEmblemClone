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
{
    public BasicEnemy()
    {
        autoTurn = true;
    }
    protected override void Move(Unit _unit)
    {
        Debug.Log("trying to move a unit");
        Tile tile;
        //don't judge
        if (_unit.currentTile.GetAdjacentTile(AdjacentDirection.Left) != null)
            tile = _unit.currentTile.GetAdjacentTile(AdjacentDirection.Left).GetAdjacentTile(AdjacentDirection.Left);
        else
            tile = _unit.currentTile;
        if (tile == null)
            tile = _unit.currentTile;
        _unit.MetaMove(tile);
    }

    protected override void Attack(Unit _unit)
    {
        _unit.StartActPhase();
        _unit.EndActPhase();
    }
}

public class PlayerBehavior : ControllerBehavior
{
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
        _unit.StartActPhase();
    }
}