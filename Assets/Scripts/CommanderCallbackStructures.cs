using System;
using UnityEngine;
namespace ParseCommandCallback
{

    public class ParseCommandPayload
    {
        public Unit actingUnit { get; private set; }
        public CommandNames commandName { get; private set; }
        public Tile targetTile { get; private set; }
        public object[] parameters { get; private set; }
        public Action[] callbacks { get; private set; }

        public ParseCommandPayload(CommandNames _commandName, Unit _actingUnit, Tile _targetTile, Action[] _callbacks)
        {
            commandName = _commandName;
            actingUnit = _actingUnit;
            targetTile = _targetTile;
            callbacks = _callbacks;
        }
        public void PerformCallbacks()
        {
            if (callbacks == null)
            {
                return;
            }

            foreach (Action action in callbacks)
            {
                action();
            }
        }

    }
}
