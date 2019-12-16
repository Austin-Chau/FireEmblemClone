using System;

public struct CursorContext
{
    public Tile currentTile { get; private set; }
    public Commander cursorCommander { get; private set; }
    public Unit targetUnit { get; private set; }
    public ControlsEnum inputButton { get; private set; }

    public CursorContext(Tile _selectedTile, Commander _currentCommander, Unit _targetUnit, ControlsEnum _inputButton)
    {
        currentTile = _selectedTile;
        cursorCommander = _currentCommander;
        targetUnit = _targetUnit;
        inputButton = _inputButton;
    }
}
