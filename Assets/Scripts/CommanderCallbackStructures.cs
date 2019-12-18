using System;

public struct ParseCommandCallbackContainer
{
    public Action<ParseCommandPayload, Action> parseCommandCallback;
    public ParseCommandPayload payload;
    public Action releaseCursorCallback;

    public ParseCommandCallbackContainer(Action<ParseCommandPayload, Action> _parseCommandCallback, Action _releaseCursorCallback)
    {
        parseCommandCallback = _parseCommandCallback;
        payload = new ParseCommandPayload();
        releaseCursorCallback = _releaseCursorCallback;
    }
    public void PerformCallback()
    {
        parseCommandCallback(payload, releaseCursorCallback);
    }

}
public struct ParseCommandPayload
{
    public Unit actingUnit { get; private set; }
    public CommandNames commandName { get; private set; }
    public Tile targetTile { get; private set; }
    public object[] parameters { get; private set; }

    public ParseCommandPayload(Unit _actingUnit, Tile _targetTile)
    {
        actingUnit = _actingUnit;
        commandName = CommandNames.None;
        targetTile = _targetTile;
        parameters = new object[0];
    }

    public ParseCommandPayload Initialize(CommandNames _commandName, Unit _actingUnit, Tile _targetTile, object[] _parameters)
    {
        commandName = _commandName;
        parameters = _parameters;
        actingUnit = _actingUnit;
        targetTile = _targetTile;

        return this;
    }
    public ParseCommandPayload Initialize(CommandNames _commandName, Unit _actingUnit, Tile _targetTile)
    {
        commandName = _commandName;
        actingUnit = _actingUnit;
        targetTile = _targetTile;

        return this;
    }
}
