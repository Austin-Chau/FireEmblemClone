using System.Collections;
using System.Collections.Generic;
using System;

public abstract class ControllerBehavior
{
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
    protected override void Move(Unit _unit)
    {
        _unit.MetaMove(_unit.currentTile.GetAdjacentTile(AdjacentDirection.Left));
    }

    protected override void Attack(Unit _unit)
    {

    }
}