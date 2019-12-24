using System;
using UnityEngine;
namespace ParseCommandCallback
{
    public class ParseCommandCallbackContainer
    {
        public Action<ParseCommandPayload> parseCommand { get; private set; }
        public ParseCommandPayload payload { get; private set; }
        public Action releaseCursorCallback { get; private set; }

        public ParseCommandCallbackContainer(Action<ParseCommandPayload> _parseCommand, Action _releaseCursorCallback)
        {
            parseCommand = _parseCommand;
            releaseCursorCallback = _releaseCursorCallback;
        }
        public void PerformCallback()
        {
            parseCommand(payload);
        }
        public void GeneratePayload(CommandNames _commandName, Unit _actingUnit, Tile _targetTile, Action[] _callbacks, object[] _parameters)
        {
            payload = new ParseCommandPayload(_commandName, _actingUnit, _targetTile, _callbacks, _parameters);
        }

    }
    public class ParseCommandPayload
    {
        public Unit actingUnit { get; private set; }
        public CommandNames commandName { get; private set; }
        public Tile targetTile { get; private set; }
        public object[] parameters { get; private set; }
        public Action[] callbacks { get; private set; }

        public ParseCommandPayload(CommandNames _commandName, Unit _actingUnit, Tile _targetTile, Action[] _callbacks, object[] _parameters)
        {
            commandName = _commandName;
            parameters = _parameters;
            actingUnit = _actingUnit;
            targetTile = _targetTile;
            callbacks = _callbacks;
        }
        public void PerformCallbacks()
        {
            foreach (Action action in callbacks)
            {
                action();
            }
        }

    }
}
