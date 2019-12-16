using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public struct CommanderParseCursorOutput
{
    public bool inMenu;
    public List<Tile> restrictedSpaces;
    public CommanderParseCursorOutput(bool _inMenu, List<Tile> _restrictedSpaces)
    {
        inMenu = _inMenu;
        restrictedSpaces = _restrictedSpaces;
    }
}