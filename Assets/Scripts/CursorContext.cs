using System;

public struct CursorContext
{
    public Tile currentTile { get; private set; } //unit needs to know this
    public Commander cursorCommander { get; private set; }
    public Unit targetUnit { get; private set; }
    public ControlsEnum inputButton { get; private set; }
    public Action releaseCursorCallback { get; private set; }
    public Action lockCursorCallback { get; private set; }

    public CursorContext(Tile _selectedTile, Commander _currentCommander, Unit _targetUnit, ControlsEnum _inputButton, Action _releaseCursorCallback, Action _lockCursorCallback)
    {
        currentTile = _selectedTile;
        cursorCommander = _currentCommander;
        targetUnit = _targetUnit;
        inputButton = _inputButton;
        releaseCursorCallback = _releaseCursorCallback;
        lockCursorCallback = _lockCursorCallback;
    }
}